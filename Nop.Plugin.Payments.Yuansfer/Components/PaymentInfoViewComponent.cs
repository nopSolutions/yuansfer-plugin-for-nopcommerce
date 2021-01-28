using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.Yuansfer.Models;
using Nop.Plugin.Payments.Yuansfer.Services;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.Yuansfer.Components
{
    /// <summary>
    /// Represents a view component to display payment info in public store
    /// </summary>
    [ViewComponent(Name = Defaults.PAYMENT_INFO_VIEW_COMPONENT_NAME)]
    public class PaymentInfoViewComponent : NopViewComponent
    {
        #region Fields

        private readonly YuansferService _yuansferService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public PaymentInfoViewComponent(
            YuansferService yuansferService, 
            ILocalizationService localizationService
        )
        {
            _yuansferService = yuansferService;
            _localizationService = localizationService;
        }

        #endregion

        #region Methods

        public IViewComponentResult Invoke()
        {
            var model = new PaymentInfoModel();

            if (!_yuansferService.IsConfigured())
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Plugins.Payments.Yuansfer.IsNotConfigured"));
            else
            {
                model.AvailablePaymentChannels = Defaults.AvailablePaymentChannels
                    .Where(item => _yuansferService.IsAvailablePaymentChannel(item.Value))
                    .ToList();
            }

            return View("~/Plugins/Payments.Yuansfer/Views/PaymentInfo.cshtml", model);
        }

        #endregion
    }
}
