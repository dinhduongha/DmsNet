using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Hano.Core.Domain.Orders;

[Table("vehicle_stocks")]
public class VehicleStock : AuditedEntity<Guid>
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

    [Column("sku_id")]
    public Guid SkuId { get; set; }

    [Column("date")]
    public DateOnly Date { get; set; }

    [Column("opening_qty")]
    public int OpeningQty { get; set; }

    [Column("sold_qty")]
    public int SoldQty { get; set; }

    [Column("return_qty")]
    public int ReturnQty { get; set; }

    [Column("damaged_qty")]
    public int DamagedQty { get; set; }

    [Column("current_qty")]
    public int CurrentQty { get; set; }
}
