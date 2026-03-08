using Riok.Mapperly.Abstractions;
using Hano.Core.Application.Contracts.Feedback.Dtos;
using Hano.Core.Domain.Feedback;

namespace Hano.Core.Application.Mappers;

[Mapper]
public static partial class FeedbackMapper
{
    [MapProperty(nameof(FeedbackReport.CreationTime), nameof(FeedbackDto.CreatedAt))]
    public static partial FeedbackDto ToDto(this FeedbackReport source);
}
