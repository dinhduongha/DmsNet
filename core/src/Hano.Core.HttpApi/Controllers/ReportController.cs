using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hano.Core.HttpApi.Controllers;

/// <summary>
/// Daily reports (text + voice) by NVBH at EOD.
/// </summary>
[Route("api/v1/reports")]
[ApiController]
[Authorize]
public class DmsReportController : HanoCoreController
{
    // ── #50 Submit Text Report ──
    [HttpPost("text")]
    public async Task<IActionResult> SubmitText([FromBody] object input)
    {
        // TODO: Inject IReportAppService → save DailyReport (ReportType=TEXT)
        return Ok(new { report_id = Guid.NewGuid(), status = "OK" });
    }

    // ── #51 Submit Voice Report ──
    [HttpPost("voice")]
    public async Task<IActionResult> SubmitVoice([FromBody] object input)
    {
        // TODO: Inject IReportAppService → save DailyReport (ReportType=VOICE)
        // Upload audio → S3, optional STT transcription
        return Ok(new { report_id = Guid.NewGuid(), transcribed_text = "", status = "OK" });
    }
}
