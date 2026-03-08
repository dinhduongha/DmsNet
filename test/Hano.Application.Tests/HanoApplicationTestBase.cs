using Volo.Abp.Modularity;

namespace Hano;

public abstract class HanoApplicationTestBase<TStartupModule> : HanoTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
