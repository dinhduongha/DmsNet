using Riok.Mapperly.Abstractions;
using Hano.Core.Application.Contracts.Outlets.Dtos;
using Hano.Core.Domain.Outlets;

namespace Hano.Core.Application.Mappers;

[Mapper]
public static partial class OutletMapper
{
    [MapProperty(nameof(Outlet.CreationTime), nameof(OutletDto.CreatedAt))]
    public static partial OutletDto ToDto(this Outlet source);
}
