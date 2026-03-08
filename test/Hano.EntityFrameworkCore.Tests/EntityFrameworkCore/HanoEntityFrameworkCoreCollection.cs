using Xunit;

namespace Hano.EntityFrameworkCore;

[CollectionDefinition(HanoTestConsts.CollectionDefinitionName)]
public class HanoEntityFrameworkCoreCollection : ICollectionFixture<HanoEntityFrameworkCoreFixture>
{

}
