using System;

namespace Hano.Core.BackgroundJobs.Args;

[Serializable]
public class CleanupNotificationsJobArgs
{
    public int RetentionDays { get; set; } = 30;
    public int MaxRecords { get; set; } = 200;
}
