using System;
namespace Hano.Core.Application.Contracts.Visits.Dtos;

public class CheckinDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid OutletId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public float GpsAccuracy { get; set; }
    public DateTime Timestamp { get; set; }
    public Guid? PhotoCheckinId { get; set; }
}
public class CheckinResultDto
{
    public Guid VisitId { get; set; }
    public decimal GpsDistanceM { get; set; }
    public string GpsFlag { get; set; } = null!;
    public string? WarningMessage { get; set; }
}
public class CheckoutDto
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}
public class SkipVisitDto
{
    public string Reason { get; set; } = null!;
    public string? ReasonNote { get; set; }
}
