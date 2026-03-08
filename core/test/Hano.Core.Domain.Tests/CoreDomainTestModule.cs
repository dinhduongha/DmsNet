using Volo.Abp.Modularity;

namespace Hano.Core;

[DependsOn(
    typeof(CoreDomainModule),
    typeof(CoreTestBaseModule)
)]
public class CoreDomainTestModule : AbpModule
{

}
