using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Hano.Core.Application.Contracts.Dtos;

public class NvbhDashboardDto
{
    [MaxLength(1024)]
    public string SessionStatus { get; set; } = null!;
    public RouteProgressDto RouteProgress { get; set; } = new();
    public TodayKpiDto TodayKpis { get; set; } = new();
    public int PendingSyncCount { get; set; }
    public List<AlertDto> Alerts { get; set; } = new();
}
public class RouteProgressDto
{
    public int Planned { get; set; }
    public int Completed { get; set; }
    public int Skipped { get; set; }
    public int InProgress { get; set; }
}
public class TodayKpiDto
{
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
    public int PhotoCount { get; set; }
}
public class AlertDto
{
    public string Type { get; set; } = null!;
    public string Message { get; set; } = null!;
}
public class GsbhDashboardDto
{
    public List<TeamMemberDto> TeamMembers { get; set; } = new();
    public TodayKpiDto TeamKpis { get; set; } = new();
    public int PendingApprovals { get; set; }
}
public class TeamMemberDto
{
    public Guid NvbhId { get; set; }
    public string Name { get; set; } = null!;
    public string SessionStatus { get; set; } = null!;
    public decimal? Lat { get; set; }
    public decimal? Lng { get; set; }
}

public class AsmDashboardDto
{
    public int TotalNvbh { get; set; }
    public int ActiveNvbh { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal VisitCompletionRate { get; set; }
}
