using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hano.Core.Application.Contracts.Orders;
using Hano.Core.Application.Contracts.Orders.Dtos;
using Volo.Abp.Application.Dtos;

namespace Hano.Core.HttpApi.Controllers;

[Route("api/v1/orders")]
[ApiController]
[Authorize]
public class DmsOrderController : HanoCoreController
{
    private readonly IOrderAppService _orderAppService;

    public DmsOrderController(IOrderAppService orderAppService)
    {
        _orderAppService = orderAppService;
    }

    // ── #28 Create Order (DSR/DSD) ──
    [HttpPost]
    public async Task<OrderDto> Create([FromBody] CreateOrderDto input)
        => await _orderAppService.CreateAsync(input);

    // ── #29 Save Draft ──
    [HttpPost("{id}/draft")]
    public async Task<OrderDto> SaveDraft(Guid id, [FromBody] CreateOrderDto input)
        => await _orderAppService.SaveDraftAsync(input);

    // ── #30 Update Draft ──
    [HttpPut("{id}/draft")]
    public async Task<OrderDto> UpdateDraft(Guid id, [FromBody] CreateOrderDto input)
    {
        // Update existing draft order
        return await _orderAppService.SaveDraftAsync(input);
    }

    // ── #31 List Orders ──
    [HttpGet]
    public async Task<PagedResultDto<OrderDto>> GetList([FromQuery] OrderFilterDto input)
        => await _orderAppService.GetListAsync(input);

    // ── #32 Get Order ──
    [HttpGet("{id}")]
    public async Task<OrderDto> Get(Guid id)
        => await _orderAppService.GetAsync(id);

    // ── #33 Reorder (suggest based on last order) ──
    [HttpGet("reorder/{outletId}")]
    public async Task<IActionResult> GetReorder(Guid outletId)
    {
        // TODO: Add GetReorderAsync to IOrderAppService
        // Fetch last order for outlet, return suggested items
        return Ok(new { outlet_id = outletId, suggested_items = Array.Empty<object>() });
    }

    // ── #36 POD (Proof of Delivery) ──
    [HttpPost("{id}/pod")]
    public async Task<IActionResult> SubmitPod(Guid id, [FromBody] object input)
    {
        // TODO: Add SubmitPodAsync to IOrderAppService
        // Update order with POD photo, payment, receiver
        return Ok(new { order_id = id, status = "DELIVERED" });
    }
}
