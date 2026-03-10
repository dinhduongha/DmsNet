using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Hano.Core.Domain.MasterData;

[Table("price_lists")]
public class PriceList : AuditedEntity<Guid>
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

    [Column("sku_id")]
    public Guid SkuId { get; set; }

    [Column("distributor_id")]
    public Guid? DistributorId { get; set; }

    [Column("unit_price")]
    public decimal UnitPrice { get; set; }

    [Column("promo_price")]
    public decimal? PromoPrice { get; set; }

    [Column("effective_from")]
    public DateOnly EffectiveFrom { get; set; }

    [Column("effective_to")]
    public DateOnly? EffectiveTo { get; set; }

    [Column("last_synced_at")]
    public DateTimeOffset? LastSyncedAt { get; set; }
}
