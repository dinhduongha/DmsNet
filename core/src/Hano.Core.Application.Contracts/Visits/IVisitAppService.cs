using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
namespace Hano.Core.Application.Contracts.Visits;

public interface IVisitAppService : IApplicationService
{
    Task<CheckinResultDto> CheckinAsync(CheckinDto input);
    Task<VisitDto> CheckoutAsync(Guid visitId, CheckoutDto input);
    Task<VisitDto> SkipAsync(Guid visitId, SkipVisitDto input);
    Task<List<VisitDto>> GetTodayAsync();
    Task<VisitDto> GetAsync(Guid visitId);
}
