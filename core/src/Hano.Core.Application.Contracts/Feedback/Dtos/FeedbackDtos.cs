using System;
using System.Collections.Generic;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Application.Dtos;

namespace Hano.Core.Application.Contracts.Feedback.Dtos;

public class CreateFeedbackDto
{
    public Guid Id { get; set; }
    public Guid VisitId { get; set; }
    public FeedbackType Type { get; set; }
    public string Category { get; set; } = null!;
    public Severity? Severity { get; set; }
    public string Content { get; set; } = null!;
    public Sentiment? Sentiment { get; set; }
    public FeedbackSource? Source { get; set; }
    public List<Guid> PhotoIds { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

public class UpdateFeedbackDto
{
    public string Content { get; set; } = null!;
    public Severity? Severity { get; set; }
    public Sentiment? Sentiment { get; set; }
}

public class FeedbackDto : EntityDto<Guid>
{
    public FeedbackType Type { get; set; }
    public string Category { get; set; } = null!;
    public Severity? Severity { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class FeedbackFilterDto : PagedAndSortedResultRequestDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public FeedbackType? Type { get; set; }
    public Severity? Severity { get; set; }
    public Guid? OutletId { get; set; }
}
