using Volo.Abp.Modularity;

namespace Hano;

[DependsOn(
    typeof(HanoDomainModule),
    typeof(HanoTestBaseModule)
)]
public class HanoDomainTestModule : AbpModule
{

}
