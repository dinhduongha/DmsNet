using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Hano.Core.EntityFrameworkCore.EntityConfigurations;

public class FeedbackReportConfiguration : IEntityTypeConfiguration<FeedbackReport>
{
    public void Configure(EntityTypeBuilder<FeedbackReport> b)
    {
        b.ToTable("feedback_reports"); b.ConfigureByConvention();
        b.Property(x => x.Category).HasMaxLength(50).IsRequired();
        b.Property(x => x.Content).HasMaxLength(500).IsRequired();
        b.HasIndex(x => x.VisitId);
        b.HasIndex(x => x.Type);
    }
}
