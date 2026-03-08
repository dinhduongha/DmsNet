using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Hano.Core.Domain.Routes;

[Table("routes")]
public class Route : FullAuditedAggregateRoot<Guid>
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

    [Column("day_of_week")]
    public int DayOfWeek { get; set; }

    [Column("assigned_user_id")]
    public Guid? AssignedUserId { get; set; }

    [Column("created_by_user_id")]
    public Guid CreatedByUserId { get; set; }

    [Column("status")]
    public RouteStatus Status { get; set; } = RouteStatus.Draft;

    [Column("approved_by_user_id")]
    public Guid? ApprovedByUserId { get; set; }

    [Column("approved_at")]
    public DateTime? ApprovedAt { get; set; }

    [Column("reject_reason")]
    public string? RejectReason { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    public ICollection<RouteOutlet> Outlets { get; set; } = new List<RouteOutlet>();
}
