using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Hano.Core.BackgroundJobs.Args;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Hano.Core.BackgroundJobs;

/// <summary>
/// Poll ODS mỗi 4h để cập nhật master data (SKU, giá, KM, POSM, NPP).
/// Chạy bằng ABP Background Worker hoặc recurring job.
/// </summary>
public class PollMasterDataJob : AsyncBackgroundJob<PollMasterDataJobArgs>, ITransientDependency
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IRepository<Sku, Guid> _skuRepo;

    public PollMasterDataJob(
        IHttpClientFactory httpClientFactory,
        IRepository<Sku, Guid> skuRepo)
    {
        _httpClientFactory = httpClientFactory;
        _skuRepo = skuRepo;
    }

    public override async Task ExecuteAsync(PollMasterDataJobArgs args)
    {
        Logger.LogInformation("PollMasterData: since {Since}", args.LastPollTimestamp);

        try
        {
            var client = _httpClientFactory.CreateClient("ODS");
            var since = args.LastPollTimestamp?.ToString("o") ?? "";
            var response = await client.GetAsync($"/api/master-data/delta?since={since}");
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadFromJsonAsync<JsonElement>();

            // TODO: Parse and upsert SKUs, Prices, Promotions, PosmItems, Distributors
            // Example for SKUs:
            // foreach (var sku in data.GetProperty("skus").EnumerateArray())
            // {
            //     var existing = await _skuRepo.FirstOrDefaultAsync(x => x.OdsSkuId == sku.GetProperty("id").GetString());
            //     if (existing != null) { /* update */ } else { /* insert */ }
            // }

            Logger.LogInformation("PollMasterData completed");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "PollMasterData failed");
        }
    }
}
