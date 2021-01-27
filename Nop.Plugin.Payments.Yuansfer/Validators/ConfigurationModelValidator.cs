using FluentValidation;
using Nop.Plugin.Payments.Yuansfer.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Payments.Yuansfer.Validators
{
    /// <summary>
    /// Represents a validator for <see cref="ConfigurationModel"/>
    /// </summary>
    public class ConfigurationModelValidator : BaseNopValidator<ConfigurationModel>
    {
        #region Ctor

        public ConfigurationModelValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.BaseApiUrl)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Plugins.Payments.Yuansfer.Fields.BaseApiUrl.Required"));

            RuleFor(model => model.MerchantId)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Plugins.Payments.Yuansfer.Fields.MerchantId.Required"));

            RuleFor(model => model.StoreId)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Plugins.Payments.Yuansfer.Fields.StoreId.Required"));

            RuleFor(model => model.ApiToken)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Plugins.Payments.Yuansfer.Fields.ApiToken.Required"));

            RuleFor(model => model.PaymentChannels)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Plugins.Payments.Yuansfer.Fields.PaymentChannels.Required"));
        }

        #endregion
    }
}
