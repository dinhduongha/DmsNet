using System;

namespace Hano.Core.BackgroundJobs.Args;

[Serializable]
public class SendPushNotificationJobArgs
{
    public Guid NotificationId { get; set; }
    public Guid TargetUserId { get; set; }
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public string? FcmToken { get; set; }
}
