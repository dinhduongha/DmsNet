using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hano.Core.Application.Contracts.Routes;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Hano.Core.Application.Routes;

public class RouteAppService
    : CrudAppService<Route, RouteDto, Guid, Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto, CreateRouteDto, UpdateRouteDto>,
      IRouteAppService
{
    private readonly IRepository<RouteOutlet, Guid> _routeOutletRepo;
    private readonly IRepository<Outlet, Guid> _outletRepo;
    private readonly IRepository<Visit, Guid> _visitRepo;

    public RouteAppService(
        IRepository<Route, Guid> repository,
        IRepository<RouteOutlet, Guid> routeOutletRepo,
        IRepository<Outlet, Guid> outletRepo,
        IRepository<Visit, Guid> visitRepo)
        : base(repository)
    {
        _routeOutletRepo = routeOutletRepo;
        _outletRepo = outletRepo;
        _visitRepo = visitRepo;
    }

    // ── Mapperly overrides ──

    protected override RouteDto MapToGetOutputDto(Route entity) => BuildRouteDto(entity).Result;
    protected override RouteDto MapToGetListOutputDto(Route entity) => BuildRouteDto(entity).Result;

    protected override async Task<Route> MapToEntityAsync(CreateRouteDto input)
    {
        return new Route
        {
            Id = GuidGenerator.Create(),
            Name = input.Name,
            DayOfWeek = input.DayOfWeek,
            AssignedUserId = input.NvbhId,
            CreatedByUserId = CurrentUser.Id!.Value,
            Status = RouteStatus.PendingApproval,
        };
    }

    protected override async Task MapToEntityAsync(UpdateRouteDto input, Route entity)
    {
        entity.Name = input.Name;
        entity.DayOfWeek = input.DayOfWeek;
        entity.AssignedUserId = input.NvbhId;
        entity.Status = RouteStatus.PendingApproval; // re-submit
    }

    // ── Override Create/Update to handle RouteOutlets ──

    public override async Task<RouteDto> CreateAsync(CreateRouteDto input)
    {
        var route = await MapToEntityAsync(input);
        await Repository.InsertAsync(route);

        await ReplaceRouteOutletsAsync(route.Id, input.OutletIds);

        return await BuildRouteDto(route);
    }

    public override async Task<RouteDto> UpdateAsync(Guid id, UpdateRouteDto input)
    {
        var route = await Repository.GetAsync(id);
        if (route.Status == RouteStatus.Approved)
            throw new UserFriendlyException("Không thể sửa route đã duyệt. Hãy tạo route mới.");

        await MapToEntityAsync(input, route);
        await Repository.UpdateAsync(route);

        await ReplaceRouteOutletsAsync(route.Id, input.OutletIds);

        return await BuildRouteDto(route);
    }

    // ── Custom business methods ──

    public async Task ApproveAsync(Guid id, string? notes)
    {
        var route = await Repository.GetAsync(id);
        if (route.Status != RouteStatus.PendingApproval)
            throw new UserFriendlyException("Route không ở trạng thái chờ duyệt.");

        route.Status = RouteStatus.Approved;
        route.ApprovedByUserId = CurrentUser.Id!.Value;
        route.ApprovedAt = DateTime.UtcNow;
        await Repository.UpdateAsync(route);
    }

    public async Task RejectAsync(Guid id, string reason)
    {
        var route = await Repository.GetAsync(id);
        if (route.Status != RouteStatus.PendingApproval)
            throw new UserFriendlyException("Route không ở trạng thái chờ duyệt.");

        route.Status = RouteStatus.Rejected;
        route.RejectReason = reason;
        await Repository.UpdateAsync(route);
    }

    public async Task<TodayRouteDto?> GetTodayAsync()
    {
        var todayDow = (int)DateTime.UtcNow.DayOfWeek;
        var userId = CurrentUser.Id!.Value;
        var route = await Repository.FirstOrDefaultAsync(
            x => x.AssignedUserId == userId
                && x.DayOfWeek == todayDow
                && x.Status == RouteStatus.Approved
                && x.IsActive);

        if (route == null) return null;

        var routeOutlets = await _routeOutletRepo.GetListAsync(x => x.RouteId == route.Id);
        var outletIds = routeOutlets.Select(ro => ro.OutletId).ToList();
        var outlets = (await _outletRepo.GetListAsync(x => outletIds.Contains(x.Id))).ToDictionary(o => o.Id);

        var todayStart = DateTime.UtcNow.Date;
        var todayVisits = await _visitRepo.GetListAsync(
            x => x.UserId == userId && x.ClientCreatedAt >= todayStart);
        var visitMap = todayVisits.ToDictionary(v => v.OutletId, v => v.Status.ToString());

        return new TodayRouteDto
        {
            RouteId = route.Id,
            RouteName = route.Name,
            Outlets = routeOutlets.OrderBy(ro => ro.SequenceOrder)
                .Select(ro => new RouteOutletDto
                {
                    OutletId = ro.OutletId,
                    OutletName = outlets.GetValueOrDefault(ro.OutletId)?.Name ?? "",
                    SequenceOrder = ro.SequenceOrder,
                    VisitStatus = visitMap.GetValueOrDefault(ro.OutletId),
                }).ToList(),
        };
    }

    // ── Helpers ──

    private async Task ReplaceRouteOutletsAsync(Guid routeId, List<Guid> outletIds)
    {
        var existing = await _routeOutletRepo.GetListAsync(x => x.RouteId == routeId);
        if (existing.Any())
            await _routeOutletRepo.DeleteManyAsync(existing);

        for (var i = 0; i < outletIds.Count; i++)
        {
            await _routeOutletRepo.InsertAsync(new RouteOutlet
            {
                Id = GuidGenerator.Create(),
                RouteId = routeId,
                OutletId = outletIds[i],
                SequenceOrder = i + 1,
            });
        }
    }

    private async Task<RouteDto> BuildRouteDto(Route route)
    {
        var routeOutlets = await _routeOutletRepo.GetListAsync(x => x.RouteId == route.Id);
        var outletIds = routeOutlets.Select(ro => ro.OutletId).ToList();
        var outlets = (await _outletRepo.GetListAsync(x => outletIds.Contains(x.Id))).ToDictionary(o => o.Id);

        return new RouteDto
        {
            Id = route.Id,
            Name = route.Name,
            DayOfWeek = route.DayOfWeek,
            Status = route.Status,
            TotalOutlets = routeOutlets.Count,
            Outlets = routeOutlets.OrderBy(ro => ro.SequenceOrder)
                .Select(ro => new RouteOutletDto
                {
                    OutletId = ro.OutletId,
                    OutletName = outlets.GetValueOrDefault(ro.OutletId)?.Name ?? "",
                    SequenceOrder = ro.SequenceOrder,
                }).ToList(),
        };
    }
}
