using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Hano.Core.EntityFrameworkCore.EntityConfigurations;

public class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> b)
    {
        b.ToTable("routes"); b.ConfigureByConvention();
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.HasIndex(x => new { x.AssignedUserId, x.DayOfWeek });
        b.HasMany(x => x.Outlets).WithOne().HasForeignKey(x => x.RouteId);
    }
}

public class RouteOutletConfiguration : IEntityTypeConfiguration<RouteOutlet>
{
    public void Configure(EntityTypeBuilder<RouteOutlet> b)
    {
        b.ToTable("route_outlets"); b.ConfigureByConvention();
        b.HasIndex(x => new { x.RouteId, x.OutletId }).IsUnique();
        b.HasIndex(x => new { x.RouteId, x.SequenceOrder }).IsUnique();
    }
}
