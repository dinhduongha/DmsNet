using System;
using System.Threading.Tasks;
using Hano.Core.Application.Contracts.Photos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hano.Core.HttpApi.Controllers;

[Route("api/v1/photos")]
[ApiController]
[Authorize]
public class DmsPhotoController : HanoCoreController
{
    private readonly IPhotoAppService _photoAppService;

    public DmsPhotoController(IPhotoAppService photoAppService)
    {
        _photoAppService = photoAppService;
    }

    [HttpPost("presigned-url")]
    public async Task<PresignedUrlResponseDto> GetPresignedUrl([FromBody] PresignedUrlRequestDto input)
        => await _photoAppService.GetPresignedUrlAsync(input);

    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmUpload([FromBody] PhotoConfirmDto input)
    { await _photoAppService.ConfirmUploadAsync(input); return Ok(); }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDownloadUrl(Guid id)
        => Ok(new { url = await _photoAppService.GetDownloadUrlAsync(id) });
}
