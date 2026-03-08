using System;
using System.Collections.Generic;
using System.Text;
using Hano.Localization;
using Volo.Abp.Application.Services;

namespace Hano;

/* Inherit your application services from this class.
 */
public abstract class HanoAppService : ApplicationService
{
    protected HanoAppService()
    {
        LocalizationResource = typeof(HanoResource);
    }
}
