using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Hano.Core.Application.Contracts.Routes;

/// <summary>
/// Inherits: GetAsync, GetListAsync, CreateAsync, UpdateAsync, DeleteAsync
/// from ICrudAppService.
/// </summary>
public interface IRouteAppService
    : ICrudAppService<RouteDto, Guid, PagedAndSortedResultRequestDto, CreateRouteDto, UpdateRouteDto>
{
    Task ApproveAsync(Guid id, string? notes);
    Task RejectAsync(Guid id, string reason);
    Task<TodayRouteDto?> GetTodayAsync();
}
