using System;
using System.Linq;
using System.Threading.Tasks;
using Hano.Core.Application.Contracts.Audit;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace Hano.Core.Application.Audit;

public class AuditAppService : HanoCoreAppServiceBase, IAuditAppService
{
    private readonly IRepository<OsaReport, Guid> _osaRepo;
    private readonly IRepository<OosReport, Guid> _oosRepo;
    private readonly IRepository<PosmReport, Guid> _posmRepo;
    private readonly IRepository<Visit, Guid> _visitRepo;
    private readonly IRepository<Photo, Guid> _photoRepo;

    public AuditAppService(
        IRepository<OsaReport, Guid> osaRepo,
        IRepository<OosReport, Guid> oosRepo,
        IRepository<PosmReport, Guid> posmRepo,
        IRepository<Visit, Guid> visitRepo,
        IRepository<Photo, Guid> photoRepo)
    {
        _osaRepo = osaRepo;
        _oosRepo = oosRepo;
        _posmRepo = posmRepo;
        _visitRepo = visitRepo;
        _photoRepo = photoRepo;
    }

    public async Task<Guid> CreateOsaAsync(OsaReportInputDto input)
    {
        // Idempotency: check if report with same Id already exists
        var existing = await _osaRepo.FirstOrDefaultAsync(x => x.Id == input.Id);
        if (existing != null) return existing.Id;

        var visit = await _visitRepo.GetAsync(input.VisitId);
        if (visit.Status != VisitStatus.InProgress)
            throw new UserFriendlyException("Visit phải ở trạng thái InProgress.");

        var report = new OsaReport
        {
            Id = input.Id == Guid.Empty ? GuidGenerator.Create() : input.Id,
            VisitId = input.VisitId,
            UserId = CurrentUserId,
            Notes = input.Notes,
            ClientCreatedAt = input.Timestamp,
        };

        foreach (var item in input.Items)
        {
            report.Items.Add(new OsaReportItem
            {
                Id = GuidGenerator.Create(),
                ReportId = report.Id,
                SkuId = item.SkuId,
                IsPresent = item.IsPresent,
                FacingCount = item.FacingCount,
                ShelfPosition = item.ShelfPosition,
                Condition = item.Condition,
            });
        }

        await _osaRepo.InsertAsync(report);

        // Link photos to visit
        foreach (var photoId in input.PhotoIds)
        {
            var photo = await _photoRepo.FindAsync(photoId);
            if (photo != null)
            {
                photo.VisitId = input.VisitId;
                await _photoRepo.UpdateAsync(photo);
            }
        }

        // Increment visit activities count
        visit.ActivitiesCount++;
        await _visitRepo.UpdateAsync(visit);

        return report.Id;
    }

    public async Task<Guid> CreateOosAsync(OosReportInputDto input)
    {
        var existing = await _oosRepo.FirstOrDefaultAsync(x => x.Id == input.Id);
        if (existing != null) return existing.Id;

        var visit = await _visitRepo.GetAsync(input.VisitId);
        if (visit.Status != VisitStatus.InProgress)
            throw new UserFriendlyException("Visit phải ở trạng thái InProgress.");

        var report = new OosReport
        {
            Id = input.Id == Guid.Empty ? GuidGenerator.Create() : input.Id,
            VisitId = input.VisitId,
            UserId = CurrentUserId,
            Notes = input.Notes,
            ClientCreatedAt = input.Timestamp,
        };

        foreach (var item in input.Items)
        {
            report.Items.Add(new OosReportItem
            {
                Id = GuidGenerator.Create(),
                ReportId = report.Id,
                SkuId = item.SkuId,
                Reason = item.Reason,
                DaysMissing = item.DaysMissing,
            });
        }

        await _oosRepo.InsertAsync(report);

        visit.ActivitiesCount++;
        await _visitRepo.UpdateAsync(visit);

        return report.Id;
    }

    public async Task<Guid> CreatePosmAsync(PosmReportInputDto input)
    {
        var existing = await _posmRepo.FirstOrDefaultAsync(x => x.Id == input.Id);
        if (existing != null) return existing.Id;

        var visit = await _visitRepo.GetAsync(input.VisitId);
        if (visit.Status != VisitStatus.InProgress)
            throw new UserFriendlyException("Visit phải ở trạng thái InProgress.");

        var report = new PosmReport
        {
            Id = input.Id == Guid.Empty ? GuidGenerator.Create() : input.Id,
            VisitId = input.VisitId,
            UserId = CurrentUserId,
            Notes = input.Notes,
            ClientCreatedAt = input.Timestamp,
        };

        foreach (var item in input.Items)
        {
            report.Items.Add(new PosmReportItem
            {
                Id = GuidGenerator.Create(),
                ReportId = report.Id,
                PosmItemId = item.PosmItemId,
                IsPresent = item.IsPresent,
                Condition = item.Condition,
                CorrectPosition = item.CorrectPosition,
                NeedsReplacement = item.NeedsReplacement,
            });
        }

        await _posmRepo.InsertAsync(report);

        foreach (var photoId in input.PhotoIds)
        {
            var photo = await _photoRepo.FindAsync(photoId);
            if (photo != null)
            {
                photo.VisitId = input.VisitId;
                await _photoRepo.UpdateAsync(photo);
            }
        }

        visit.ActivitiesCount++;
        await _visitRepo.UpdateAsync(visit);

        return report.Id;
    }
}
