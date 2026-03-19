using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Hano.Core.Domain.Entities;

[Table("outlets")]
public class Outlet : FullAuditedAggregateRoot<Guid>, IMultiTenant
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

    [Column("address")]
    public string Address { get; set; } = null!;

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("outlet_type")]
    public OutletType OutletType { get; set; }

    [Column("latitude")]
    public decimal Latitude { get; set; }

    [Column("longitude")]
    public decimal Longitude { get; set; }

    [Column("channel")]
    public Channel? Channel { get; set; }

    [Column("size")]
    public OutletSize? Size { get; set; }

    [Column("visit_frequency")]
    public VisitFrequency? VisitFrequency { get; set; }

    [Column("status")]
    public OutletStatus Status { get; set; } = OutletStatus.PendingApproval;

    [Column("created_by_user_id")]
    public Guid CreatedByUserId { get; set; }

    [Column("approved_by_user_id")]
    public Guid? ApprovedByUserId { get; set; }

    [Column("approved_at")]
    public DateTimeOffset? ApprovedAt { get; set; }

    [Column("reject_reason")]
    public string? RejectReason { get; set; }

    [Column("ods_outlet_id")]
    public string? OdsOutletId { get; set; }

    [Column("sync_status")]
    public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;

    [Column("client_created_at")]
    public DateTimeOffset? ClientCreatedAt { get; set; }
}
