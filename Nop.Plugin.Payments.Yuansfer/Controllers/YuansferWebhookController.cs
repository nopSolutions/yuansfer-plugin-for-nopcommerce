using System;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.Yuansfer.Extensions;
using Nop.Plugin.Payments.Yuansfer.Models;
using Nop.Services.Orders;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.Yuansfer.Controllers
{
    public class YuansferWebhookController : BasePaymentController
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly YuansferPaymentSettings _yuansferPaymentSettings;

        #endregion

        #region Ctor

        public YuansferWebhookController(
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            YuansferPaymentSettings yuansferPaymentSettings
        )
        {
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _yuansferPaymentSettings = yuansferPaymentSettings;
        }

        #endregion

        #region Methods

        [HttpPost]
        public IActionResult SecurePayWebhook(SecurePayWebhookRequest request)
        {
            if (!ModelState.IsValid)
                return Ok();

            if (request.Status != "success")
                return Ok();

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
                return Ok();

            if (!Guid.TryParse(request.Reference, out var orderGuid))
                return Ok();

            var order = _orderService.GetOrderByGuid(orderGuid);
            if (order == null)
                return Ok();

            if (_orderProcessingService.CanMarkOrderAsPaid(order))
            {
                order.CaptureTransactionId = request.TransactionNo;

                _orderService.UpdateOrder(order);
                _orderProcessingService.MarkOrderAsPaid(order);
            }

            return Ok();
        }

        #endregion
    }
}
