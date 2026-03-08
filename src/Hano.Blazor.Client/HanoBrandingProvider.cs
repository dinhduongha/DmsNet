using Microsoft.Extensions.Localization;
using Hano.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Hano.Blazor.Client;

[Dependency(ReplaceServices = true)]
public class HanoBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<HanoResource> _localizer;

    public HanoBrandingProvider(IStringLocalizer<HanoResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
