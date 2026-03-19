using System;
using System.Linq;
using System.Threading.Tasks;
using Hano.Core.Application.Contracts.MasterData;
using Hano.Core.Application.Mappers;
using Volo.Abp.Domain.Repositories;

namespace Hano.Core.Application.MasterData;

public class MasterDataAppService : HanoCoreAppServiceBase, IMasterDataAppService
{
    private readonly IRepository<Sku, Guid> _skuRepo;
    private readonly IRepository<PriceList, Guid> _priceRepo;

    public MasterDataAppService(
        IRepository<Sku, Guid> skuRepo,
        IRepository<PriceList, Guid> priceRepo)
    {
        _skuRepo = skuRepo;
        _priceRepo = priceRepo;
    }

    public async Task<MasterDataSyncResponseDto> SyncAsync(DateTime? lastSyncTimestamp)
    {
        var now = DateTime.UtcNow;

        // Get SKUs changed since lastSyncTimestamp (or all if first sync)
        var skuQuery = await _skuRepo.GetQueryableAsync();
        if (lastSyncTimestamp.HasValue)
            skuQuery = skuQuery.Where(x => x.LastSyncedAt >= lastSyncTimestamp.Value
                || x.LastModificationTime >= lastSyncTimestamp.Value);

        var skus = skuQuery
            .Where(x => x.IsActive)
            .ToList();

        // Get active prices
        var today = DateOnly.FromDateTime(now);
        var priceQuery = await _priceRepo.GetQueryableAsync();
        var prices = priceQuery
            .Where(x => x.EffectiveFrom <= today
                && (x.EffectiveTo == null || x.EffectiveTo >= today))
            .ToList();

        // If incremental sync, filter prices by lastSyncTimestamp
        if (lastSyncTimestamp.HasValue)
            prices = prices.Where(x => x.LastSyncedAt >= lastSyncTimestamp.Value).ToList();

        return new MasterDataSyncResponseDto
        {
            Skus = skus.Select(s => s.ToSyncDto()).ToList(),
            Prices = prices.Select(p => new PriceSyncDto
            {
                SkuId = p.SkuId,
                UnitPrice = p.UnitPrice,
                PromoPrice = p.PromoPrice,
            }).ToList(),
            NewSyncTimestamp = now,
        };
    }
}
