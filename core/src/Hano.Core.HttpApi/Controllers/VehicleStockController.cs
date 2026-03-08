using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hano.Core.HttpApi.Controllers;

/// <summary>
/// Vehicle stock (tồn xe) management.
/// </summary>
[Route("api/v1/vehicle-stock")]
[ApiController]
[Authorize]
public class DmsVehicleStockController : HanoCoreController
{
    // ── #34 Current Stock ──
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent([FromQuery] DateOnly? date)
    {
        // TODO: Inject IVehicleStockAppService
        // Return opening, sold, return, damaged, closing per SKU
        return Ok(new { date = date ?? DateOnly.FromDateTime(DateTime.UtcNow), items = Array.Empty<object>() });
    }

    // ── #35 Reconcile (đối soát cuối ngày) ──
    [HttpPost("reconcile")]
    public async Task<IActionResult> Reconcile([FromBody] object input)
    {
        // TODO: Inject IVehicleStockAppService
        // Compare system stock vs physical stock, flag discrepancy
        return Ok(new { reconciliation_id = Guid.NewGuid(), has_discrepancy = false });
    }
}
