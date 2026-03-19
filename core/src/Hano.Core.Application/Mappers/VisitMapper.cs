using Riok.Mapperly.Abstractions;

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
