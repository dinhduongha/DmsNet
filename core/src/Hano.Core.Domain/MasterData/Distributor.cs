using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Hano.Core.Domain.Entities;

[Table("distributors")]
public class Distributor : AuditedEntity<Guid>
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

    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("address")]
    public string? Address { get; set; }

    [Column("region")]
    public string? Region { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("ods_distributor_id")]
    public string? OdsDistributorId { get; set; }

    [Column("last_synced_at")]
    public DateTimeOffset? LastSyncedAt { get; set; }
}
