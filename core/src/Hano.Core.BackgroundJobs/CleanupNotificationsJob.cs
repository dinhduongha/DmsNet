using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Hano.Core.BackgroundJobs.Args;
using Hano.Core.Domain.Notifications;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Hano.Core.BackgroundJobs;

/// <summary>
/// Dọn dẹp notifications cũ hơn 30 ngày, giữ tối đa 200/user.
/// Chạy hàng ngày.
/// </summary>
public class CleanupNotificationsJob : AsyncBackgroundJob<CleanupNotificationsJobArgs>, ITransientDependency
{
    private readonly IRepository<Notification, Guid> _notifRepo;

    public CleanupNotificationsJob(IRepository<Notification, Guid> notifRepo)
    {
        _notifRepo = notifRepo;
    }

    public override async Task ExecuteAsync(CleanupNotificationsJobArgs args)
    {
        var cutoff = DateTime.UtcNow.AddDays(-args.RetentionDays);
        var expired = await _notifRepo.GetListAsync(x => x.CreationTime < cutoff);

        if (expired.Any())
        {
            await _notifRepo.DeleteManyAsync(expired);
            Logger.LogInformation("Cleaned up {Count} expired notifications", expired.Count);
        }
    }
}
