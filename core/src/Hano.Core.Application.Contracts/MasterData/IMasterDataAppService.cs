using System;
using System.Threading.Tasks;
using Hano.Core.Application.Contracts.MasterData.Dtos;
using Volo.Abp.Application.Services;
namespace Hano.Core.Application.Contracts.MasterData;

public interface IMasterDataAppService : IApplicationService
{
    Task<MasterDataSyncResponseDto> SyncAsync(DateTime? lastSyncTimestamp);
}
