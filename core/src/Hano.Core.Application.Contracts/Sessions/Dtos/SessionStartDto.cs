using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Hano.Core.Application.Contracts.Dtos;

public class SessionStartDto
{
    [Range(typeof(decimal), "-10000", "10000")]
    public decimal Latitude { get; set; }

    [Range(typeof(decimal), "-10000", "10000")]
    public decimal Longitude { get; set; }

    public Guid? SelfiePhotoId { get; set; }
    public List<VehicleStockItemDto>? VehicleStockItems { get; set; }
}
public class VehicleStockItemDto
{
    public Guid SkuId { get; set; }


    [Range(typeof(decimal), "-10000", "10000")]
    public decimal Quantity { get; set; }

    public string Unit { get; set; } = null!;
}
