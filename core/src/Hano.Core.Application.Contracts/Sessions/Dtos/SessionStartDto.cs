using System;
using System.Collections.Generic;
namespace Hano.Core.Application.Contracts.Sessions.Dtos;

public class SessionStartDto
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public Guid? SelfiePhotoId { get; set; }
    public List<VehicleStockItemDto>? VehicleStockItems { get; set; }
}
public class VehicleStockItemDto
{
    public Guid SkuId { get; set; }
    public int Quantity { get; set; }
    public string Unit { get; set; } = null!;
}
