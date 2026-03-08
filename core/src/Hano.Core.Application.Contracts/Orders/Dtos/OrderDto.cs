using System;
using System.Collections.Generic;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Application.Dtos;
namespace Hano.Core.Application.Contracts.Orders.Dtos;

public class OrderDto : EntityDto<Guid>
{
    public string? OrderCode { get; set; }
    public string OutletName { get; set; } = null!;
    public OrderType OrderType { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalItems { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public List<OrderLineDto> Lines { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
public class OrderLineDto
{
    public Guid SkuId { get; set; }
    public string SkuCode { get; set; } = null!;
    public string SkuName { get; set; } = null!;
    public int Quantity { get; set; }
    public string Unit { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
public class OrderFilterDto : PagedAndSortedResultRequestDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public Guid? OutletId { get; set; }
    public OrderStatus? Status { get; set; }
    public OrderType? OrderType { get; set; }
}
public class ReconcileDto
{
    public Guid Id { get; set; }
    public List<ReconcileItemDto> Items { get; set; } = new();
}
public class ReconcileItemDto
{
    public Guid SkuId { get; set; }
    public int PhysicalCount { get; set; }
    public int ReturnQty { get; set; }
    public int DamagedQty { get; set; }
    public string? Reason { get; set; }
}
