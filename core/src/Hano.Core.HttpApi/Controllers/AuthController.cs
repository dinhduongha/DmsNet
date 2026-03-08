using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hano.Core.HttpApi.Controllers;

/// <summary>
/// Authentication & Device Management.
/// #1-#4 proxy to AuthServer (OpenIddict).
/// #5-#9 device management.
/// </summary>
[Route("api/v1/auth")]
[ApiController]
public class AuthController : HanoCoreController
{
    // ── #1 Login (proxy to AuthServer) ──
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] object input)
    {
        // Proxy to AuthServer /connect/token (OpenIddict)
        // In production: use IHttpClientFactory("AuthServer") to forward
        return Ok(new { access_token = "jwt_placeholder", refresh_token = "rt_placeholder" });
    }

    // ── #2 Refresh Token ──
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] object input)
    {
        // Proxy to AuthServer /connect/token with grant_type=refresh_token
        return Ok(new { access_token = "jwt_new", refresh_token = "rt_new" });
    }

    // ── #3 Logout ──
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] object input)
    {
        // Revoke tokens + unbind device
        return Ok(new { success = true });
    }

    // ── #4 Verify Token ──
    [HttpPost("verify-token")]
    public async Task<IActionResult> VerifyToken([FromBody] object input)
    {
        // Validate JWT, check account status
        return Ok(new { is_valid = true, account_status = "active" });
    }

    // ── #5 Device Check ──
    [HttpPost("device/check")]
    public async Task<IActionResult> DeviceCheck([FromBody] object input)
    {
        // Check rooted/jailbroken, device whitelist
        return Ok(new { is_allowed = true });
    }

    // ── #6 Bind Device ──
    [HttpPost("devices/bind")]
    [Authorize]
    public async Task<IActionResult> BindDevice([FromBody] object input)
    {
        return Ok(new { bound = true });
    }

    // ── #7 Unbind Device ──
    [HttpDelete("devices/{id}")]
    [Authorize]
    public async Task<IActionResult> UnbindDevice(Guid id)
    {
        return Ok(new { success = true });
    }

    // ── #8 Update FCM Token ──
    [HttpPost("devices/fcm-token")]
    [Authorize]
    public async Task<IActionResult> UpdateFcmToken([FromBody] object input)
    {
        return Ok(new { success = true });
    }

    // ── #9 Get Profile ──
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        return Ok();
    }
}
