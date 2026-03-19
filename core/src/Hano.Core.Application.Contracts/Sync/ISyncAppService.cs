using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
namespace Hano.Core.Application.Contracts.Sync;

public interface ISyncAppService : IApplicationService
{
    Task<SyncUploadResultDto> UploadAsync(SyncUploadDto input);
    Task ResolveConflictAsync(ResolveConflictDto input);
}
