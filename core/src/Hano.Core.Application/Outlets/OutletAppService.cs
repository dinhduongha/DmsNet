using System;
using System.Linq;
using System.Threading.Tasks;
using Hano.Core.Application.Contracts.Outlets;
using Hano.Core.Application.Contracts.Outlets.Dtos;
using Hano.Core.Application.Mappers;
using Hano.Core.Domain.Outlets;
using Hano.Core.Domain.Routes;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Hano.Core.Application.Outlets;

public class OutletAppService
    : CrudAppService<Outlet, OutletDto, Guid, OutletFilterDto, CreateOutletDto, UpdateOutletDto>,
      IOutletAppService
{
    private readonly IRepository<RouteOutlet, Guid> _routeOutletRepo;

    public OutletAppService(
        IRepository<Outlet, Guid> repository,
        IRepository<RouteOutlet, Guid> routeOutletRepo)
        : base(repository)
    {
        _routeOutletRepo = routeOutletRepo;
    }

    // ── Mapperly overrides (no AutoMapper) ──

    protected override OutletDto MapToGetOutputDto(Outlet entity) => entity.ToDto();
    protected override OutletDto MapToGetListOutputDto(Outlet entity) => entity.ToDto();

    protected override async Task<Outlet> MapToEntityAsync(CreateOutletDto input)
    {
        return new Outlet
        {
            Id = input.Id == Guid.Empty ? GuidGenerator.Create() : input.Id,
            Name = input.Name,
            Address = input.Address,
            Phone = input.Phone,
            OutletType = input.OutletType,
            Latitude = input.Latitude,
            Longitude = input.Longitude,
            Status = OutletStatus.PendingApproval,
            CreatedByUserId = CurrentUser.Id!.Value,
        };
    }

    protected override async Task MapToEntityAsync(UpdateOutletDto input, Outlet entity)
    {
        entity.Name = input.Name;
        entity.Address = input.Address;
        entity.Phone = input.Phone;
        entity.OutletType = input.OutletType;
        entity.Latitude = input.Latitude;
        entity.Longitude = input.Longitude;
    }

    // ── Override CreateFilteredQueryAsync for custom filter logic ──

    protected override async Task<IQueryable<Outlet>> CreateFilteredQueryAsync(OutletFilterDto input)
    {
        var q = await base.CreateFilteredQueryAsync(input);

        if (input.Status.HasValue)
            q = q.Where(x => x.Status == input.Status.Value);
        if (input.Channel.HasValue)
            q = q.Where(x => x.Channel == input.Channel.Value);
        if (!string.IsNullOrWhiteSpace(input.Search))
            q = q.Where(x => x.Name.Contains(input.Search) || x.Address.Contains(input.Search));

        if (input.RouteId.HasValue)
        {
            var routeOutletIds = (await _routeOutletRepo.GetQueryableAsync())
                .Where(ro => ro.RouteId == input.RouteId.Value)
                .Select(ro => ro.OutletId);
            q = q.Where(x => routeOutletIds.Contains(x.Id));
        }

        return q;
    }

    // ── Custom business methods ──

    public async Task ApproveAsync(Guid id, ApproveOutletDto input)
    {
        var outlet = await Repository.GetAsync(id);
        if (outlet.Status != OutletStatus.PendingApproval)
            throw new UserFriendlyException("Outlet không ở trạng thái chờ duyệt.");

        outlet.Status = OutletStatus.Approved;
        outlet.Channel = input.Channel;
        outlet.Size = input.Size;
        outlet.VisitFrequency = input.VisitFrequency;
        outlet.ApprovedByUserId = CurrentUser.Id!.Value;
        outlet.ApprovedAt = DateTime.UtcNow;
        await Repository.UpdateAsync(outlet);

        if (input.RouteId.HasValue)
        {
            var maxSeq = (await _routeOutletRepo.GetQueryableAsync())
                .Where(x => x.RouteId == input.RouteId.Value)
                .Select(x => (int?)x.SequenceOrder)
                .Max() ?? 0;

            await _routeOutletRepo.InsertAsync(new RouteOutlet
            {
                Id = GuidGenerator.Create(),
                RouteId = input.RouteId.Value,
                OutletId = outlet.Id,
                SequenceOrder = maxSeq + 1,
            });
        }
    }

    public async Task RejectAsync(Guid id, string reason)
    {
        var outlet = await Repository.GetAsync(id);
        if (outlet.Status != OutletStatus.PendingApproval)
            throw new UserFriendlyException("Outlet không ở trạng thái chờ duyệt.");

        outlet.Status = OutletStatus.Rejected;
        outlet.RejectReason = reason;
        await Repository.UpdateAsync(outlet);
    }

    public async Task<PagedResultDto<OutletDto>> GetPendingAsync(PagedAndSortedResultRequestDto input)
    {
        var q = (await Repository.GetQueryableAsync())
            .Where(x => x.Status == OutletStatus.PendingApproval);

        var total = q.Count();
        var items = q
            .OrderByDescending(x => x.CreationTime)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new PagedResultDto<OutletDto>(total, items.Select(MapToGetListOutputDto).ToList());
    }

    public async Task DeactivateAsync(Guid id, string reason)
    {
        var outlet = await Repository.GetAsync(id);
        outlet.Status = OutletStatus.Inactive;
        outlet.RejectReason = reason;
        await Repository.UpdateAsync(outlet);
    }
}
