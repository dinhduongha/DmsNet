using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hano.Core.Application.Contracts.Sessions;
using Hano.Core.Application.Contracts.Sessions.Dtos;
using Hano.Core.Domain.Orders;
using Hano.Core.Domain.Sessions;
using Hano.Core.Domain.Visits;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace Hano.Core.Application.Sessions;

public class SessionAppService : HanoCoreAppServiceBase, ISessionAppService
{
    private readonly IRepository<WorkSession, Guid> _sessionRepo;
    private readonly IRepository<GpsBreadcrumb, Guid> _bcRepo;
    private readonly IRepository<Visit, Guid> _visitRepo;
    private readonly IRepository<VehicleStock, Guid> _vsRepo;

    public SessionAppService(
        IRepository<WorkSession, Guid> sessionRepo,
        IRepository<GpsBreadcrumb, Guid> bcRepo,
        IRepository<Visit, Guid> visitRepo,
        IRepository<VehicleStock, Guid> vsRepo)
    {
        _sessionRepo = sessionRepo;
        _bcRepo = bcRepo;
        _visitRepo = visitRepo;
        _vsRepo = vsRepo;
    }

    public async Task<SessionDto> StartAsync(SessionStartDto input)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var existing = await _sessionRepo.FirstOrDefaultAsync(x => x.UserId == CurrentUserId && x.Date == today);
        if (existing is { Status: SessionStatus.Active })
            throw new UserFriendlyException("Đã có phiên đang hoạt động.");

        var session = new WorkSession
        {
            Id = GuidGenerator.Create(), UserId = CurrentUserId, Date = today,
            SodTimestamp = DateTime.UtcNow, SodLatitude = input.Latitude, SodLongitude = input.Longitude,
            SodSelfiePhotoId = input.SelfiePhotoId,
        };
        await _sessionRepo.InsertAsync(session);

        if (input.VehicleStockItems?.Count > 0)
            foreach (var item in input.VehicleStockItems)
                await _vsRepo.InsertAsync(new VehicleStock { Id = GuidGenerator.Create(), SessionId = session.Id, UserId = CurrentUserId, SkuId = item.SkuId, Date = today, OpeningQty = item.Quantity, CurrentQty = item.Quantity });

        return new SessionDto { Id = session.Id, Status = session.Status, StartedAt = session.SodTimestamp };
    }

    public async Task<SessionDto> EndAsync(Guid sessionId, decimal latitude, decimal longitude)
    {
        var s = await _sessionRepo.GetAsync(sessionId);
        if (s.Status != SessionStatus.Active) throw new UserFriendlyException("Phiên đã kết thúc.");
        s.EodTimestamp = DateTime.UtcNow; s.EodLatitude = latitude; s.EodLongitude = longitude;
        s.Status = SessionStatus.Completed;
        var visits = await _visitRepo.GetListAsync(x => x.SessionId == sessionId);
        s.TotalVisits = visits.Count(v => v.Status == VisitStatus.Completed);
        await _sessionRepo.UpdateAsync(s);
        return new SessionDto { Id = s.Id, Status = s.Status, StartedAt = s.SodTimestamp, EndedAt = s.EodTimestamp };
    }

    public async Task<SessionDto?> GetCurrentAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var s = await _sessionRepo.FirstOrDefaultAsync(x => x.UserId == CurrentUserId && x.Date == today && x.Status == SessionStatus.Active);
        return s == null ? null : new SessionDto { Id = s.Id, Status = s.Status, StartedAt = s.SodTimestamp };
    }

    public async Task SendBreadcrumbsAsync(Guid sessionId, List<BreadcrumbDto> points)
    {
        foreach (var p in points)
            await _bcRepo.InsertAsync(new GpsBreadcrumb { Id = GuidGenerator.Create(), SessionId = sessionId, Latitude = p.Lat, Longitude = p.Lng, Accuracy = p.Accuracy, Timestamp = p.Timestamp });
    }

    public async Task<SessionSummaryDto> GetSummaryAsync(Guid sessionId)
    {
        var s = await _sessionRepo.GetAsync(sessionId);
        return new SessionSummaryDto { TotalVisits = s.TotalVisits, TotalOrders = s.TotalOrders, TotalRevenue = s.TotalRevenue, TotalDistanceKm = s.TotalDistanceKm, WorkDurationMinutes = (int)(((s.EodTimestamp ?? DateTime.UtcNow) - s.SodTimestamp).TotalMinutes) };
    }
}
