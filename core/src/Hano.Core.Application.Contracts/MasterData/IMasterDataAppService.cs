using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
namespace Hano.Core.Application.Contracts.MasterData;

public interface IMasterDataAppService : IApplicationService
{
    Task<MasterDataSyncResponseDto> SyncAsync(DateTime? lastSyncTimestamp);
}
