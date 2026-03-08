using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hano.Core.Application.Contracts.Audit;
using Hano.Core.Application.Contracts.Audit.Dtos;

namespace Hano.Core.HttpApi.Controllers;

[Route("api/v1/audit")]
[ApiController]
[Authorize]
public class DmsAuditController : HanoCoreController
{
    private readonly IAuditAppService _auditAppService;

    public DmsAuditController(IAuditAppService auditAppService)
    {
        _auditAppService = auditAppService;
    }

    // ── #37 Create OSA ──
    [HttpPost("osa")]
    public async Task<IActionResult> CreateOsa([FromBody] OsaReportInputDto input)
        => Ok(new { report_id = await _auditAppService.CreateOsaAsync(input), status = "OK" });

    // ── #38 Get OSA ──
    [HttpGet("osa/{id}")]
    public async Task<IActionResult> GetOsa(Guid id)
    {
        // TODO: Add GetOsaAsync to IAuditAppService
        return Ok(new { id });
    }

    // ── #39 Create OOS ──
    [HttpPost("oos")]
    public async Task<IActionResult> CreateOos([FromBody] OosReportInputDto input)
        => Ok(new { report_id = await _auditAppService.CreateOosAsync(input), status = "OK" });

    // ── #40 Get OOS ──
    [HttpGet("oos/{id}")]
    public async Task<IActionResult> GetOos(Guid id)
    {
        // TODO: Add GetOosAsync to IAuditAppService
        return Ok(new { id });
    }

    // ── #41 Create POSM ──
    [HttpPost("posm")]
    public async Task<IActionResult> CreatePosm([FromBody] PosmReportInputDto input)
        => Ok(new { report_id = await _auditAppService.CreatePosmAsync(input), status = "OK" });

    // ── #42 Get POSM ──
    [HttpGet("posm/{id}")]
    public async Task<IActionResult> GetPosm(Guid id)
    {
        // TODO: Add GetPosmAsync to IAuditAppService
        return Ok(new { id });
    }
}
