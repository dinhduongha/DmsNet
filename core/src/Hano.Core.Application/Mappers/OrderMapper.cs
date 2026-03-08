using Riok.Mapperly.Abstractions;
using Hano.Core.Application.Contracts.Orders.Dtos;
using Hano.Core.Domain.Orders;
using System.Linq;

namespace Hano.Core.Application.Mappers;

[Mapper]
public static partial class OrderMapper
{
    public static partial OrderLineDto ToDto(this OrderLine source);

    public static OrderDto ToDto(this Order source, string outletName)
    {
        return new OrderDto
        {
            Id = source.Id,
            OrderCode = source.OrderCode,
            OutletName = outletName,
            OrderType = source.OrderType,
            Status = source.Status,
            TotalAmount = source.TotalAmount,
            TotalItems = source.TotalItems,
            PaymentMethod = source.PaymentMethod,
            Lines = source.Lines.Select(l => l.ToDto()).ToList(),
            CreatedAt = source.CreationTime,
        };
    }
}
