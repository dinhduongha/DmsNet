using System;
using System.Collections.Generic;
namespace Hano.Core.Application.Contracts.MasterData.Dtos;

public class MasterDataSyncResponseDto
{
    public List<SkuSyncDto> Skus { get; set; } = new();
    public List<PriceSyncDto> Prices { get; set; } = new();
    public DateTime NewSyncTimestamp { get; set; }
}
public class SkuSyncDto { public Guid Id { get; set; } public string Code { get; set; } = null!; public string Name { get; set; } = null!; public string Category { get; set; } = null!; public string Unit { get; set; } = null!; public string? ImageUrl { get; set; } public bool IsActive { get; set; } }
public class PriceSyncDto { public Guid SkuId { get; set; } public decimal UnitPrice { get; set; } public decimal? PromoPrice { get; set; } }
