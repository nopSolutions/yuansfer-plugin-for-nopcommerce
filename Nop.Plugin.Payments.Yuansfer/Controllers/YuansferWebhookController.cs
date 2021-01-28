using System;
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
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly YuansferPaymentSettings _yuansferPaymentSettings;

        #endregion

        #region Ctor

        public YuansferWebhookController(
            ILogger logger,
            IPaymentPluginManager paymentPluginManager,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            YuansferPaymentSettings yuansferPaymentSettings
        )
        {
            _logger = logger;
            _paymentPluginManager = paymentPluginManager;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _yuansferPaymentSettings = yuansferPaymentSettings;
        }

        #endregion

        #region Methods

        [HttpPost]
        public IActionResult SecurePayWebhook(SecurePayWebhookRequest request)
        {
            if (!(_paymentPluginManager.LoadPluginBySystemName(Defaults.SystemName) is YuansferPaymentProcessor processor) || !_paymentPluginManager.IsPluginActive(processor))
                throw new NopException("Yuansfer module cannot be loaded");

            if (!ModelState.IsValid)
                return Ok();

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
            {
                _logger.Error($"{Defaults.SystemName}: The payment transaction failed. The tranaction status is '{request.Status}'. The payload: {requestPayload}");
                return Ok();
            }

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

            var verifySign = CommonHelpers
                .GenerateSign(_yuansferPaymentSettings.ApiToken, signingParams);

            if (verifySign != request.VerifySign)
            {
                _logger.Error($"{Defaults.SystemName}: Invalid verification of the payment transaction. The expected signature is '{verifySign}', but found '{request.VerifySign}'. The payload: {requestPayload}");
                return Ok();
            }

            if (!Guid.TryParse(request.Reference, out var orderGuid))
            {
                _logger.Error($"{Defaults.SystemName}: Invalid parse the order guid '{request.Reference}'. The payload: {requestPayload}");
                return Ok();
            }

            var order = _orderService.GetOrderByGuid(orderGuid);
            if (order == null)
            {
                _logger.Error($"{Defaults.SystemName}: The order not found with guid '{orderGuid}'. The payload: {requestPayload}");
                return Ok();
            }

            if (!_orderProcessingService.CanMarkOrderAsPaid(order))
                _logger.Error($"{Defaults.SystemName}: The order with id '{order.Id}' already marked as paid. The payload: {requestPayload}");
            else
            {
                _orderService.InsertOrderNote(new OrderNote
                {
                    OrderId = order.Id,
                    Note = $"The payment transaction was successful. The payload: {requestPayload}",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                order.CaptureTransactionId = request.TransactionNo;

                _orderService.UpdateOrder(order);
                _orderProcessingService.MarkOrderAsPaid(order);
            }

            return Ok();
        }

        #endregion
    }
}
