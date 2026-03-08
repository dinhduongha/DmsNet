using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hano.Core.Application.Contracts.Sessions;
using Hano.Core.Application.Contracts.Sessions.Dtos;

namespace Hano.Core.HttpApi.Controllers;

[Route("api/v1/sessions")]
[ApiController]
[Authorize]
public class DmsSessionController : HanoCoreController
{
    private readonly ISessionAppService _sessionAppService;

    public DmsSessionController(ISessionAppService sessionAppService)
    {
        _sessionAppService = sessionAppService;
    }

    // ── #10 SOD (Start of Day) ──
    [HttpPost("start")]
    public async Task<SessionDto> Start([FromBody] SessionStartDto input)
        => await _sessionAppService.StartAsync(input);

    // ── #11 EOD (End of Day) ──
    [HttpPost("end")]
    public async Task<SessionDto> End([FromBody] EndSessionDto input)
        => await _sessionAppService.EndAsync(input.SessionId, input.Latitude, input.Longitude);

    // ── #12 Current Session ──
    [HttpGet("current")]
    public async Task<SessionDto?> GetCurrent()
        => await _sessionAppService.GetCurrentAsync();

    // ── #13 Breadcrumbs ──
    [HttpPost("{id}/breadcrumbs")]
    public async Task<IActionResult> SendBreadcrumbs(Guid id, [FromBody] List<BreadcrumbDto> points)
    {
        await _sessionAppService.SendBreadcrumbsAsync(id, points);
        return Ok(new { received_count = points.Count });
    }

    // ── #14 Summary ──
    [HttpGet("{id}/summary")]
    public async Task<SessionSummaryDto> GetSummary(Guid id)
        => await _sessionAppService.GetSummaryAsync(id);
}

public class EndSessionDto
{
    public Guid SessionId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}
