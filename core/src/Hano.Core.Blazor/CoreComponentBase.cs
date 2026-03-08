using Hano.Core.Localization;
using Volo.Abp.AspNetCore.Components;

namespace Hano.Core.Blazor;

public abstract class CoreComponentBase : AbpComponentBase
{
    protected CoreComponentBase()
    {
        LocalizationResource = typeof(CoreResource);
    }
}