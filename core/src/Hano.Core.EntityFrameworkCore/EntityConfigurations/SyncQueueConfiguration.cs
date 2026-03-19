using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Hano.Core.EntityFrameworkCore.EntityConfigurations;

public class SyncQueueConfiguration : IEntityTypeConfiguration<SyncQueue>
{
    public void Configure(EntityTypeBuilder<SyncQueue> b)
    {
        b.ToTable("sync_queue"); b.ConfigureByConvention();
        b.Property(x => x.EntityType).HasMaxLength(50).IsRequired();
        b.Property(x => x.Action).HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.Status);
    }
}
