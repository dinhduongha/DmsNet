using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace Hano.Core.Domain;

[DependsOn(typeof(AbpDddDomainModule))]
public class HanoCoreDomainModule : AbpModule
{
}
