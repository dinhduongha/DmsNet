using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
namespace Hano.Core.Application.Contracts.Audit;

public interface IAuditAppService : IApplicationService
{
    Task<Guid> CreateOsaAsync(OsaReportInputDto input);
    Task<Guid> CreateOosAsync(OosReportInputDto input);
    Task<Guid> CreatePosmAsync(PosmReportInputDto input);
}
