using System;
using System.Threading.Tasks;
using Hano.Core.Application.Contracts.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;

namespace Hano.Core.HttpApi.Controllers;

[Route("api/v1/notifications")]
[ApiController]
[Authorize]
public class DmsNotificationController : HanoCoreController
{
    private readonly INotificationAppService _notifAppService;

    public DmsNotificationController(INotificationAppService notifAppService)
    {
        _notifAppService = notifAppService;
    }

    // ── #46 List Notifications ──
    [HttpGet("")]
    public async Task<PagedResultDto<NotificationDto>> GetList([FromQuery] NotifFilterDto input)
        => await _notifAppService.GetListAsync(input);

    // ── #47 Mark Read ──
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    { await _notifAppService.MarkAsReadAsync(id); return Ok(); }

    // ── #48 Mark All Read ──
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    { await _notifAppService.MarkAllAsReadAsync(); return Ok(); }

    // ── #49 Unread Count ──
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
        => Ok(new { count = await _notifAppService.GetUnreadCountAsync() });

    // ── #74 Send (GSBH/ASM) ──
    [HttpPost("send")]
    [Authorize(Roles = "Gsbh,Asm")]
    public async Task<IActionResult> Send([FromBody] SendNotifDto input)
    { await _notifAppService.SendAsync(input); return Ok(); }
}
