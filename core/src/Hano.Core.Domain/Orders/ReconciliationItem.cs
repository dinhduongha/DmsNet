using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Hano.Core.Domain.Entities;

[Table("reconciliation_items")]
public class ReconciliationItem : Entity<Guid>
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

    [Column("reconciliation_id")]
    public Guid ReconciliationId { get; set; }

    [Column("sku_id")]
    public Guid SkuId { get; set; }

    [Column("system_qty")]
    public int SystemQty { get; set; }

    [Column("physical_qty")]
    public int PhysicalQty { get; set; }

    [Column("return_qty")]
    public int ReturnQty { get; set; }

    [Column("damaged_qty")]
    public int DamagedQty { get; set; }

    [Column("reason")]
    public string? Reason { get; set; }
}
