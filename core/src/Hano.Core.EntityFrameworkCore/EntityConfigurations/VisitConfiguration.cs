using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Hano.Core.EntityFrameworkCore.EntityConfigurations;

public class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> b)
    {
        b.ToTable("visits");
        b.ConfigureByConvention();
        b.Property(x => x.CheckinLatitude).HasColumnType("decimal(10,7)");
        b.Property(x => x.CheckinLongitude).HasColumnType("decimal(10,7)");
        b.Property(x => x.CheckoutLatitude).HasColumnType("decimal(10,7)");
        b.Property(x => x.CheckoutLongitude).HasColumnType("decimal(10,7)");
        b.Property(x => x.GpsDistanceM).HasColumnType("decimal(8,2)");
        b.Property(x => x.SkipReason).HasMaxLength(200);
        b.HasIndex(x => x.SessionId);
        b.HasIndex(x => new { x.UserId, x.CheckinAt });
        b.HasIndex(x => x.OutletId);
    }
}
