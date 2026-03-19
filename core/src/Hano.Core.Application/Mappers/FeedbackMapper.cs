using Riok.Mapperly.Abstractions;

namespace Hano.Core.Application.Mappers;

[Mapper]
public static partial class FeedbackMapper
{
    [MapProperty(nameof(FeedbackReport.CreationTime), nameof(FeedbackDto.CreatedAt))]
    public static partial FeedbackDto ToDto(this FeedbackReport source);
}
