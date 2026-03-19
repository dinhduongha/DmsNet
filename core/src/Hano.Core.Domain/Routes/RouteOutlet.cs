using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Hano.Core.Domain.Entities;

[Table("route_outlets")]
public class RouteOutlet : Entity<Guid>, IMultiTenant
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

    [Column("route_id")]
    public Guid RouteId { get; set; }

    [Column("outlet_id")]
    public Guid OutletId { get; set; }

    [Column("sequence_order")]
    public int SequenceOrder { get; set; }

    [Column("sync_status")]
    public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;
}
