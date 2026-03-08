using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hano.Core.Application.Contracts.Sessions.Dtos;
using Volo.Abp.Application.Services;
namespace Hano.Core.Application.Contracts.Sessions;

public interface ISessionAppService : IApplicationService
{
    Task<SessionDto> StartAsync(SessionStartDto input);
    Task<SessionDto> EndAsync(Guid sessionId, decimal latitude, decimal longitude);
    Task<SessionDto?> GetCurrentAsync();
    Task SendBreadcrumbsAsync(Guid sessionId, List<BreadcrumbDto> points);
    Task<SessionSummaryDto> GetSummaryAsync(Guid sessionId);
}
