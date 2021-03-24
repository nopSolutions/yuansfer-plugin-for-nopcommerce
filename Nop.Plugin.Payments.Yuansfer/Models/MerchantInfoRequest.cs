using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Yuansfer.Models
{
    /// <summary>
    /// Represents a request to pass merchant info
    /// </summary>
    public class MerchantInfoRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets the merchant email
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the merchant full name
        /// </summary>
        [JsonProperty("clientName")]
        public string ClientName { get; set; }

        /// <summary>
        /// Gets or sets the plugin info
        /// </summary>
        [JsonProperty("plugin")]
        public string Plugin { get; set; }

        #endregion
    }
}