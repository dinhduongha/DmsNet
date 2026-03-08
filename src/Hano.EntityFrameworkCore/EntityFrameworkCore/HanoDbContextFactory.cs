using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Hano.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class HanoDbContextFactory : IDesignTimeDbContextFactory<HanoDbContext>
{
    public HanoDbContext CreateDbContext(string[] args)
    {
        // https://www.npgsql.org/efcore/release-notes/6.0.html#opting-out-of-the-new-timestamp-mapping-logic
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        HanoEfCoreEntityExtensionMappings.Configure();

        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<HanoDbContext>()
            .UseNpgsql(configuration.GetConnectionString("Default"));

        return new HanoDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Hano.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.secrets.json", optional: true) // 👈 thêm dòng này
            ;

        return builder.Build();
    }
}
