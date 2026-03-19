using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Application.Dtos;

namespace Hano.Core.Application.Contracts.Dtos;

public class CreateOutletDto
{
    public Guid Id { get; set; }

    [MaxLength(1024)]
    public string Name { get; set; } = null!;

    [MaxLength(1024)]
    public string Address { get; set; } = null!;

    [MaxLength(1024)]
    public string? Phone { get; set; }
    public OutletType OutletType { get; set; }

    [Range(typeof(decimal), "-10000", "10000")]
    public decimal Latitude { get; set; }

    [Range(typeof(decimal), "-10000", "10000")]
    public decimal Longitude { get; set; }
    public List<Guid> PhotoIds { get; set; } = new();
}

public class UpdateOutletDto
{
    [MaxLength(1024)]
    public string Name { get; set; } = null!;

    [MaxLength(1024)]
    public string Address { get; set; } = null!;

    [MaxLength(1024)]
    public string? Phone { get; set; }
    public OutletType OutletType { get; set; }

    [Range(typeof(decimal), "-10000", "10000")]
    public decimal Latitude { get; set; }

    [Range(typeof(decimal), "-10000", "10000")]
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
    [MaxLength(1024)]
    public string Name { get; set; } = null!;

    [MaxLength(1024)]
    public string Address { get; set; } = null!;

    public OutletType OutletType { get; set; }

    [Range(typeof(decimal), "-10000", "10000")]
    public decimal Latitude { get; set; }

    [Range(typeof(decimal), "-10000", "10000")]
    public decimal Longitude { get; set; }
    public OutletStatus Status { get; set; }
    public Channel? Channel { get; set; }
    public OutletSize? Size { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class OutletFilterDto : PagedAndSortedResultRequestDto
{
    public Guid? RouteId { get; set; }
    public OutletStatus? Status { get; set; }
    public Channel? Channel { get; set; }
    public string? Search { get; set; }
}
