using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hano.Core.Application.Contracts.Visits;
using Hano.Core.Application.Contracts.Visits.Dtos;

namespace Hano.Core.HttpApi.Controllers;

[Route("api/v1/visits")]
[ApiController]
[Authorize]
public class DmsOutletVisitController : HanoCoreController
{
    private readonly IVisitAppService _visitAppService;

    public DmsOutletVisitController(IVisitAppService visitAppService)
    {
        _visitAppService = visitAppService;
    }

    [HttpPost("checkin")]
    public async Task<CheckinResultDto> Checkin([FromBody] CheckinDto input)
        => await _visitAppService.CheckinAsync(input);

    [HttpPost("{id}/checkout")]
    public async Task<VisitDto> Checkout(Guid id, [FromBody] CheckoutDto input)
        => await _visitAppService.CheckoutAsync(id, input);

    [HttpPost("{id}/skip")]
    public async Task<VisitDto> Skip(Guid id, [FromBody] SkipVisitDto input)
        => await _visitAppService.SkipAsync(id, input);

    [HttpGet("today")]
    public async Task<List<VisitDto>> GetToday()
        => await _visitAppService.GetTodayAsync();

    [HttpGet("{id}")]
    public async Task<VisitDto> Get(Guid id)
        => await _visitAppService.GetAsync(id);
}
