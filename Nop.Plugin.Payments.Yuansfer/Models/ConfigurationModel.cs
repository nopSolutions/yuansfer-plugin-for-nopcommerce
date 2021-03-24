using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.Yuansfer.Models
{
    /// <summary>
    /// Represents a configuration model
    /// </summary>
    public class ConfigurationModel : BaseNopModel
    {
        #region Properties

        public int ActiveStoreScopeConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the base URL of the Yuansfer environment API
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.Yuansfer.Fields.BaseApiUrl")]
        public string BaseApiUrl { get; set; }
        public bool BaseApiUrl_OverrideForStore { get; set; }

        /// <summary>
        /// Gets or sets the merchant id
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.Yuansfer.Fields.MerchantId")]
        public string MerchantId { get; set; }
        public bool MerchantId_OverrideForStore { get; set; }

        /// <summary>
        /// Gets or sets the store id
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.Yuansfer.Fields.StoreId")]
        public string StoreId { get; set; }
        public bool StoreId_OverrideForStore { get; set; }

        /// <summary>
        /// Gets or sets the token to sign the API requests
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.Yuansfer.Fields.ApiToken")]
        public string ApiToken { get; set; }
        public bool ApiToken_OverrideForStore { get; set; }

        /// <summary>
        /// Gets or sets the payment channels
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.Yuansfer.Fields.PaymentChannels")]
        public IList<string> PaymentChannels { get; set; }
        public bool PaymentChannels_OverrideForStore { get; set; }

        /// <summary>
        /// Gets or sets the available payment channels
        /// </summary>
        public IList<SelectListItem> AvailablePaymentChannels { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.Yuansfer.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        /// <summary>
        /// Gets or sets an additional fee
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.Yuansfer.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Yuansfer.Fields.MerchantEmail")]
        [DataType(DataType.EmailAddress)]
        public string MerchantEmail { get; set; }

        #endregion

        #region Ctor

        public ConfigurationModel()
        {
            PaymentChannels = new List<string>();
            AvailablePaymentChannels = new List<SelectListItem>();
        }

        #endregion
    }
}