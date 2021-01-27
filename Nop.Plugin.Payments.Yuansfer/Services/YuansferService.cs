using System;
using System.Linq;
using FluentValidation;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Payments.Yuansfer.Models;
using Nop.Services.Common;

namespace Nop.Plugin.Payments.Yuansfer.Services
{
    public class YuansferService
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreContext _storeContext;
        private readonly YuansferPaymentSettings _settings;
        private readonly IValidator<ConfigurationModel> _configurationModelValidator;

        #endregion

        #region Ctor

        public YuansferService(
            IGenericAttributeService genericAttributeService,
            IStoreContext storeContext,
            YuansferPaymentSettings settings,
            IValidator<ConfigurationModel> configurationModelValidator
        )
        {
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
            _settings = settings;
            _configurationModelValidator = configurationModelValidator;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the value indicating whether to plugin is configured
        /// </summary>
        /// <returns>The value indicating whether to plugin is configured</returns>
        public bool IsConfigured()
        {
            var model = new ConfigurationModel
            {
                BaseApiUrl = _settings.BaseApiUrl,
                MerchantId = _settings.MerchantId,
                StoreId = _settings.StoreId,
                ApiToken = _settings.ApiToken,
                PaymentChannels = _settings.PaymentChannels,
                AdditionalFee = _settings.AdditionalFee,
                AdditionalFeePercentage = _settings.AdditionalFeePercentage,
            };

            var validationResult = _configurationModelValidator.Validate(model);

            return validationResult.IsValid;
        }

        /// <summary>
        /// Gets the customer payment channel
        /// </summary>
        /// <param name="customer">The customer</param>
        /// <returns>The customer payment channel</returns>
        public string GetCustomerPaymentChannel(Customer customer)
        {
            if (customer is null)
                throw new ArgumentNullException(nameof(customer));

            var paymentChannel = _genericAttributeService.GetAttribute<string>(
                customer, Defaults.PaymentChannelAttribute, _storeContext.CurrentStore.Id);

            return IsAvailablePaymentChannel(paymentChannel)
                ? paymentChannel
                : null;
        }

        /// <summary>
        /// Sets the customer payment channel
        /// </summary>
        /// <param name="customer">The customer</param>
        /// <param name="paymentChannel">The payment channel</param>
        public void SetCustomerPaymentChannel(Customer customer, string paymentChannel)
        {
            if (customer is null)
                throw new ArgumentNullException(nameof(customer));

            if (!IsAvailablePaymentChannel(paymentChannel))
                throw new ArgumentException($"'{nameof(paymentChannel)}' isn't available.", nameof(paymentChannel));

            _genericAttributeService.SaveAttribute(
                customer, Defaults.PaymentChannelAttribute, paymentChannel, _storeContext.CurrentStore.Id);
        }

        /// <summary>
        /// Returns the value indicating whether to payment channel is available
        /// </summary>
        /// <param name="paymentChannel">The payment channel</param>
        /// <returns>The value indicating whether to payment channel is available</returns>
        public bool IsAvailablePaymentChannel(string paymentChannel)
        {
            return !string.IsNullOrWhiteSpace(paymentChannel) &&
                _settings.PaymentChannels.Any(ch => ch == paymentChannel);
        }

        #endregion
    }
}
