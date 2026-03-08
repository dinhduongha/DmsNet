using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace Hano.Core;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(CoreDomainSharedModule)
)]
public class CoreDomainModule : AbpModule
{

}
