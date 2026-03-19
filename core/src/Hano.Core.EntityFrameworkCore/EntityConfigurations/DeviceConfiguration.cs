using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Hano.Core.EntityFrameworkCore.EntityConfigurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> b)
    {
        b.ToTable("devices");
        b.ConfigureByConvention();
        b.Property(x => x.DeviceId).HasMaxLength(200).IsRequired();
        b.Property(x => x.Platform).HasMaxLength(20).IsRequired();
        b.Property(x => x.Model).HasMaxLength(100);
        b.Property(x => x.FcmToken).HasMaxLength(500);
        b.HasIndex(x => x.DeviceId).IsUnique();
        b.HasIndex(x => x.UserId);
    }
}
