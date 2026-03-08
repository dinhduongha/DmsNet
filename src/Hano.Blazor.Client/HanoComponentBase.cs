using Hano.Localization;
using Volo.Abp.AspNetCore.Components;

namespace Hano.Blazor.Client;

public abstract class HanoComponentBase : AbpComponentBase
{
    protected HanoComponentBase()
    {
        LocalizationResource = typeof(HanoResource);
    }
}
