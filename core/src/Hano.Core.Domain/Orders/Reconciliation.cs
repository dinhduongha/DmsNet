using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Hano.Core.Domain.Entities;

[Table("reconciliations")]
public class Reconciliation : FullAuditedAggregateRoot<Guid>
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

    [Column("session_id")]
    public Guid SessionId { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("date")]
    public DateOnly Date { get; set; }

    [Column("has_discrepancy")]
    public bool HasDiscrepancy { get; set; }

    [Column("total_sold_amount")]
    public decimal TotalSoldAmount { get; set; }

    [Column("total_collected")]
    public decimal TotalCollected { get; set; }

    [Column("discrepancy_notes")]
    public string? DiscrepancyNotes { get; set; }

    public ICollection<ReconciliationItem> Items { get; set; } = new List<ReconciliationItem>();
}
