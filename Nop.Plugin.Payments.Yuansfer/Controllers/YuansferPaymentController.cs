using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.Yuansfer.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.Yuansfer.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public class YuansferPaymentController : BasePaymentController
    {
        #region Fields
        
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public YuansferPaymentController(
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext
        )
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var yuansferPaymentSettings = _settingService.LoadSetting<YuansferPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                BaseApiUrl = yuansferPaymentSettings.BaseApiUrl,
                MerchantId = yuansferPaymentSettings.MerchantId,
                StoreId = yuansferPaymentSettings.StoreId,
                ApiToken = yuansferPaymentSettings.ApiToken,
                PaymentChannels = yuansferPaymentSettings.PaymentChannels,
                AdditionalFee = yuansferPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = yuansferPaymentSettings.AdditionalFeePercentage,
                AvailablePaymentChannels = Defaults.AvailablePaymentChannels
            };

            if (storeScope > 0)
            {
                model.BaseApiUrl_OverrideForStore = _settingService.SettingExists(yuansferPaymentSettings, x => x.BaseApiUrl, storeScope);
                model.MerchantId_OverrideForStore = _settingService.SettingExists(yuansferPaymentSettings, x => x.MerchantId, storeScope);
                model.StoreId_OverrideForStore = _settingService.SettingExists(yuansferPaymentSettings, x => x.StoreId, storeScope);
                model.ApiToken_OverrideForStore = _settingService.SettingExists(yuansferPaymentSettings, x => x.ApiToken, storeScope);
                model.PaymentChannels_OverrideForStore = _settingService.SettingExists(yuansferPaymentSettings, x => x.PaymentChannels, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(yuansferPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(yuansferPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
            }

            return View("~/Plugins/Payments.Yuansfer/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var yuansferPaymentSettings = _settingService.LoadSetting<YuansferPaymentSettings>(storeScope);

            //save settings
            yuansferPaymentSettings.BaseApiUrl = model.BaseApiUrl;
            yuansferPaymentSettings.MerchantId = model.MerchantId;
            yuansferPaymentSettings.StoreId = model.StoreId;
            yuansferPaymentSettings.ApiToken = model.ApiToken;
            yuansferPaymentSettings.PaymentChannels = model.PaymentChannels.ToList();
            yuansferPaymentSettings.AdditionalFee = model.AdditionalFee;
            yuansferPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */

            _settingService.SaveSettingOverridablePerStore(yuansferPaymentSettings, x => x.BaseApiUrl, model.BaseApiUrl_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(yuansferPaymentSettings, x => x.MerchantId, model.MerchantId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(yuansferPaymentSettings, x => x.StoreId, model.StoreId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(yuansferPaymentSettings, x => x.ApiToken, model.ApiToken_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(yuansferPaymentSettings, x => x.PaymentChannels, model.PaymentChannels_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(yuansferPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(yuansferPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            
            //now clear settings cache
            _settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        #endregion
    }
}