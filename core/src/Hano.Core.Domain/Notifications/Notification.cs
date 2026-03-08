using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Hano.Core.Domain.Notifications;

[Table("notifications")]
public class Notification : CreationAuditedEntity<Guid>
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

    [Column("target_user_id")]
    public Guid TargetUserId { get; set; }

    [Column("type")]
    public string Type { get; set; } = null!;

    [Column("title")]
    public string Title { get; set; } = null!;

    [Column("body")]
    public string Body { get; set; } = null!;

    [Column("priority")]
    public NotificationPriority Priority { get; set; }

    [Column("is_read")]
    public bool IsRead { get; set; }

    [Column("read_at")]
    public DateTime? ReadAt { get; set; }

    [Column("deeplink")]
    public string? Deeplink { get; set; }

    [Column("related_data")]
    public string? RelatedData { get; set; }

    [Column("sender_user_id")]
    public Guid? SenderUserId { get; set; }

    [Column("expiry_time")]
    public DateTime? ExpiryTime { get; set; }
}
