using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Hano.Core.Application.Contracts.Photos;
using Hano.Core.Application.Contracts.Photos.Dtos;
using Hano.Core.Domain.Photos;
using Hano.Core.Domain.Shared;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace Hano.Core.Application.Photos;

public class PhotoAppService : HanoCoreAppServiceBase, IPhotoAppService
{
    private readonly IRepository<Photo, Guid> _photoRepo;
    private readonly IConfiguration _configuration;

    public PhotoAppService(
        IRepository<Photo, Guid> photoRepo,
        IConfiguration configuration)
    {
        _photoRepo = photoRepo;
        _configuration = configuration;
    }

    public async Task<PresignedUrlResponseDto> GetPresignedUrlAsync(PresignedUrlRequestDto input)
    {
        var photoId = GuidGenerator.Create();
        var s3Key = $"uploads/{CurrentUserId}/{DateTime.UtcNow:yyyy/MM/dd}/{photoId}/{input.Filename}";

        // Create photo record (not yet uploaded)
        var photo = new Photo
        {
            Id = photoId,
            S3Key = s3Key,
            Context = input.Context,
            ContentType = input.ContentType,
            UploadedBy = CurrentUserId,
            IsUploaded = false,
        };
        await _photoRepo.InsertAsync(photo);

        // Generate presigned URL
        // In production, use AWS SDK:
        //   var request = new GetPreSignedUrlRequest { BucketName = bucket, Key = s3Key, Verb = HttpVerb.PUT, Expires = expiry };
        //   var url = s3Client.GetPreSignedURL(request);
        var bucket = _configuration["Storage:S3:Bucket"] ?? "hanoimilk-photos";
        var region = _configuration["Storage:S3:Region"] ?? "ap-southeast-1";
        var expiresAt = DateTime.UtcNow.AddMinutes(HanoCoreConsts.PresignedUrlExpiryMin);

        // Placeholder URL — replace with actual S3 presigned URL generation
        var uploadUrl = $"https://{bucket}.s3.{region}.amazonaws.com/{s3Key}?X-Amz-Expires={HanoCoreConsts.PresignedUrlExpiryMin * 60}";

        return new PresignedUrlResponseDto
        {
            PhotoId = photoId,
            UploadUrl = uploadUrl,
            ExpiresAt = expiresAt,
        };
    }

    public async Task ConfirmUploadAsync(PhotoConfirmDto input)
    {
        var photo = await _photoRepo.GetAsync(input.PhotoId);
        if (photo.UploadedBy != CurrentUserId)
            throw new UserFriendlyException("Không có quyền xác nhận ảnh này.");

        photo.IsUploaded = true;
        photo.Latitude = input.Latitude;
        photo.Longitude = input.Longitude;
        photo.VisitId = input.VisitId;
        photo.CapturedAt = DateTime.UtcNow;

        await _photoRepo.UpdateAsync(photo);
    }

    public async Task<string> GetDownloadUrlAsync(Guid photoId)
    {
        var photo = await _photoRepo.GetAsync(photoId);
        if (!photo.IsUploaded)
            throw new UserFriendlyException("Ảnh chưa được upload.");

        // In production, generate presigned GET URL
        var bucket = _configuration["Storage:S3:Bucket"] ?? "hanoimilk-photos";
        var region = _configuration["Storage:S3:Region"] ?? "ap-southeast-1";

        return $"https://{bucket}.s3.{region}.amazonaws.com/{photo.S3Key}?X-Amz-Expires=3600";
    }
}
