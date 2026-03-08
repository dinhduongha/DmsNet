using Hano.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Hano.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class HanoController : AbpControllerBase
{
    protected HanoController()
    {
        LocalizationResource = typeof(HanoResource);
    }
}
