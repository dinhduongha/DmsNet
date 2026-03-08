using Riok.Mapperly.Abstractions;
using Hano.Core.Application.Contracts.MasterData.Dtos;
using Hano.Core.Domain.MasterData;

namespace Hano.Core.Application.Mappers;

[Mapper]
public static partial class MasterDataMapper
{
    public static partial SkuSyncDto ToSyncDto(this Sku source);
}
