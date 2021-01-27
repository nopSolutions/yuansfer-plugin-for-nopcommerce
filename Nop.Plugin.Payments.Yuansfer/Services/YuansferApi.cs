using System;
using System.Net.Http;
using System.Threading.Tasks;
using Nop.Plugin.Payments.Yuansfer.Models;

namespace Nop.Plugin.Payments.Yuansfer.Services
{
    /// <summary>
    /// Provides an default implementation the HTTP client to interact with the yuansfer endpoints
    /// </summary>
    public class YuansferApi : BaseHttpClient
    {
        #region Ctor

        public YuansferApi(YuansferPaymentSettings settings, HttpClient httpClient)
            : base (settings, httpClient)
        {
        }

        #endregion

        #region Methods

        public Task<ApiResponse<SecurePayPayload>> SecurePayAsync(SecurePayRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            return PostAsync<ApiResponse<SecurePayPayload>>(Defaults.Api.Endpoints.SecurePayPath, request);
        }

        public Task<ApiResponse<CreateRefundPayload>> CreateRefundAsync(CreateRefundRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            return PostAsync<ApiResponse<CreateRefundPayload>>(Defaults.Api.Endpoints.RefundPath, request);
        }

        #endregion
    }
}
