using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Hano.Core.Domain.Orders;

[Table("orders")]
public class Order : FullAuditedAggregateRoot<Guid>
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

    [Column("visit_id")]
    public Guid VisitId { get; set; }

    [Column("outlet_id")]
    public Guid OutletId { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("session_id")]
    public Guid SessionId { get; set; }

    [Column("order_code")]
    public string? OrderCode { get; set; }

    [Column("order_type")]
    public OrderType OrderType { get; set; }

    [Column("status")]
    public OrderStatus Status { get; set; } = OrderStatus.Draft;

    [Column("total_amount")]
    public decimal TotalAmount { get; set; }

    [Column("total_items")]
    public int TotalItems { get; set; }

    [Column("discount_amount")]
    public decimal DiscountAmount { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("payment_method")]
    public PaymentMethod? PaymentMethod { get; set; }

    [Column("pod_photo_id")]
    public Guid? PodPhotoId { get; set; }

    [Column("amount_collected")]
    public decimal? AmountCollected { get; set; }

    [Column("receiver_name")]
    public string? ReceiverName { get; set; }

    [Column("promotion_id")]
    public Guid? PromotionId { get; set; }

    [Column("ods_order_id")]
    public string? OdsOrderId { get; set; }

    [Column("sync_status")]
    public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;

    [Column("client_created_at")]
    public DateTime ClientCreatedAt { get; set; }

    public ICollection<OrderLine> Lines { get; set; } = new List<OrderLine>();
}
