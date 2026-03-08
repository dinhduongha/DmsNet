using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.Application;
using Hano.Core.BackgroundJobs;

namespace Hano.Core;

[DependsOn(
    typeof(CoreDomainModule),
    typeof(CoreApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(HanoCoreBackgroundJobsModule),
    typeof(AbpMapperlyModule)
    )]
public class CoreApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<CoreApplicationModule>();
    }
}
