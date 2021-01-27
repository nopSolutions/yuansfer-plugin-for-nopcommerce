namespace Nop.Plugin.Payments.Yuansfer.Models
{
    /// <summary>
    /// Represents a webhook request after paying
    /// </summary>
    public class SecurePayWebhookRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets the transaction amount
        /// </summary>
        public string Amount { get; set; }

        /// <summary>
        /// Gets or sets the transaction currency; USD, CNY, PHP, IDR, KRW, HKD
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets the settlement currency (used for all currencies excluding USD)
        /// </summary>
        public string SettleCurrency { get; set; }

        /// <summary>
        /// Gets or sets the parameter signature, see https://docs.yuansfer.com/api-reference-v3/signing-api-parameters
        /// </summary>
        public string VerifySign { get; set; }

        /// <summary>
        /// Gets or sets the invoice number of the transaction in the merchant’s system
        /// </summary>
        public string Reference { get; set; }

        /// <summary>
        /// Gets or sets the transaction ID in the Yuansfer system
        /// </summary>
        public string TransactionNo { get; set; }

        /// <summary>
        /// Gets or sets the request time
        /// </summary>
        public string Time { get; set; }

        /// <summary>
        /// Gets or sets the transaction status
        /// </summary>
        public string Status { get; set; }

        #endregion
    }
}
