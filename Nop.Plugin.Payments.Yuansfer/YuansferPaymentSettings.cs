using System.Collections.Generic;
using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.Yuansfer
{
    /// <summary>
    /// Represents the settings of Yuansfer payment plugin
    /// </summary>
    public class YuansferPaymentSettings : ISettings
    {
        #region Properties

        /// <summary>
        /// Gets or sets the base URL of the Yuansfer environment API
        /// </summary>
        public string BaseApiUrl { get; set; }

        /// <summary>
        /// Gets or sets the merchant id
        /// </summary>
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets the store id
        /// </summary>
        public string StoreId { get; set; }

        /// <summary>
        /// Gets or sets the token to sign the API requests
        /// </summary>
        public string ApiToken { get; set; }

        /// <summary>
        /// Gets or sets the payment channels
        /// </summary>
        public List<string> PaymentChannels { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }

        /// <summary>
        /// Gets or sets an additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }

        #endregion
    }
}
