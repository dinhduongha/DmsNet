using System;
using System.Threading.Tasks;
using Hano.Core.Application.Contracts.Outlets.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Hano.Core.Application.Contracts.Outlets;

/// <summary>
/// Inherits: GetAsync, GetListAsync, CreateAsync, UpdateAsync, DeleteAsync
/// from ICrudAppService.
/// </summary>
public interface IOutletAppService
    : ICrudAppService<OutletDto, Guid, OutletFilterDto, CreateOutletDto, UpdateOutletDto>
{
    Task ApproveAsync(Guid id, ApproveOutletDto input);
    Task RejectAsync(Guid id, string reason);
    Task<PagedResultDto<OutletDto>> GetPendingAsync(PagedAndSortedResultRequestDto input);
    Task DeactivateAsync(Guid id, string reason);
}
