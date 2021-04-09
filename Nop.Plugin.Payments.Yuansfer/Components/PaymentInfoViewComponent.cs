using System.Linq;
using System.Threading.Tasks;
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

        private readonly ILocalizationService _localizationService;
        private readonly YuansferService _yuansferService;

        #endregion

        #region Ctor

        public PaymentInfoViewComponent(ILocalizationService localizationService,
            YuansferService yuansferService)
        {
            _localizationService = localizationService;
            _yuansferService = yuansferService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <returns>A task that represents the asynchronous operation whose result contains the view component result</returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new PaymentInfoModel();

            if (!await _yuansferService.IsConfiguredAsync())
                ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Plugins.Payments.Yuansfer.IsNotConfigured"));
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