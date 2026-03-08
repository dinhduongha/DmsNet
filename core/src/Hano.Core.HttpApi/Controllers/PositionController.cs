using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hano.Core.HttpApi.Controllers;

[Route("api/v1/position")]
[ApiController]
[Authorize]
public class DmsPositionController : HanoCoreController
{
    [HttpPost("current")]
    public async Task<IActionResult> ReportPosition([FromBody] object input)
    {
        // TODO: Save to GpsBreadcrumb or separate position table
        return Ok(new { received = true });
    }
}
