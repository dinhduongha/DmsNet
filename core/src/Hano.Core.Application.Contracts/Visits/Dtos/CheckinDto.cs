using System;
using System.ComponentModel.DataAnnotations;
namespace Hano.Core.Application.Contracts.Visits.Dtos;

public class CheckinDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid OutletId { get; set; }

    [Range(typeof(decimal), "-10000", "10000")]
    public decimal Latitude { get; set; }

    [Range(typeof(decimal), "-10000", "10000")]
    public decimal Longitude { get; set; }
    public float GpsAccuracy { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public Guid? PhotoCheckinId { get; set; }
}
public class CheckinResultDto
{
    public Guid VisitId { get; set; }

    [Range(typeof(decimal), "-10000", "10000")]
    public decimal GpsDistanceM { get; set; }
    public string GpsFlag { get; set; } = null!;
    public string? WarningMessage { get; set; }
}
public class CheckoutDto
{
    [Range(typeof(decimal), "-10000", "10000")]
    public decimal Latitude { get; set; }

    [Range(typeof(decimal), "-10000", "10000")]
    public decimal Longitude { get; set; }

    public DateTimeOffset? Timestamp { get; set; }
}

public class SkipVisitDto
{
    [MaxLength(10240)]
    public string Reason { get; set; } = null!;

    [MaxLength(10240)]
    public string? ReasonNote { get; set; }
}
