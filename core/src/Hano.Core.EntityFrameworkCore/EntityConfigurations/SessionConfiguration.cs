using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Hano.Core.EntityFrameworkCore.EntityConfigurations;

public class WorkSessionConfiguration : IEntityTypeConfiguration<WorkSession>
{
    public void Configure(EntityTypeBuilder<WorkSession> b)
    {
        b.ToTable("work_sessions");
        b.ConfigureByConvention();
        b.Property(x => x.SodLatitude).HasColumnType("decimal(10,7)");
        b.Property(x => x.SodLongitude).HasColumnType("decimal(10,7)");
        b.Property(x => x.EodLatitude).HasColumnType("decimal(10,7)");
        b.Property(x => x.EodLongitude).HasColumnType("decimal(10,7)");
        b.Property(x => x.TotalDistanceKm).HasColumnType("decimal(8,2)");
        b.Property(x => x.TotalRevenue).HasColumnType("decimal(15,2)");
        b.HasIndex(x => new { x.UserId, x.Date }).IsUnique();
        b.HasMany(x => x.Breadcrumbs).WithOne().HasForeignKey(x => x.SessionId);
    }
}

public class GpsBreadcrumbConfiguration : IEntityTypeConfiguration<GpsBreadcrumb>
{
    public void Configure(EntityTypeBuilder<GpsBreadcrumb> b)
    {
        b.ToTable("gps_breadcrumbs");
        b.ConfigureByConvention();
        b.Property(x => x.Latitude).HasColumnType("decimal(10,7)");
        b.Property(x => x.Longitude).HasColumnType("decimal(10,7)");
        b.HasIndex(x => x.SessionId);
    }
}
