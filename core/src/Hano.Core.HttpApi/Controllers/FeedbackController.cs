using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hano.Core.Application.Contracts.Feedback;
using Hano.Core.Application.Contracts.Feedback.Dtos;
using Volo.Abp.Application.Dtos;

namespace Hano.Core.HttpApi.Controllers;

[Route("api/v1/feedbacks")]
[ApiController]
[Authorize]
public class DmsFeedbackController : HanoCoreController
{
    private readonly IFeedbackAppService _feedbackAppService;

    public DmsFeedbackController(IFeedbackAppService feedbackAppService)
    {
        _feedbackAppService = feedbackAppService;
    }

    // ── #43 Create Feedback ──
    [HttpPost("")]
    public async Task<FeedbackDto> Create([FromBody] CreateFeedbackDto input)
        => await _feedbackAppService.CreateAsync(input);

    // ── #44 List Feedbacks ──
    [HttpGet("")]
    public async Task<PagedResultDto<FeedbackDto>> GetList([FromQuery] FeedbackFilterDto input)
        => await _feedbackAppService.GetListAsync(input);

    // ── #45 Get Feedback ──
    [HttpGet("{id}")]
    public async Task<FeedbackDto> Get(Guid id)
        => await _feedbackAppService.GetAsync(id);
}
