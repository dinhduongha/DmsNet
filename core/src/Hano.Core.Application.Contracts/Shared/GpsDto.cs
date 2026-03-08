namespace Hano.Core.Application.Contracts.Shared;

public class GpsDto
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public float? Accuracy { get; set; }
}
