using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Hano.Core.Domain.Entities;

[Table("sync_queue")]
public class SyncQueue : Entity<Guid>
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

    [Column("entity_type")]
    public string EntityType { get; set; } = null!;

    [Column("entity_id")]
    public Guid EntityId { get; set; }

    [Column("action")]
    public string Action { get; set; } = null!;

    [Column("payload")]
    public string Payload { get; set; } = null!;

    [Column("client_timestamp")]
    public DateTimeOffset? ClientTimestamp { get; set; }

    [Column("server_received_at")]
    public DateTimeOffset? ServerReceivedAt { get; set; }

    [Column("status")]
    public SyncQueueStatus Status { get; set; } = SyncQueueStatus.Pending;

    [Column("retry_count")]
    public int RetryCount { get; set; }

    [Column("error_message")]
    public string? ErrorMessage { get; set; }
}
