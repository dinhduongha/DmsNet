using System.Threading.Tasks;
using Hano.Core.Application.Contracts.Dashboard.Dtos;
using Volo.Abp.Application.Services;
namespace Hano.Core.Application.Contracts.Dashboard;

public interface IDashboardAppService : IApplicationService
{
    Task<NvbhDashboardDto> GetNvbhAsync();
    Task<GsbhDashboardDto> GetGsbhAsync();
    Task<AsmDashboardDto> GetAsmAsync();
}
