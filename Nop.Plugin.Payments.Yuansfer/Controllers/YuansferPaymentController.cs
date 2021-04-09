using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.Yuansfer.Models;
using Nop.Plugin.Payments.Yuansfer.Services;
using Nop.Services.Configuration;
using Nop.Services.Customers;
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

        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly YuansferApi _yuansferApi;

        #endregion

        #region Ctor

        public YuansferPaymentController(ICustomerService customerService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWorkContext workContext,
            ShoppingCartSettings shoppingCartSettings,
            YuansferApi yuansferApi)
        {
            _customerService = customerService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _workContext = workContext;
            _shoppingCartSettings = shoppingCartSettings;
            _yuansferApi = yuansferApi;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var yuansferPaymentSettings = await _settingService.LoadSettingAsync<YuansferPaymentSettings>(storeScope);

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
                model.BaseApiUrl_OverrideForStore = await _settingService.SettingExistsAsync(yuansferPaymentSettings, x => x.BaseApiUrl, storeScope);
                model.MerchantId_OverrideForStore = await _settingService.SettingExistsAsync(yuansferPaymentSettings, x => x.MerchantId, storeScope);
                model.StoreId_OverrideForStore = await _settingService.SettingExistsAsync(yuansferPaymentSettings, x => x.StoreId, storeScope);
                model.ApiToken_OverrideForStore = await _settingService.SettingExistsAsync(yuansferPaymentSettings, x => x.ApiToken, storeScope);
                model.PaymentChannels_OverrideForStore = await _settingService.SettingExistsAsync(yuansferPaymentSettings, x => x.PaymentChannels, storeScope);
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(yuansferPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(yuansferPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
            }

            //prices and total aren't rounded, so display warning
            if (!_shoppingCartSettings.RoundPricesDuringCalculation)
            {
                var url = Url.Action("AllSettings", "Setting", new { settingName = nameof(ShoppingCartSettings.RoundPricesDuringCalculation) });
                var warning = string.Format(await _localizationService.GetResourceAsync("Plugins.Payments.Yuansfer.RoundingWarning"), url);
                _notificationService.WarningNotification(warning, false);
            }

            return View("~/Plugins/Payments.Yuansfer/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var yuansferPaymentSettings = await _settingService.LoadSettingAsync<YuansferPaymentSettings>(storeScope);

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

            await _settingService.SaveSettingOverridablePerStoreAsync(yuansferPaymentSettings, x => x.BaseApiUrl, model.BaseApiUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(yuansferPaymentSettings, x => x.MerchantId, model.MerchantId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(yuansferPaymentSettings, x => x.StoreId, model.StoreId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(yuansferPaymentSettings, x => x.ApiToken, model.ApiToken_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(yuansferPaymentSettings, x => x.PaymentChannels, model.PaymentChannels_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(yuansferPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(yuansferPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("request-demo")]
        public async Task<IActionResult> RequestDemo(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            try
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var fullName = await _customerService.GetCustomerFullNameAsync(customer);
                var request = new MerchantInfoRequest
                {
                    Email = model.MerchantEmail,
                    ClientName = fullName,
                    Plugin = Defaults.Api.UserAgent
                };
                var response = await _yuansferApi.RequestDemoAsync(request);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Payments.Yuansfer.Fields.MerchantEmail.Success"));
            }
            catch (Exception exception)
            {
                await _notificationService.ErrorNotificationAsync(exception);
            }

            return await Configure();
        }

        #endregion
    }
}