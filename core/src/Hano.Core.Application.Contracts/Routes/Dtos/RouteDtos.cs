using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Application.Dtos;

namespace Hano.Core.Application.Contracts.Dtos;

public class CreateRouteDto
{
    [MaxLength(1024)]
    public string Name { get; set; } = null!;
    public int DayOfWeek { get; set; }
    public Guid NvbhId { get; set; }
    public List<Guid> OutletIds { get; set; } = new();
}

public class UpdateRouteDto
{
    [MaxLength(1024)]
    public string Name { get; set; } = null!;
    public int DayOfWeek { get; set; }
    public Guid NvbhId { get; set; }
    public List<Guid> OutletIds { get; set; } = new();
}

public class RouteDto : EntityDto<Guid>
{
    [MaxLength(1024)]
    public string Name { get; set; } = null!;
    public int DayOfWeek { get; set; }
    public RouteStatus Status { get; set; }
    public int TotalOutlets { get; set; }
    public List<RouteOutletDto> Outlets { get; set; } = new();
}

public class RouteOutletDto
{
    public Guid OutletId { get; set; }

    [MaxLength(1024)]
    public string OutletName { get; set; } = null!;
    public int SequenceOrder { get; set; }
    public string? VisitStatus { get; set; }
}

public class TodayRouteDto
{
    public Guid RouteId { get; set; }
    public string RouteName { get; set; } = null!;
    public List<RouteOutletDto> Outlets { get; set; } = new();
}

public class ApproveRejectDto
{
    public string? Notes { get; set; }
    public string? Reason { get; set; }
}
