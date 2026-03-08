using System;
using System.Collections.Generic;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Application.Dtos;

namespace Hano.Core.Application.Contracts.Outlets.Dtos;

public class CreateOutletDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? Phone { get; set; }
    public OutletType OutletType { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public List<Guid> PhotoIds { get; set; } = new();
}

public class UpdateOutletDto
{
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? Phone { get; set; }
    public OutletType OutletType { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}

public class ApproveOutletDto
{
    public Channel Channel { get; set; }
    public OutletSize Size { get; set; }
    public VisitFrequency VisitFrequency { get; set; }
    public Guid? RouteId { get; set; }
}

public class OutletDto : EntityDto<Guid>
{
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public OutletType OutletType { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public OutletStatus Status { get; set; }
    public Channel? Channel { get; set; }
    public OutletSize? Size { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OutletFilterDto : PagedAndSortedResultRequestDto
{
    public Guid? RouteId { get; set; }
    public OutletStatus? Status { get; set; }
    public Channel? Channel { get; set; }
    public string? Search { get; set; }
}
