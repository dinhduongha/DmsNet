using System;
using System.Collections.Generic;
using Hano.Core.Domain.Shared.Enums;
namespace Hano.Core.Application.Contracts.Orders.Dtos;

public class CreateOrderDto
{
    public Guid Id { get; set; }
    public Guid VisitId { get; set; }
    public Guid OutletId { get; set; }
    public OrderType OrderType { get; set; }
    public List<OrderLineInputDto> Items { get; set; } = new();
    public string? Notes { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public Guid? PodPhotoId { get; set; }
    public DateTime Timestamp { get; set; }
}
public class OrderLineInputDto
{
    public Guid SkuId { get; set; }
    public int Quantity { get; set; }
    public string Unit { get; set; } = null!;
}
