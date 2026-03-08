using Hano.Samples;
using Xunit;

namespace Hano.EntityFrameworkCore.Domains;

[Collection(HanoTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<HanoEntityFrameworkCoreTestModule>
{

}
