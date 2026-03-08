using System;
using System.Linq;
using System.Threading.Tasks;
using Hano.Core.Application.Contracts.Dashboard;
using Hano.Core.Application.Contracts.Dashboard.Dtos;
using Hano.Core.Domain.Orders;
using Hano.Core.Domain.Sessions;
using Hano.Core.Domain.Visits;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Domain.Repositories;

namespace Hano.Core.Application.Dashboard;

public class DashboardAppService : HanoCoreAppServiceBase, IDashboardAppService
{
    private readonly IRepository<WorkSession, Guid> _sessionRepo;
    private readonly IRepository<Visit, Guid> _visitRepo;
    private readonly IRepository<Order, Guid> _orderRepo;

    public DashboardAppService(IRepository<WorkSession, Guid> sessionRepo, IRepository<Visit, Guid> visitRepo, IRepository<Order, Guid> orderRepo)
    { _sessionRepo = sessionRepo; _visitRepo = visitRepo; _orderRepo = orderRepo; }

    public async Task<NvbhDashboardDto> GetNvbhAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var session = await _sessionRepo.FirstOrDefaultAsync(x => x.UserId == CurrentUserId && x.Date == today);
        var visits = session != null ? await _visitRepo.GetListAsync(x => x.SessionId == session.Id) : [];
        var orders = session != null ? await _orderRepo.GetListAsync(x => x.SessionId == session.Id) : [];
        return new NvbhDashboardDto
        {
            SessionStatus = session?.Status.ToString() ?? "NOT_STARTED",
            RouteProgress = new() { Planned = visits.Count(v => v.Status == VisitStatus.Planned), InProgress = visits.Count(v => v.Status == VisitStatus.InProgress), Completed = visits.Count(v => v.Status == VisitStatus.Completed), Skipped = visits.Count(v => v.Status == VisitStatus.Skipped) },
            TodayKpis = new() { OrderCount = orders.Count, Revenue = orders.Sum(o => o.TotalAmount) },
        };
    }

    public Task<GsbhDashboardDto> GetGsbhAsync() => Task.FromResult(new GsbhDashboardDto()); // TODO: filter by team
    public Task<AsmDashboardDto> GetAsmAsync() => Task.FromResult(new AsmDashboardDto()); // TODO: filter by region
}
