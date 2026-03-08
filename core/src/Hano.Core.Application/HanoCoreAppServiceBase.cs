using System;
using Volo.Abp.Application.Services;
namespace Hano.Core.Application;

public abstract class HanoCoreAppServiceBase : ApplicationService
{
    protected Guid CurrentUserId => CurrentUser.Id ?? throw new Volo.Abp.AbpException("User not authenticated");
}
