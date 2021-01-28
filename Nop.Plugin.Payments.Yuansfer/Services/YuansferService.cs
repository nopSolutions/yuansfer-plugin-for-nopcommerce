using System.Linq;
using FluentValidation;
using Nop.Plugin.Payments.Yuansfer.Models;

namespace Nop.Plugin.Payments.Yuansfer.Services
{
    public class YuansferService
    {
        #region Fields

        private readonly YuansferPaymentSettings _settings;
        private readonly IValidator<ConfigurationModel> _configurationModelValidator;

        #endregion

        #region Ctor

        public YuansferService(
            YuansferPaymentSettings settings,
            IValidator<ConfigurationModel> configurationModelValidator
        )
        {
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
