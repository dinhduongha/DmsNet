using Riok.Mapperly.Abstractions;

namespace Hano.Core.Application.Mappers;

[Mapper]
public static partial class OutletMapper
{
    [MapProperty(nameof(Outlet.CreationTime), nameof(OutletDto.CreatedAt))]
    public static partial OutletDto ToDto(this Outlet source);
}
