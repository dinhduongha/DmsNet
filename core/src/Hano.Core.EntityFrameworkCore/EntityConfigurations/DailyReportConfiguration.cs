using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Hano.Core.EntityFrameworkCore.EntityConfigurations;

public class DailyReportConfiguration : IEntityTypeConfiguration<DailyReport>
{
    public void Configure(EntityTypeBuilder<DailyReport> b)
    {
        b.ToTable("daily_reports"); b.ConfigureByConvention();
        b.Property(x => x.ReportType).HasMaxLength(10).IsRequired();
        b.HasIndex(x => new { x.UserId, x.Date }).IsUnique();
    }
}
