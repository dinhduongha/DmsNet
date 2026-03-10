using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hano.Core.HttpApi.Controllers;

[Route("api/v1/app-version")]
[ApiController]
[AllowAnonymous]
public class DmsAppVersionController : HanoCoreController
{
    [HttpGet("check")]
    public async Task<IActionResult> Check([FromQuery] string currentVersion, [FromQuery] string platform)
    {
        // TODO: Inject IAppVersionAppService
        return Ok(new { update_required = false, update_type = "SOFT", latest_version = "1.0.0" });
    }
}
