using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Hano.Core.Domain.Entities;

[Table("oos_report_items")]
public class OosReportItem : Entity<Guid>, IMultiTenant
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

    [Column("reason")]
    public string? Reason { get; set; }

    [Column("days_missing")]
    public string? DaysMissing { get; set; }

    [Column("impact")]
    public string? Impact { get; set; }
}
