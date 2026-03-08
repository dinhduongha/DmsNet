using System;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Application.Dtos;

namespace Hano.Core.Application.Contracts.Notifications.Dtos;

public class NotificationDto : EntityDto<Guid>
{
    public string Type { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public NotificationPriority Priority { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Deeplink { get; set; }
}

public class SendNotifDto
{
    public Guid[] TargetUserIds { get; set; } = [];
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public NotificationPriority Priority { get; set; }
}

public class NotifFilterDto : PagedAndSortedResultRequestDto
{
    public bool? IsRead { get; set; }
    public NotificationPriority? Priority { get; set; }
}
