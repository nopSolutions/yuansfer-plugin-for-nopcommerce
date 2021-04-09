using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.Yuansfer.Models
{
    /// <summary>
    /// Represents a payment info model
    /// </summary>
    public record PaymentInfoModel : BaseNopModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets the selected payment channel
        /// </summary>
        public string PaymentChannel { get; set; }

        /// <summary>
        /// Gets or sets the available payment channels
        /// </summary>
        public IList<SelectListItem> AvailablePaymentChannels { get; set; }

        #endregion

        #region Ctor

        public PaymentInfoModel()
        {
            AvailablePaymentChannels = new List<SelectListItem>();
        }

        #endregion
    }
}