using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.Yuansfer.Extensions;
using Nop.Plugin.Payments.Yuansfer.Models;
using Nop.Plugin.Payments.Yuansfer.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;

namespace Nop.Plugin.Payments.Yuansfer
{
    /// <summary>
    /// Represents the Yuansfer payment processor
    /// </summary>
    public class YuansferPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly YuansferApi _yuansferApi;
        private readonly CurrencySettings _currencySettings;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ICurrencyService _currencyService;
        private readonly IOrderService _orderService;
        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
        private readonly IProductService _productService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly ISettingService _settingService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;
        private readonly YuansferService _yuansferService;
        private readonly YuansferPaymentSettings _yuansferPaymentSettings;

        #endregion

        #region Ctor

        public YuansferPaymentProcessor(
            YuansferApi checkoutApi,
            CurrencySettings currencySettings,
            IActionContextAccessor actionContextAccessor,
            ICurrencyService currencyService,
            IOrderService orderService,
            ILocalizationService localizationService,
            IPaymentService paymentService,
            IProductService productService,
            IProductAttributeFormatter productAttributeFormatter, 
            ISettingService settingService,
            IHttpContextAccessor httpContextAccessor,
            IUrlHelperFactory urlHelperFactory,
            IWorkContext workContext,
            IWebHelper webHelper,
            YuansferService yuansferService,
            YuansferPaymentSettings yuansferPaymentSettings
        )
        {
            _yuansferApi = checkoutApi;
            _currencySettings = currencySettings;
            _actionContextAccessor = actionContextAccessor;
            _currencyService = currencyService;
            _orderService = orderService;
            _localizationService = localizationService;
            _paymentService = paymentService;
            _productService = productService;
            _productAttributeFormatter = productAttributeFormatter;
            _settingService = settingService;
            _httpContextAccessor = httpContextAccessor;
            _urlHelperFactory = urlHelperFactory;
            _workContext = workContext;
            _webHelper = webHelper;
            _yuansferService = yuansferService;
            _yuansferPaymentSettings = yuansferPaymentSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult();
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            if (postProcessPaymentRequest is null)
                throw new ArgumentNullException(nameof(postProcessPaymentRequest));

            var storeCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
            if (storeCurrency == null)
                throw new NopException("Primary store currency is not set");

            var order = postProcessPaymentRequest.Order;
            var orderCustomValues = _paymentService.DeserializeCustomValues(order);

            var paymentChannelKey = _localizationService.GetResource("Plugins.Payments.Yuansfer.PaymentChannel.Key");
            if (!orderCustomValues.TryGetValue(paymentChannelKey, out var vendor))
                throw new NopException("The payment channel is not set");

            var goods = new List<object>();
            var orderItems = _orderService.GetOrderItems(order.Id);
            foreach (var item in orderItems)
            {
                var product = _productService.GetProductById(item.ProductId);
                if (product == null)
                    throw new InvalidOperationException("Cannot get the product.");

                var productName = string.Empty;
                if (string.IsNullOrEmpty(item.AttributesXml))
                    productName = product.Name;
                else
                {
                    var customer = _workContext.CurrentCustomer;
                    var attributeInfo = _productAttributeFormatter
                        .FormatAttributes(product, item.AttributesXml, customer, ", ");

                    productName = $"{product.Name} ({attributeInfo})";
                }

                goods.Add(new { goods_name = productName, quantity = item.Quantity.ToString() });
            }

            var currentRequestProtocol = _webHelper.CurrentRequestProtocol;
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var callbackUrl = urlHelper.RouteUrl(Defaults.CheckoutCompletedRouteName, new { orderId = order.Id }, currentRequestProtocol);
            var ipnUrl = urlHelper.RouteUrl(Defaults.SecurePayWebhookRouteName, null, currentRequestProtocol);

            var request = new SecurePayRequest
            {
                MerchantId = _yuansferPaymentSettings.MerchantId,
                StoreId = _yuansferPaymentSettings.StoreId,
                Vendor = vendor.ToString(),
                Terminal = "ONLINE",
                Amount = order.OrderTotal.ToString(CultureInfo.InvariantCulture),
                Currency = storeCurrency.CurrencyCode,
                SettleCurrency = storeCurrency.CurrencyCode,
                CallbackUrl = callbackUrl,
                IpnUrl = ipnUrl,
                GoodsInfo = JsonConvert.SerializeObject(goods),
                Reference = order.OrderGuid.ToString()
            };

            var signingParams = new[]
            {
                ("amount", request.Amount),
                ("callbackUrl", request.CallbackUrl),
                ("currency", request.Currency),
                ("goodsInfo", request.GoodsInfo),
                ("ipnUrl", request.IpnUrl),
                ("merchantNo", request.MerchantId),
                ("reference", request.Reference),
                ("storeNo", request.StoreId),
                ("terminal", request.Terminal),
                ("vendor", request.Vendor),
                ("settleCurrency", request.SettleCurrency),
            };
            request.VerifySign = CommonHelpers.GenerateSign(_yuansferPaymentSettings.ApiToken, signingParams);

            var response = _yuansferApi.SecurePayAsync(request).GetAwaiter().GetResult();
            if (response?.Payload == null)
                throw new NopException($"Error when calling Yuansfer secure pay endpoint. Code: {response?.Code}, Message: {response?.Message}.");

            _httpContextAccessor.HttpContext.Response.Redirect(response.Payload.CashierUrl);
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _paymentService.CalculateAdditionalFee(cart,
                _yuansferPaymentSettings.AdditionalFee, _yuansferPaymentSettings.AdditionalFeePercentage);
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            return new CapturePaymentResult { Errors = new[] { "Capture method not supported" } };
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            if (refundPaymentRequest is null)
                throw new ArgumentNullException(nameof(refundPaymentRequest));

            if (!_yuansferService.IsConfigured())
                return new RefundPaymentResult { Errors = new[] { _localizationService.GetResource("Plugins.Payments.Yuansfer.IsNotConfigured") } };

            var storeCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
            if (storeCurrency == null)
                return new RefundPaymentResult { Errors = new[] { "Primary store currency is not set" } };

            var request = new CreateRefundRequest
            {
                MerchantId = _yuansferPaymentSettings.MerchantId,
                StoreId = _yuansferPaymentSettings.StoreId,
                RefundAmount = refundPaymentRequest.AmountToRefund.ToString(CultureInfo.InvariantCulture),
                Currency = storeCurrency.CurrencyCode,
                SettleCurrency = storeCurrency.CurrencyCode,
                TransactionId = refundPaymentRequest.Order.CaptureTransactionId
            };

            var signingParams = new[]
            {
                ("refundAmount", request.RefundAmount),
                ("currency", request.Currency),
                ("merchantNo", request.MerchantId),
                ("storeNo", request.StoreId),
                ("settleCurrency", request.SettleCurrency),
                ("transactionNo", request.TransactionId),
            };
            request.VerifySign = CommonHelpers.GenerateSign(_yuansferPaymentSettings.ApiToken, signingParams);

            var response = _yuansferApi.CreateRefundAsync(request).GetAwaiter().GetResult();
            if (response?.Payload == null)
                return new RefundPaymentResult { Errors = new[] { $"Error when calling Yuansfer secure pay endpoint. Code: {response?.Code}, Message: {response?.Message}." } };

            if (response.Payload.Status != "success")
                return new RefundPaymentResult { Errors = new[] { "Order refund is invalid" } };

            return new RefundPaymentResult
            {
                NewPaymentStatus = refundPaymentRequest.IsPartialRefund
                    ? PaymentStatus.PartiallyRefunded
                    : PaymentStatus.Refunded
            };
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            return new VoidPaymentResult { Errors = new[] { "Void method not supported" } };
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //let's ensure that at least 5 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                return false;

            return true;
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            var errors = new List<string>();

            if (!_yuansferService.IsConfigured())
                errors.Add(_localizationService.GetResource("Plugins.Payments.Yuansfer.IsNotConfigured"));

            if (!form.TryGetValue(nameof(PaymentInfoModel.PaymentChannel), out var paymentChannel) ||
                StringValues.IsNullOrEmpty(paymentChannel) || 
                !_yuansferService.IsAvailablePaymentChannel(paymentChannel))
            {
                errors.Add(_localizationService.GetResource("Plugins.Payments.Yuansfer.PaymentChannel.IsNotAvailable"));
            }

            return errors;
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            var paymentRequest = new ProcessPaymentRequest();

            var paymentChannel = form[nameof(PaymentInfoModel.PaymentChannel)].ToString();
            var paymentChannelKey = _localizationService.GetResource("Plugins.Payments.Yuansfer.PaymentChannel.Key");
            paymentRequest.CustomValues.Add(paymentChannelKey, paymentChannel);

            return paymentRequest;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).RouteUrl(Defaults.ConfigurationRouteName);
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <returns>View component name</returns>
        public string GetPublicViewComponentName()
        {
            return Defaults.PAYMENT_INFO_VIEW_COMPONENT_NAME;
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new YuansferPaymentSettings
            {
                BaseApiUrl = Defaults.Api.SandboxBaseUrl,
                PaymentChannels = new List<string> { "alipay", "wechatpay", "paypal", "venmo", "unionpay" }
            };
            _settingService.SaveSetting(settings);

            //locales
            _localizationService.AddPluginLocaleResource(new Dictionary<string, string>
            {
                ["Plugins.Payments.Yuansfer.IsNotConfigured"] = "Plugin isn't configured.",
                ["Plugins.Payments.Yuansfer.PaymentChannel.IsNotAvailable"] = "The payment channel isn't available.",
                ["Plugins.Payments.Yuansfer.PaymentChannel.Select"] = "Select the wallet:",
                ["Plugins.Payments.Yuansfer.PaymentChannel.Key"] = "The wallet",
                ["Plugins.Payments.Yuansfer.PaymentMethodDescription"] = "Pay by Yuansfer",
                ["Plugins.Payments.Yuansfer.RoundingWarning"] = "It looks like you have \"ShoppingCartSettings.RoundPricesDuringCalculation\" setting disabled. Keep in mind that this can lead to a discrepancy of the order total amount, as PayPal only rounds to two decimals.",
                ["Plugins.Payments.Yuansfer.Fields.ApiToken"] = "API token",
                ["Plugins.Payments.Yuansfer.Fields.ApiToken.Required"] = "The API token is required.",
                ["Plugins.Payments.Yuansfer.Fields.ApiToken.Hint"] = "Enter the token to sign the API requests.",
                ["Plugins.Payments.Yuansfer.Fields.BaseApiUrl"] = "Base API URL",
                ["Plugins.Payments.Yuansfer.Fields.BaseApiUrl.Required"] = "The base API URL is required.",
                ["Plugins.Payments.Yuansfer.Fields.BaseApiUrl.Hint"] = "Enter the base URL of the Yuansfer environment API.",
                ["Plugins.Payments.Yuansfer.Fields.MerchantId"] = "Merchant No.",
                ["Plugins.Payments.Yuansfer.Fields.MerchantId.Required"] = "The merchant No. is required.",
                ["Plugins.Payments.Yuansfer.Fields.MerchantId.Hint"] = "Enter the merchant No.",
                ["Plugins.Payments.Yuansfer.Fields.StoreId"] = "Store No.",
                ["Plugins.Payments.Yuansfer.Fields.StoreId.Required"] = "The store No. is required.",
                ["Plugins.Payments.Yuansfer.Fields.StoreId.Hint"] = "Enter the store No.",
                ["Plugins.Payments.Yuansfer.Fields.PaymentChannels"] = "Payment channels",
                ["Plugins.Payments.Yuansfer.Fields.PaymentChannels.Required"] = "The payment channels are required.",
                ["Plugins.Payments.Yuansfer.Fields.PaymentChannels.Hint"] = "Select the payment channels available in checkout.",
                ["Plugins.Payments.Yuansfer.Fields.AdditionalFee"] = "Additional fee",
                ["Plugins.Payments.Yuansfer.Fields.AdditionalFee.Hint"] = "Enter additional fee to charge your customers.",
                ["Plugins.Payments.Yuansfer.Fields.AdditionalFeePercentage"] = "Additional fee. Use percentage",
                ["Plugins.Payments.Yuansfer.Fields.AdditionalFeePercentage.Hint"] = "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.",
            });

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<YuansferPaymentSettings>();

            //locales
            _localizationService.DeletePluginLocaleResources("Plugins.Payments.Yuansfer");

            base.Uninstall();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => false;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => true;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => true;

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid => false;

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo => false;

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        /// <remarks>
        /// return description of this payment method to be display on "payment method" checkout step. good practice is to make it localizable
        /// for example, for a redirection payment method, description may be like this: "You will be redirected to PayPal site to complete the payment"
        /// </remarks>
        public string PaymentMethodDescription => _localizationService.GetResource("Plugins.Payments.Yuansfer.PaymentMethodDescription");

        #endregion
    }
}