using Volo.Abp.Modularity;

namespace Hano.Core;

[DependsOn(
    typeof(CoreApplicationModule),
    typeof(CoreDomainTestModule)
    )]
public class CoreApplicationTestModule : AbpModule
{

}
