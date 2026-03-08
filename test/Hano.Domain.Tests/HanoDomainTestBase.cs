using Volo.Abp.Modularity;

namespace Hano;

/* Inherit from this class for your domain layer tests. */
public abstract class HanoDomainTestBase<TStartupModule> : HanoTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
