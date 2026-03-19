using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Hano.Core.EntityFrameworkCore.EntityConfigurations;

public class PhotoConfiguration : IEntityTypeConfiguration<Photo>
{
    public void Configure(EntityTypeBuilder<Photo> b)
    {
        b.ToTable("photos"); b.ConfigureByConvention();
        b.Property(x => x.S3Key).HasMaxLength(500).IsRequired();
        b.Property(x => x.Latitude).HasColumnType("decimal(10,7)");
        b.Property(x => x.Longitude).HasColumnType("decimal(10,7)");
        b.HasIndex(x => x.VisitId);
    }
}
