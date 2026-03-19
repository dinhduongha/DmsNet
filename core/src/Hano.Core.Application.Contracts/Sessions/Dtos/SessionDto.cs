using System;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Application.Dtos;
namespace Hano.Core.Application.Contracts.Dtos;

public class SessionDto : EntityDto<Guid>
{
    public SessionStatus Status { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public SessionSummaryDto? Summary { get; set; }
}
public class SessionSummaryDto
{
    public int TotalVisits { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal? TotalDistanceKm { get; set; }
    public int WorkDurationMinutes { get; set; }
}
public class BreadcrumbDto
{
    public decimal Lat { get; set; }
    public decimal Lng { get; set; }
    public float Accuracy { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}
