using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Yuansfer.Models
{
    /// <summary>
    /// Represents a response payload when refund of the transaction is requested
    /// </summary>
    public class CreateRefundPayload
    {
        #region Properties

        /// <summary>
        /// Gets or sets the transaction status
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        #endregion
    }
}
