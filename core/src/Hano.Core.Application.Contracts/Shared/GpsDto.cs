namespace Hano.Core.Application.Contracts.Dtos;

public class GpsDto
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public float? Accuracy { get; set; }
}
