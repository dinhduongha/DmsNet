using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Hano.Core.Domain.Entities;

[Table("order_lines")]
public class OrderLine : Entity<Guid>
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

    [Column("order_id")]
    public Guid OrderId { get; set; }

    [Column("sku_id")]
    public Guid SkuId { get; set; }

    [Column("sku_code")]
    public string SkuCode { get; set; } = null!;

    [Column("sku_name")]
    public string SkuName { get; set; } = null!;

    [Column("quantity")]
    public decimal Quantity { get; set; }

    [Column("unit")]
    public string Unit { get; set; } = null!;

    [Column("unit_price")]
    public decimal UnitPrice { get; set; }

    [Column("line_total")]
    public decimal LineTotal { get; set; }

    [Column("discount")]
    public decimal Discount { get; set; }
}
