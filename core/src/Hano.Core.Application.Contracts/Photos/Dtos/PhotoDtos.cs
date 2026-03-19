using System;
using Hano.Core.Domain.Shared.Enums;
namespace Hano.Core.Application.Contracts.Dtos;

public class PresignedUrlRequestDto
{
    public string Filename { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public PhotoContext Context { get; set; }
}

public class PresignedUrlResponseDto
{
    public Guid PhotoId { get; set; }
    public string UploadUrl { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}
public class PhotoConfirmDto
{
    public Guid PhotoId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public Guid? VisitId { get; set; }
}
