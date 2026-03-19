using System.IO;
using System.Linq;
using FluentValidation;

namespace Hano.Core.Application.Contracts.Dtos;

public class PresignedUrlRequestDtoValidator : AbstractValidator<PresignedUrlRequestDto>
{
    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
    ];

    private static readonly string[] AllowedExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
    ];

    public PresignedUrlRequestDtoValidator()
    {
        RuleFor(x => x.Filename)
            .NotEmpty()
            .MaximumLength(255)
            .Matches(@"^[a-zA-Z0-9._-]+$")
                .WithMessage("Tên file chỉ được chứa chữ cái, số và các ký tự: . _ -")
            .Must(name => !name.Contains(".."))
                .WithMessage("Tên file không hợp lệ.")
            .Must(name => AllowedExtensions.Contains(
                    Path.GetExtension(name).ToLowerInvariant()))
                .WithMessage($"Chỉ chấp nhận file ảnh: {string.Join(", ", AllowedExtensions)}");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(ct => AllowedContentTypes.Contains(ct.ToLowerInvariant()))
                .WithMessage($"ContentType không hợp lệ. Chỉ chấp nhận: {string.Join(", ", AllowedContentTypes)}");
    }
}
