using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Yuansfer.Models
{
    /// <summary>
    /// Represents a API response
    /// </summary>
    public class ApiResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets the response return code. For more details, see https://docs.yuansfer.com/api-reference-v3/notes#response-return-code.
        /// </summary>
        [JsonProperty("ret_code")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the message
        /// </summary>
        [JsonProperty("ret_msg")]
        public string Message { get; set; }

        #endregion
    }

    /// <summary>
    /// Represents a API response with payload
    /// </summary>
    public class ApiResponse<TPayload> : ApiResponse where TPayload : class
    {
        #region Properties

        /// <summary>
        /// Gets or sets the response payload (parsed HTTP body).
        /// </summary>
        [JsonProperty("result")]
        public TPayload Payload { get; set; }

        #endregion
    }
}
