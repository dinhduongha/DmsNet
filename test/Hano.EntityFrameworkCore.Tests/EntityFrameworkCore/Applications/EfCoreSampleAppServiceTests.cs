using Hano.Samples;
using Xunit;

namespace Hano.EntityFrameworkCore.Applications;

[Collection(HanoTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<HanoEntityFrameworkCoreTestModule>
{

}
