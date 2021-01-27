using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Yuansfer.Models
{
    /// <summary>
    /// Represents a response to pay for an order
    /// </summary>
    public class SecurePayPayload
    {
        #region Properties

        /// <summary>
        /// Gets or sets the transaction amount. It returns when you use USD as the payment currency
        /// </summary>
        [JsonProperty("amount")]
        public string Amount { get; set; }

        /// <summary>
        /// Gets or sets the transaction currency; USD, CNY, PHP, IDR, KRW, HKD
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets the three-character currency code that identifies the settlement currency. The possible values are: "USD"
        /// </summary>
        [JsonProperty("settleCurrency")]
        public string SettleCurrency { get; set; }

        /// <summary>
        /// Gets or sets the transaction ID in the Yuansfer system
        /// </summary>
        [JsonProperty("transactionNo")]
        public string TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the invoice number of the transaction in the merchant’s system
        /// </summary>
        [JsonProperty("reference")]
        public string Reference { get; set; }

        /// <summary>
        /// Gets or sets the URL to the cashier page
        /// </summary>
        [JsonProperty("cashierUrl")]
        public string CashierUrl { get; set; }

        #endregion
    }
}
