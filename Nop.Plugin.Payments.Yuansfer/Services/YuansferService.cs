using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.Yuansfer.Models;

namespace Nop.Plugin.Payments.Yuansfer.Services
{
    public class YuansferService
    {
        #region Fields

        private readonly YuansferPaymentSettings _settings;

        #endregion

        #region Ctor

        public YuansferService(YuansferPaymentSettings settings)
        {
            _settings = settings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the value indicating whether to plugin is configured
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the value indicating whether to plugin is configured
        /// </returns>
        public async Task<bool> IsConfiguredAsync()
        {
            //resolve validator here to exclude warnings after installation process
            var validator = EngineContext.Current.Resolve<IValidator<ConfigurationModel>>();
            var validationResult = await validator.ValidateAsync(new ConfigurationModel
            {
                BaseApiUrl = _settings.BaseApiUrl,
                MerchantId = _settings.MerchantId,
                StoreId = _settings.StoreId,
                ApiToken = _settings.ApiToken,
                PaymentChannels = _settings.PaymentChannels
            });

            return validationResult.IsValid;
        }

        /// <summary>
        /// Returns the value indicating whether to payment channel is available
        /// </summary>
        /// <param name="paymentChannel">The payment channel</param>
        /// <returns>The value indicating whether to payment channel is available</returns>
        public bool IsAvailablePaymentChannel(string paymentChannel)
        {
            return !string.IsNullOrWhiteSpace(paymentChannel) && _settings.PaymentChannels.Any(ch => ch == paymentChannel);
        }

        #endregion
    }
}