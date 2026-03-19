using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Hano.Core.Domain.Entities;

[Table("feedback_reports")]
public class FeedbackReport : FullAuditedAggregateRoot<Guid>
{
    [Key]
    [Column("id")]
    public Guid Id { get => base.Id; set => base.Id = value; }

    [Column("tenant_id")]
    public Guid? TenantId { get; set; }

    [Column("organization_id")]
    public Guid? OrganizationUnitId { get; set; }

    [Column("team_id")]
    public Guid? TeamId { get; set; }

    [Column("visit_id")]
    public Guid VisitId { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("type")]
    public FeedbackType Type { get; set; }

    [Column("category")]
    public string Category { get; set; } = null!;

    [Column("severity")]
    public Severity? Severity { get; set; }

    [Column("content")]
    public string Content { get; set; } = null!;

    [Column("sentiment")]
    public Sentiment? Sentiment { get; set; }

    [Column("source")]
    public FeedbackSource? Source { get; set; }

    [Column("tags")]
    public string? Tags { get; set; }

    [Column("sync_status")]
    public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;

    [Column("client_created_at")]
    public DateTimeOffset? ClientCreatedAt { get; set; }
}
