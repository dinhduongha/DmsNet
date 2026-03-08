using Volo.Abp.Modularity;

namespace Hano;

[DependsOn(
    typeof(HanoApplicationModule),
    typeof(HanoDomainTestModule)
)]
public class HanoApplicationTestModule : AbpModule
{

}
