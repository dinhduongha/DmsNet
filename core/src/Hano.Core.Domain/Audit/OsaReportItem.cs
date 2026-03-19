using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Hano.Core.Domain.Entities;

[Table("osa_report_items")]
public class OsaReportItem : Entity<Guid>
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

    [Column("report_id")]
    public Guid ReportId { get; set; }

    [Column("sku_id")]
    public Guid SkuId { get; set; }

    [Column("is_present")]
    public bool IsPresent { get; set; }

    [Column("facing_count")]
    public int? FacingCount { get; set; }

    [Column("shelf_position")]
    public ShelfPosition? ShelfPosition { get; set; }

    [Column("condition")]
    public SkuCondition? Condition { get; set; }
}
