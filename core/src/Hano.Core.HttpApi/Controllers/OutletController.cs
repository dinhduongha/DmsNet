using System;
using System.Threading.Tasks;
using Hano.Core.Application.Contracts.Outlets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;

namespace Hano.Core.HttpApi.Controllers;

[Route("api/v1/outlets")]
[ApiController]
[Authorize]
public class DmsOutletController : HanoCoreController
{
    private readonly IOutletAppService _outletAppService;

    public DmsOutletController(IOutletAppService outletAppService)
    {
        _outletAppService = outletAppService;
    }

    // ── #58 Get Outlet ──
    [HttpGet("{id}")]
    public async Task<OutletDto> Get(Guid id)
        => await _outletAppService.GetAsync(id);

    // ── #59 List Outlets ──
    [HttpGet("")]
    public async Task<PagedResultDto<OutletDto>> GetList([FromQuery] OutletFilterDto input)
        => await _outletAppService.GetListAsync(input);

    // ── #60 Create Outlet ──
    [HttpPost("")]
    public async Task<OutletDto> Create([FromBody] CreateOutletDto input)
        => await _outletAppService.CreateAsync(input);

    // ── #61 Update Outlet ──
    [HttpPut("{id}")]
    public async Task<OutletDto> Update(Guid id, [FromBody] UpdateOutletDto input)
        => await _outletAppService.UpdateAsync(id, input);

    // ── #79 Get Pending ──
    [HttpGet("pending")]
    [Authorize(Roles = "Gsbh")]
    public async Task<PagedResultDto<OutletDto>> GetPending([FromQuery] PagedAndSortedResultRequestDto input)
        => await _outletAppService.GetPendingAsync(input);

    // ── #80 Approve ──
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Gsbh")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveOutletDto input)
    { await _outletAppService.ApproveAsync(id, input); return Ok(); }

    // ── #81 Reject ──
    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Gsbh")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectReasonDto input)
    { await _outletAppService.RejectAsync(id, input.Reason); return Ok(); }

    // ── #82 Deactivate ──
    [HttpPost("{id}/deactivate")]
    [Authorize(Roles = "Gsbh")]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] RejectReasonDto input)
    { await _outletAppService.DeactivateAsync(id, input.Reason); return Ok(); }
}

public class RejectReasonDto { public string Reason { get; set; } = null!; }
