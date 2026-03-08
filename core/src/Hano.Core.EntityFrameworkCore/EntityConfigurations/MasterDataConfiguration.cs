using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Hano.Core.Domain.MasterData;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Hano.Core.EntityFrameworkCore.EntityConfigurations;

public class SkuConfiguration : IEntityTypeConfiguration<Sku>
{
    public void Configure(EntityTypeBuilder<Sku> b)
    {
        b.ToTable("skus"); b.ConfigureByConvention();
        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.HasIndex(x => x.Code).IsUnique();
    }
}

public class PriceListConfiguration : IEntityTypeConfiguration<PriceList>
{
    public void Configure(EntityTypeBuilder<PriceList> b)
    {
        b.ToTable("price_lists"); b.ConfigureByConvention();
        b.Property(x => x.UnitPrice).HasColumnType("decimal(12,2)");
        b.Property(x => x.PromoPrice).HasColumnType("decimal(12,2)");
        b.HasIndex(x => new { x.SkuId, x.DistributorId, x.EffectiveFrom });
    }
}

public class DistributorConfiguration : IEntityTypeConfiguration<Distributor>
{
    public void Configure(EntityTypeBuilder<Distributor> b)
    {
        b.ToTable("distributors"); b.ConfigureByConvention();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
    }
}
