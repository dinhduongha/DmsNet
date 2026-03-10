using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Hano.Core.Domain.Identity;

[Table("devices")]
public class Device : FullAuditedEntity<Guid>
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

    [Column("device_id")]
    public string DeviceId { get; set; } = null!;

    [Column("platform")]
    public string Platform { get; set; } = null!;

    [Column("model")]
    public string? Model { get; set; }

    [Column("os_version")]
    public string? OsVersion { get; set; }

    [Column("app_version")]
    public string? AppVersion { get; set; }

    [Column("fcm_token")]
    public string? FcmToken { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("bound_at")]
    public DateTimeOffset? BoundAt { get; set; }

    [Column("last_seen_at")]
    public DateTimeOffset? LastSeenAt { get; set; }
}
