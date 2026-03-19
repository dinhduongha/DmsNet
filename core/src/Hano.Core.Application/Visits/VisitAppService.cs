using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hano.Core.Application.Contracts.Visits;
using Hano.Core.Application.Mappers;
using Hano.Core.Domain.Shared;
using Hano.Core.Domain.Shared.Enums;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace Hano.Core.Application.Visits;

public class VisitAppService : HanoCoreAppServiceBase, IVisitAppService
{
    private readonly IRepository<Visit, Guid> _visitRepo;
    private readonly IRepository<Outlet, Guid> _outletRepo;
    private readonly IDistributedCache _distrubeCache;
    public VisitAppService(IRepository<Visit, Guid> visitRepo, IRepository<Outlet, Guid> outletRepo, IDistributedCache distrubeCache)
    {
        _visitRepo = visitRepo; _outletRepo = outletRepo;
        _distrubeCache = distrubeCache;
    }

    public async Task<CheckinResultDto> CheckinAsync(CheckinDto input)
    {
        var existing = await _visitRepo.FirstOrDefaultAsync(x => x.Id == input.Id);
        if (existing != null)
            return new CheckinResultDto { VisitId = existing.Id, GpsDistanceM = existing.GpsDistanceM ?? 0, GpsFlag = existing.GpsFlag?.ToString() ?? "Unavailable" };

        var outlet = await _outletRepo.GetAsync(input.OutletId);
        var dist = Haversine((double)input.Latitude, (double)input.Longitude, (double)outlet.Latitude, (double)outlet.Longitude);
        var flag = dist <= HanoCoreConsts.GpsOkMeters ? GpsFlag.Ok : dist <= HanoCoreConsts.GpsWarningMeters ? GpsFlag.Warning : GpsFlag.Violation;

        var visit = new Visit { Id = input.Id, SessionId = input.SessionId, OutletId = input.OutletId, UserId = CurrentUserId, Status = VisitStatus.InProgress, CheckinAt = input.Timestamp, CheckinLatitude = input.Latitude, CheckinLongitude = input.Longitude, GpsDistanceM = (decimal)dist, GpsFlag = flag, ClientCreatedAt = input.Timestamp };
        await _visitRepo.InsertAsync(visit);

        return new CheckinResultDto { VisitId = visit.Id, GpsDistanceM = (decimal)dist, GpsFlag = flag.ToString(), WarningMessage = flag == GpsFlag.Violation ? $"GPS cách outlet {dist:F0}m" : null };
    }

    public async Task<VisitDto> CheckoutAsync(Guid visitId, CheckoutDto input)
    {
        var v = await _visitRepo.GetAsync(visitId);
        if (v.Status != VisitStatus.InProgress) throw new UserFriendlyException("Visit không InProgress.");
        v.CheckoutAt = DateTime.UtcNow; v.CheckoutLatitude = input.Latitude; v.CheckoutLongitude = input.Longitude;
        v.DurationMinutes = v.CheckinAt.HasValue ? (int)(v.CheckoutAt.Value - v.CheckinAt.Value).TotalMinutes : 0;
        v.Status = VisitStatus.Completed;
        await _visitRepo.UpdateAsync(v);
        var o = await _outletRepo.GetAsync(v.OutletId);
        return v.ToDtoWithOutlet(o.Name);
    }

    public async Task<VisitDto> SkipAsync(Guid visitId, SkipVisitDto input)
    {
        var v = await _visitRepo.GetAsync(visitId);
        v.Status = VisitStatus.Skipped; v.SkipReason = string.IsNullOrEmpty(input.ReasonNote) ? input.Reason : $"{input.Reason}: {input.ReasonNote}";
        await _visitRepo.UpdateAsync(v);
        var o = await _outletRepo.GetAsync(v.OutletId);
        return v.ToDtoWithOutlet(o.Name);
    }

    public async Task<List<VisitDto>> GetTodayAsync()
    {
        var today = DateTime.UtcNow.Date;
        var visits = await _visitRepo.GetListAsync(x => x.UserId == CurrentUserId && x.ClientCreatedAt >= today);
        var outlets = (await _outletRepo.GetListAsync(x => visits.Select(vv => vv.OutletId).Contains(x.Id))).ToDictionary(o => o.Id);
        return visits.Select(v => v.ToDtoWithOutlet(outlets.GetValueOrDefault(v.OutletId)?.Name ?? "")).ToList();
    }

    public async Task<VisitDto> GetAsync(Guid visitId)
    {
        var v = await _visitRepo.GetAsync(visitId);
        var o = await _outletRepo.GetAsync(v.OutletId);
        return v.ToDtoWithOutlet(o.Name);
    }

    static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000;
        var dLat = (lat2 - lat1) * Math.PI / 180; var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
