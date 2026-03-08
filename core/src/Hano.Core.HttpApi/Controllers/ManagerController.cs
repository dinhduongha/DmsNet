using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hano.Core.HttpApi.Controllers;

[Route("api/v1/manager")]
[ApiController]
[Authorize(Roles = "Asm")]
public class DmsManagerController : HanoCoreController
{
    [HttpPost("aggregated")]
    public async Task<IActionResult> GetAggregated()
    {
        return Ok(new { data = System.Array.Empty<object>() });
    }

    [HttpPost("reports/search")]
    public async Task<IActionResult> SearchReports()
    {
        return Ok(new { total_count = 0, reports = System.Array.Empty<object>() });
    }

    [HttpPost("reports/export")]
    public async Task<IActionResult> ExportReport()
    {
        return Ok(new { export_id = System.Guid.NewGuid(), status = "PROCESSING" });
    }
}
