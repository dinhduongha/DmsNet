using System;
using System.Threading.Tasks;
using Hano.Core.Application.Contracts.Photos.Dtos;
using Volo.Abp.Application.Services;
namespace Hano.Core.Application.Contracts.Photos;

public interface IPhotoAppService : IApplicationService
{
    Task<PresignedUrlResponseDto> GetPresignedUrlAsync(PresignedUrlRequestDto input);
    Task ConfirmUploadAsync(PhotoConfirmDto input);
    Task<string> GetDownloadUrlAsync(Guid photoId);
}
