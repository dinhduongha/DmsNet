using System;
using System.Threading.Tasks;
using Hano.Core.Application.Contracts.Sync;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hano.Core.HttpApi.Controllers;

[Route("api/v1/sync")]
[ApiController]
[Authorize]
public class DmsSyncController : HanoCoreController
{
    private readonly ISyncAppService _syncAppService;

    public DmsSyncController(ISyncAppService syncAppService)
    {
        _syncAppService = syncAppService;
    }

    // ── #20 Upload ──
    [HttpPost("upload")]
    public async Task<SyncUploadResultDto> Upload([FromBody] SyncUploadDto input)
        => await _syncAppService.UploadAsync(input);

    // ── #21 Status ──
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus([FromQuery] Guid[] uuids)
    {
        // TODO: Add GetStatusAsync to ISyncAppService
        // Query SyncQueue by entity IDs, return status per UUID
        return Ok(new { items = Array.Empty<object>() });
    }

    // ── #22 Resolve Conflict ──
    [HttpPost("resolve-conflict")]
    public async Task<IActionResult> ResolveConflict([FromBody] ResolveConflictDto input)
    { await _syncAppService.ResolveConflictAsync(input); return Ok(); }
}
