using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Hano.Core.Domain.Notifications;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Hano.Core.EntityFrameworkCore.EntityConfigurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.ToTable("notifications"); b.ConfigureByConvention();
        b.Property(x => x.Type).HasMaxLength(20).IsRequired();
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.HasIndex(x => new { x.TargetUserId, x.IsRead });
    }
}
