using System;
using System.ComponentModel.DataAnnotations;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Application.Dtos;
namespace Hano.Core.Application.Contracts.Dtos;

public class VisitDto : EntityDto<Guid>
{
    public Guid OutletId { get; set; }

    [MaxLength(10240)]
    public string OutletName { get; set; } = null!;
    public VisitStatus Status { get; set; }
    public DateTimeOffset? CheckinAt { get; set; }
    public DateTimeOffset? CheckoutAt { get; set; }
    public int? DurationMinutes { get; set; }
    public int ActivitiesCount { get; set; }
    public GpsFlag? GpsFlag { get; set; }
}
