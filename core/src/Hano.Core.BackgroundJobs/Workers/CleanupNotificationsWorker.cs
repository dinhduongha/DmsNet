using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Hano.Core.BackgroundJobs.Args;
using Hano.Core.Domain.Shared;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace Hano.Core.BackgroundJobs.Workers;

/// <summary>
/// Chạy hàng ngày — dọn dẹp notifications cũ.
/// </summary>
public class CleanupNotificationsWorker : AsyncPeriodicBackgroundWorkerBase
{
    public CleanupNotificationsWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = 24 * 60 * 60 * 1000; // 24h
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        Logger.LogInformation("CleanupNotificationsWorker: running...");

        var jobManager = workerContext.ServiceProvider.GetRequiredService<IBackgroundJobManager>();
        await jobManager.EnqueueAsync(new CleanupNotificationsJobArgs
        {
            RetentionDays = HanoCoreConsts.NotifRetentionDays,
            MaxRecords = HanoCoreConsts.NotifMaxRecords
        });
    }
}
