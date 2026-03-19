using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Hano.Core.Domain.Entities;

[Table("daily_reports")]
public class DailyReport : FullAuditedEntity<Guid>
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

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("session_id")]
    public Guid SessionId { get; set; }

    [Column("date")]
    public DateOnly Date { get; set; }

    [Column("report_type")]
    public string ReportType { get; set; } = null!;

    [Column("summary")]
    public string? Summary { get; set; }

    [Column("issues")]
    public string? Issues { get; set; }

    [Column("suggestions")]
    public string? Suggestions { get; set; }

    [Column("audio_url")]
    public string? AudioUrl { get; set; }

    [Column("transcribed_text")]
    public string? TranscribedText { get; set; }

    [Column("audio_duration_seconds")]
    public int? AudioDurationSeconds { get; set; }

    [Column("sync_status")]
    public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;

    [Column("client_created_at")]
    public DateTimeOffset? ClientCreatedAt { get; set; }
}
