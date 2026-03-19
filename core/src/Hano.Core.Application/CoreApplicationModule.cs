using Hano.Core.BackgroundJobs;
using Hano.Core.Import.Excel;
using Hano.Core.Import.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Application;
using Volo.Abp.Identity;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.TenantManagement;

namespace Hano.Core;

[DependsOn(
    typeof(CoreDomainModule),
    typeof(CoreApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(HanoCoreBackgroundJobsModule),
    typeof(AbpMapperlyModule),
    typeof(AbpIdentityDomainModule),
    typeof(AbpTenantManagementDomainModule)
)]
public class CoreApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<CoreApplicationModule>();
        context.Services.AddTransient<ExcelReaderFactory>();
        context.Services.AddTransient<UsernamePasswordGenerator>();
    }
}
