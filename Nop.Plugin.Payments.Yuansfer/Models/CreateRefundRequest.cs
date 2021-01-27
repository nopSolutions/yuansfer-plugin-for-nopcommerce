using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Yuansfer.Models
{
    /// <summary>
    /// Represents a request to create the refund of the transaction
    /// </summary>
    public class CreateRefundRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets the merchant id
        /// </summary>
        [JsonProperty("merchantNo")]
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets the store id
        /// </summary>
        [JsonProperty("storeNo")]
        public string StoreId { get; set; }

        /// <summary>
        /// Gets or sets the refund amount
        /// </summary>
        [JsonProperty("refundAmount")]
        public string RefundAmount { get; set; }

        /// <summary>
        /// Gets or sets the transaction currency; USD, CNY, PHP, IDR, KRW, HKD
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets the settlement currency (used for all currencies excluding USD)
        /// </summary>
        [JsonProperty("settleCurrency")]
        public string SettleCurrency { get; set; }

        /// <summary>
        /// Gets or sets the parameter signature, see https://docs.yuansfer.com/api-reference-v3/signing-api-parameters
        /// </summary>
        [JsonProperty("verifySign")]
        public string VerifySign { get; set; }

        /// <summary>
        /// Gets or sets the transaction ID in the Yuansfer system
        /// </summary>
        [JsonProperty("transactionNo")]
        public string TransactionId { get; set; }

        #endregion
    }
}
