using Riok.Mapperly.Abstractions;

namespace Hano.Core.Application.Mappers;

[Mapper]
public static partial class MasterDataMapper
{
    public static partial SkuSyncDto ToSyncDto(this Sku source);
}
