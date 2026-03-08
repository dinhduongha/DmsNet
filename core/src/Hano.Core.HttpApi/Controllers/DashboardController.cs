using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hano.Core.Application.Contracts.Dashboard;
using Hano.Core.Application.Contracts.Dashboard.Dtos;

namespace Hano.Core.HttpApi.Controllers;

[Route("api/v1/dashboard")]
[ApiController]
[Authorize]
public class DmsDashboardController : HanoCoreController
{
    private readonly IDashboardAppService _dashboardAppService;

    public DmsDashboardController(IDashboardAppService dashboardAppService)
    {
        _dashboardAppService = dashboardAppService;
    }

    // ── #52 NVBH Dashboard ──
    [HttpGet("nvbh")]
    public async Task<NvbhDashboardDto> GetNvbh()
        => await _dashboardAppService.GetNvbhAsync();

    // ── #66 GSBH Dashboard ──
    [HttpGet("gsbh")]
    [Authorize(Roles = "Gsbh,Asm")]
    public async Task<GsbhDashboardDto> GetGsbh()
        => await _dashboardAppService.GetGsbhAsync();

    // ── #67 GSBH Map View ──
    [HttpGet("gsbh/map")]
    [Authorize(Roles = "Gsbh,Asm")]
    public async Task<IActionResult> GetGsbhMap()
    {
        // TODO: Add GetGsbhMapAsync to IDashboardAppService
        // Return NVBH positions + visit statuses for map overlay
        return Ok(new { members = System.Array.Empty<object>() });
    }

    // ── #83 ASM Dashboard ──
    [HttpGet("asm")]
    [Authorize(Roles = "Asm")]
    public async Task<AsmDashboardDto> GetAsm()
        => await _dashboardAppService.GetAsmAsync();
}
