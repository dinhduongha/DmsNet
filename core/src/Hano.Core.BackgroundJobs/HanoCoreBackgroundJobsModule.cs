using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.IO;

using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Modularity;
using Hano.Core.Domain;
using Volo.Abp;
using Volo.Abp.BackgroundJobs.Hangfire;

using Hangfire;
using Hangfire.Storage.SQLite;
using Hangfire.PostgreSql;
using Hangfire.Redis.StackExchange;

namespace Hano.Core.BackgroundJobs;

[DependsOn(
    typeof(AbpBackgroundWorkersModule),
    typeof(AbpBackgroundJobsModule),
    typeof(AbpBackgroundJobsHangfireModule),
    typeof(HanoCoreDomainModule)
)]
public class HanoCoreBackgroundJobsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        //... other configurations.

        ConfigureHangfire(context, configuration);
    }

    private void ConfigureHangfire(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddHangfire(config =>
        {
            config.UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSQLiteStorage("Data Source=hano.sqlite;");
        });


        // var redisConnection = configuration.GetConnectionString("Redis");
        // context.Services.AddHangfire(config =>
        // {
        //     config
        //         .UseSimpleAssemblyNameTypeSerializer()
        //         .UseRecommendedSerializerSettings()
        //         .UseRedisStorage(redisConnection, new RedisStorageOptions
        //         {
        //             Prefix = "hangfire:",
        //             InvisibilityTimeout = TimeSpan.FromMinutes(5),
        //             ExpiryCheckInterval = TimeSpan.FromMinutes(30),
        //             DeletedListSize = 1000,
        //             SucceededListSize = 1000
        //         });
        // });
        // context.Services.AddHangfireServer(options =>
        // {
        //     options.WorkerCount = Environment.ProcessorCount * 5;
        //     options.Queues = new[] { "default" };
        // });

        // context.Services.AddHangfire(config =>
        // {
        //     config.UsePostgreSqlStorage(
        //     options =>
        //     {
        //         options.UseNpgsqlConnection(
        //             context.Services
        //                 .GetConfiguration()
        //                 .GetConnectionString("Default")!
        //         );
        //     },
        //     new Hangfire.PostgreSql.PostgreSqlStorageOptions
        //     {
        //         SchemaName = "hangfire",
        //         QueuePollInterval = TimeSpan.FromSeconds(15),
        //         InvisibilityTimeout = TimeSpan.FromMinutes(5)
        //     });
        // });

        context.Services.AddHangfireServer();
    }
    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await context.AddBackgroundWorkerAsync<Workers.MasterDataPollWorker>();
        await context.AddBackgroundWorkerAsync<Workers.CleanupNotificationsWorker>();
    }
}
