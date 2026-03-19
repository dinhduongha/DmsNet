using Riok.Mapperly.Abstractions;

namespace Hano.Core.Application.Mappers;

[Mapper]
public static partial class RouteMapper
{
    public static partial RouteDto ToDto(this Route source);
}
