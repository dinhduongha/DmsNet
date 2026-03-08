using System;
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
/// ABP Background Worker — chạy mỗi 4h poll ODS master data.
/// Đăng ký trong Module: context.AddBackgroundWorkerAsync&lt;MasterDataPollWorker&gt;();
/// </summary>
public class MasterDataPollWorker : AsyncPeriodicBackgroundWorkerBase
{
    public MasterDataPollWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = HanoCoreConsts.OdsPollIntervalHours * 60 * 60 * 1000; // 4h in ms
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        Logger.LogInformation("MasterDataPollWorker: starting poll...");

        var jobManager = workerContext.ServiceProvider.GetRequiredService<IBackgroundJobManager>();
        await jobManager.EnqueueAsync(new PollMasterDataJobArgs
        {
            LastPollTimestamp = DateTime.UtcNow.AddHours(-HanoCoreConsts.OdsPollIntervalHours)
        });
    }
}
