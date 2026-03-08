using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Hano.Core.Domain.Audit;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Hano.Core.EntityFrameworkCore.EntityConfigurations;

public class OsaReportConfiguration : IEntityTypeConfiguration<OsaReport>
{
    public void Configure(EntityTypeBuilder<OsaReport> b)
    {
        b.ToTable("osa_reports"); b.ConfigureByConvention();
        b.HasIndex(x => x.VisitId);
        b.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.ReportId);
    }
}
public class OsaReportItemConfiguration : IEntityTypeConfiguration<OsaReportItem>
{ public void Configure(EntityTypeBuilder<OsaReportItem> b) { b.ToTable("osa_report_items"); b.ConfigureByConvention(); } }

public class OosReportConfiguration : IEntityTypeConfiguration<OosReport>
{
    public void Configure(EntityTypeBuilder<OosReport> b)
    {
        b.ToTable("oos_reports");

        b.HasKey(x => x.Id);
        b.HasIndex(x => x.VisitId);
        b.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.ReportId);
        b.Property(x => x.Id)
         .HasDefaultValueSql("uuidv7()")
         .HasColumnName("id");
    }
}
public class OosReportItemConfiguration : IEntityTypeConfiguration<OosReportItem>
{ public void Configure(EntityTypeBuilder<OosReportItem> b) { b.ToTable("oos_report_items"); b.ConfigureByConvention(); } }

public class PosmReportConfiguration : IEntityTypeConfiguration<PosmReport>
{
    public void Configure(EntityTypeBuilder<PosmReport> b)
    {
        b.ToTable("posm_reports"); b.ConfigureByConvention();
        b.HasIndex(x => x.VisitId);
        b.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.ReportId);
    }
}
public class PosmReportItemConfiguration : IEntityTypeConfiguration<PosmReportItem>
{ public void Configure(EntityTypeBuilder<PosmReportItem> b) { b.ToTable("posm_report_items"); b.ConfigureByConvention(); } }
