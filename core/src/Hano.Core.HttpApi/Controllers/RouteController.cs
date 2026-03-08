using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hano.Core.Application.Contracts.Routes;
using Hano.Core.Application.Contracts.Routes.Dtos;
using Volo.Abp.Application.Dtos;

namespace Hano.Core.HttpApi.Controllers;

[Route("api/v1/routes")]
[ApiController]
[Authorize]
public class DmsRouteController : HanoCoreController
{
    private readonly IRouteAppService _routeAppService;

    public DmsRouteController(IRouteAppService routeAppService)
    {
        _routeAppService = routeAppService;
    }

    // ── #56 Get Today Route ──
    [HttpGet("today")]
    public async Task<TodayRouteDto?> GetToday()
        => await _routeAppService.GetTodayAsync();

    // ── #57 List Routes ──
    [HttpGet("")]
    public async Task<PagedResultDto<RouteDto>> GetList([FromQuery] PagedAndSortedResultRequestDto input)
        => await _routeAppService.GetListAsync(input);

    // ── #76 Create Route (GSBH) ──
    [HttpPost("")]
    [Authorize(Roles = "Gsbh")]
    public async Task<RouteDto> Create([FromBody] CreateRouteDto input)
        => await _routeAppService.CreateAsync(input);

    // ── #77 Update Route (GSBH) ──
    [HttpPut("{id}")]
    [Authorize(Roles = "Gsbh")]
    public async Task<RouteDto> Update(Guid id, [FromBody] UpdateRouteDto input)
        => await _routeAppService.UpdateAsync(id, input);

    // ── #78 Get Route ──
    [HttpGet("{id}")]
    public async Task<RouteDto> Get(Guid id)
        => await _routeAppService.GetAsync(id);

    // ── #85 Approve Route (ASM) ──
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Asm")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveRejectDto input)
    { await _routeAppService.ApproveAsync(id, input.Notes); return Ok(); }

    // ── #86 Reject Route (ASM) ──
    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Asm")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] ApproveRejectDto input)
    { await _routeAppService.RejectAsync(id, input.Reason!); return Ok(); }
}
