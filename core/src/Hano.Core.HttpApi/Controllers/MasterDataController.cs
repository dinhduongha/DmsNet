using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hano.Core.Application.Contracts.MasterData;
using Hano.Core.Application.Contracts.MasterData.Dtos;

namespace Hano.Core.HttpApi.Controllers;

[Route("api/v1/master-data")]
[ApiController]
[Authorize]
public class DmsMasterDataController : HanoCoreController
{
    private readonly IMasterDataAppService _masterDataAppService;

    public DmsMasterDataController(IMasterDataAppService masterDataAppService)
    {
        _masterDataAppService = masterDataAppService;
    }

    // ── #15 Full Sync ──
    [HttpGet("sync")]
    public async Task<MasterDataSyncResponseDto> Sync([FromQuery] DateTime? lastSyncTimestamp)
        => await _masterDataAppService.SyncAsync(lastSyncTimestamp);

    // ── #16 SKUs ──
    [HttpGet("skus")]
    public async Task<IActionResult> GetSkus([FromQuery] DateTime? since)
    {
        var data = await _masterDataAppService.SyncAsync(since);
        return Ok(new { items = data.Skus, timestamp = data.NewSyncTimestamp });
    }

    // ── #17 Prices ──
    [HttpGet("prices")]
    public async Task<IActionResult> GetPrices([FromQuery] DateTime? since)
    {
        var data = await _masterDataAppService.SyncAsync(since);
        return Ok(new { items = data.Prices, timestamp = data.NewSyncTimestamp });
    }

    // ── #18 Promotions ──
    [HttpGet("promotions")]
    public async Task<IActionResult> GetPromotions([FromQuery] DateTime? since)
    {
        // TODO: Add promotions to MasterDataSyncResponseDto
        return Ok(new { items = Array.Empty<object>() });
    }

    // ── #19 Distributors ──
    [HttpGet("distributors")]
    public async Task<IActionResult> GetDistributors([FromQuery] DateTime? since)
    {
        // TODO: Add distributors to MasterDataSyncResponseDto
        return Ok(new { items = Array.Empty<object>() });
    }
}
