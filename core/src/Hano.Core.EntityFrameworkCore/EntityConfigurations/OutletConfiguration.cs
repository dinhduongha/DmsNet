using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Hano.Core.Domain.Outlets;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Hano.Core.EntityFrameworkCore.EntityConfigurations;

public class OutletConfiguration : IEntityTypeConfiguration<Outlet>
{
    public void Configure(EntityTypeBuilder<Outlet> b)
    {
        b.ToTable("outlets"); b.ConfigureByConvention();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Address).HasMaxLength(500).IsRequired();
        b.Property(x => x.Latitude).HasColumnType("decimal(10,7)");
        b.Property(x => x.Longitude).HasColumnType("decimal(10,7)");
        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.OdsOutletId);
    }
}
