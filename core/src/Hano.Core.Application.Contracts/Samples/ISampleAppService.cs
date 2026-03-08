using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Hano.Core.Samples;

public interface ISampleAppService : IApplicationService
{
    Task<SampleDto> GetAsync();

    Task<SampleDto> GetAuthorizedAsync();
}
