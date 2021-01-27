using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Yuansfer.Models
{
    /// <summary>
    /// Represents a request to pay for an order
    /// </summary>
    public class SecurePayRequest
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
        /// Gets or sets the transaction amount
        /// </summary>
        [JsonProperty("amount")]
        public string Amount { get; set; }

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
        /// Gets or sets the payment channel. 
        /// The possible values are: 
        /// "alipay", "wechatpay", "paypal",  "venmo", 
        /// "unionpay", "creditcard" "truemoney", "alipay_hk", 
        /// "tng", "gcash", "dana", "kakaopay", "bkash", "easypaisa"
        /// </summary>
        [JsonProperty("vendor")]
        public string Vendor { get; set; }

        /// <summary>
        /// Gets or sets the asynchronous callback address. The IPN url must be secure
        /// </summary>
        [JsonProperty("ipnUrl")]
        public string IpnUrl { get; set; }

        /// <summary>
        /// Gets or sets the synchronous callback HTTP address to receive notification messages for events. 
        /// The callback url follows macro substitution rules like xxxcallback_url?trans_no={amount}&amount={amount}, 
        /// then Yuansfer will automatically replace the values of {}. For a list of parameters.
        /// </summary>
        [JsonProperty("callbackUrl")]
        public string CallbackUrl { get; set; }

        /// <summary>
        /// Gets or sets the terminal. The possible values are: "ONLINE", "WAP", "YIP"
        /// </summary>
        [JsonProperty("terminal")]
        public string Terminal { get; set; }

        /// <summary>
        /// Gets or sets the goods info. The JSON encoded string of an array of items that the customer purchases from the merchant.
        /// Special characters are not supported. e.g.: 
        /// <code>[{"goods_name":"name1", "quantity":"quantity1"}, {"goods_name":"name2", "quantity":"quantity2"}]</code>
        /// </summary>
        [JsonProperty("goodsInfo")]
        public string GoodsInfo { get; set; }

        /// <summary>
        /// Gets or sets the parameter signature, see https://docs.yuansfer.com/api-reference-v3/signing-api-parameters
        /// </summary>
        [JsonProperty("verifySign")]
        public string VerifySign { get; set; }

        /// <summary>
        /// Gets or sets the invoice number of the transaction in the merchant’s system
        /// </summary>
        [JsonProperty("reference")]
        public string Reference { get; set; }

        #endregion
    }
}
