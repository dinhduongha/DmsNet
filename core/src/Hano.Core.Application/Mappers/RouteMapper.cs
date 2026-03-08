using Riok.Mapperly.Abstractions;
using Hano.Core.Application.Contracts.Routes.Dtos;
using Hano.Core.Domain.Routes;

namespace Hano.Core.Application.Mappers;

[Mapper]
public static partial class RouteMapper
{
    public static partial RouteDto ToDto(this Route source);
}
