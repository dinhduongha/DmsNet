using Volo.Abp.AspNetCore.Mvc;

namespace Hano.Core.HttpApi;

public abstract class HanoCoreController : AbpControllerBase
{
    protected HanoCoreController()
    {
        // No auto API explorer — manual [Route] on each controller
    }
}
