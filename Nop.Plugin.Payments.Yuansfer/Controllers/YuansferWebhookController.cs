using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.Yuansfer.Extensions;
using Nop.Plugin.Payments.Yuansfer.Models;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;

namespace Nop.Plugin.Payments.Yuansfer.Controllers
{
    public class YuansferWebhookController : Controller
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly YuansferPaymentSettings _yuansferPaymentSettings;

        #endregion

        #region Ctor

        public YuansferWebhookController(ILogger logger,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            IPaymentPluginManager paymentPluginManager,
            YuansferPaymentSettings yuansferPaymentSettings)
        {
            _logger = logger;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _paymentPluginManager = paymentPluginManager;
            _yuansferPaymentSettings = yuansferPaymentSettings;
        }

        #endregion

        #region Methods

        [HttpPost]
        public async Task<IActionResult> SecurePayWebhook(SecurePayWebhookRequest request)
        {
            try
            {
                var plugin = await _paymentPluginManager.LoadPluginBySystemNameAsync(Defaults.SystemName);
                if (plugin is not YuansferPaymentProcessor processor || !_paymentPluginManager.IsPluginActive(processor))
                    throw new NopException("Module could not be loaded");

                if (!ModelState.IsValid)
                    throw new NopException("Request is invalid");

                var requestPayload = @$"
                    Status: {request.Status}
                    Transaction number: {request.TransactionNo}
                    Order guid: {request.Reference}
                    Amount: {request.Amount}
                    Currency: {request.Currency}
                    Settlement currency: {request.SettleCurrency}
                    Time: {request.Time}
                ";

                if (request.Status != "success")
                    throw new NopException($"The payment transaction failed. The tranaction status is '{request.Status}'. The payload: {requestPayload}");

                var signingParams = new[]
                {
                    ("amount", request.Amount),
                    ("currency", request.Currency),
                    ("reference", request.Reference),
                    ("settleCurrency", request.SettleCurrency),
                    ("status", request.Status),
                    ("time", request.Time),
                    ("transactionNo", request.TransactionNo),
                };

                var verifySign = CommonHelpers.GenerateSign(_yuansferPaymentSettings.ApiToken, signingParams);
                if (verifySign != request.VerifySign)
                    throw new NopException($"Invalid verification of the payment transaction. The expected signature is '{verifySign}', but found '{request.VerifySign}'. The payload: {requestPayload}");

                if (!Guid.TryParse(request.Reference, out var orderGuid))
                    throw new NopException($"Invalid parse the order guid '{request.Reference}'. The payload: {requestPayload}");

                var order = await _orderService.GetOrderByGuidAsync(orderGuid)
                    ?? throw new NopException($"The order not found with guid '{orderGuid}'. The payload: {requestPayload}");

                if (!_orderProcessingService.CanMarkOrderAsPaid(order))
                    throw new NopException($"The order with id '{order.Id}' already marked as paid. The payload: {requestPayload}");

                await _orderService.InsertOrderNoteAsync(new OrderNote
                {
                    OrderId = order.Id,
                    Note = $"The payment transaction was successful. The payload: {requestPayload}",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                order.CaptureTransactionId = request.TransactionNo;
                await _orderService.UpdateOrderAsync(order);
                await _orderProcessingService.MarkOrderAsPaidAsync(order);
            }
            catch (Exception exception)
            {
                await _logger.ErrorAsync($"{Defaults.SystemName} webhook error: {exception.Message}", exception);
            }

            return Ok();
        }

        #endregion
    }
}
