using System;
using System.Linq;
using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
using Hano.Core.Application.Contracts.Feedback;
using Hano.Core.Application.Mappers;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Hano.Core.Application.Feedback;

public class FeedbackAppService
    : CrudAppService<FeedbackReport, FeedbackDto, Guid, FeedbackFilterDto, CreateFeedbackDto, UpdateFeedbackDto>,
      IFeedbackAppService
{
    private readonly IRepository<Visit, Guid> _visitRepo;
    private readonly IRepository<Photo, Guid> _photoRepo;

    public FeedbackAppService(
        IRepository<FeedbackReport, Guid> repository,
        IRepository<Visit, Guid> visitRepo,
        IRepository<Photo, Guid> photoRepo)
        : base(repository)
    {
        _visitRepo = visitRepo;
        _photoRepo = photoRepo;
    }

    // ── Mapperly overrides ──

    protected override FeedbackDto MapToGetOutputDto(FeedbackReport entity) => entity.ToDto();
    protected override FeedbackDto MapToGetListOutputDto(FeedbackReport entity) => entity.ToDto();

    protected override async Task<FeedbackReport> MapToEntityAsync(CreateFeedbackDto input)
    {
        return new FeedbackReport
        {
            Id = input.Id == Guid.Empty ? GuidGenerator.Create() : input.Id,
            VisitId = input.VisitId,
            UserId = CurrentUser.Id!.Value,
            Type = input.Type,
            Category = input.Category,
            Severity = input.Severity,
            Content = input.Content,
            Sentiment = input.Sentiment,
            Source = input.Source,
            Tags = input.Tags.Count > 0 ? string.Join(",", input.Tags) : null,
            ClientCreatedAt = input.Timestamp,
        };
    }

    protected override async Task MapToEntityAsync(UpdateFeedbackDto input, FeedbackReport entity)
    {
        entity.Content = input.Content;
        entity.Severity = input.Severity;
        entity.Sentiment = input.Sentiment;
    }

    // ── Override CreateAsync for business logic ──

    public override async Task<FeedbackDto> CreateAsync(CreateFeedbackDto input)
    {
        // Idempotency
        if (input.Id != Guid.Empty)
        {
            var existing = await Repository.FirstOrDefaultAsync(x => x.Id == input.Id);
            if (existing != null) return MapToGetOutputDto(existing);
        }

        var visit = await _visitRepo.GetAsync(input.VisitId);
        if (visit.Status != VisitStatus.InProgress)
            throw new UserFriendlyException("Visit phải ở trạng thái InProgress.");

        var entity = await MapToEntityAsync(input);
        await Repository.InsertAsync(entity);

        // Link photos
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

        return MapToGetOutputDto(entity);
    }

    // ── Override CreateFilteredQueryAsync for custom filters ──

    protected override async Task<IQueryable<FeedbackReport>> CreateFilteredQueryAsync(FeedbackFilterDto input)
    {
        var q = await base.CreateFilteredQueryAsync(input);
        q = q.Where(x => x.UserId == CurrentUser.Id!.Value);

        if (input.FromDate.HasValue)
            q = q.Where(x => x.ClientCreatedAt >= input.FromDate.Value);
        if (input.ToDate.HasValue)
            q = q.Where(x => x.ClientCreatedAt <= input.ToDate.Value);
        if (input.Type.HasValue)
            q = q.Where(x => x.Type == input.Type.Value);
        if (input.Severity.HasValue)
            q = q.Where(x => x.Severity == input.Severity.Value);

        return q;
    }
}
