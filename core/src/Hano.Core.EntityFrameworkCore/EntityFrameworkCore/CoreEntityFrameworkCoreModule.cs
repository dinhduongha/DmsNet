using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Hano.Core.EntityFrameworkCore;

[DependsOn(
    typeof(CoreDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class CoreEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<CoreDbContext>(options =>
        {
            options.AddDefaultRepositories<ICoreDbContext>(includeAllEntities: true);
            
            /* Add custom repositories here. Example:
            * options.AddRepository<Question, EfCoreQuestionRepository>();
            */
        });
    }
}
