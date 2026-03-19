using System;
using System.ComponentModel.DataAnnotations;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Application.Dtos;

namespace Hano.Core.Application.Contracts.Dtos;

public class NotificationDto : EntityDto<Guid>
{
    [MaxLength(1024)]
    public string Type { get; set; } = null!;

    [MaxLength(1024)]
    public string Title { get; set; } = null!;
    [MaxLength(10240)]
    public string Body { get; set; } = null!;
    public NotificationPriority Priority { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? Deeplink { get; set; }
}

public class SendNotifDto
{
    public Guid[] TargetUserIds { get; set; } = [];

    [MaxLength(1024)]
    public string Title { get; set; } = null!;

    [MaxLength(10240)]
    public string Message { get; set; } = null!;
    public NotificationPriority Priority { get; set; }
}

public class NotifFilterDto : PagedAndSortedResultRequestDto
{
    public bool? IsRead { get; set; }
    public NotificationPriority? Priority { get; set; }
}
