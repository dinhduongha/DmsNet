using Hano.Core.Localization;
using Volo.Abp.Application.Services;

namespace Hano.Core;

public abstract class CoreAppService : ApplicationService
{
    protected CoreAppService()
    {
        LocalizationResource = typeof(CoreResource);
        ObjectMapperContext = typeof(CoreApplicationModule);
    }
}
