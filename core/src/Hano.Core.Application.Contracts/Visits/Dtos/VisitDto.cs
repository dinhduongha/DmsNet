using System;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Application.Dtos;
namespace Hano.Core.Application.Contracts.Visits.Dtos;

public class VisitDto : EntityDto<Guid>
{
    public Guid OutletId { get; set; }
    public string OutletName { get; set; } = null!;
    public VisitStatus Status { get; set; }
    public DateTime? CheckinAt { get; set; }
    public DateTime? CheckoutAt { get; set; }
    public int? DurationMinutes { get; set; }
    public int ActivitiesCount { get; set; }
    public GpsFlag? GpsFlag { get; set; }
}
