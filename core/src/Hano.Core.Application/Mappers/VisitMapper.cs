using Riok.Mapperly.Abstractions;
using Hano.Core.Application.Contracts.Visits.Dtos;
using Hano.Core.Domain.Visits;

namespace Hano.Core.Application.Mappers;

[Mapper]
public static partial class VisitMapper
{
    public static partial VisitDto ToDto(this Visit source);

    public static VisitDto ToDtoWithOutlet(this Visit source, string outletName)
    {
        var dto = source.ToDto();
        dto.OutletName = outletName;
        return dto;
    }
}
