using Hano.Core.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Hano.Core;

public abstract class CoreController : AbpControllerBase
{
    protected CoreController()
    {
        LocalizationResource = typeof(CoreResource);
    }
}
