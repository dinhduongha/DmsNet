using Riok.Mapperly.Abstractions;
using Hano.Core.Application.Contracts.Notifications.Dtos;
using Hano.Core.Domain.Notifications;

namespace Hano.Core.Application.Mappers;

[Mapper]
public static partial class NotificationMapper
{
    [MapProperty(nameof(Notification.CreationTime), nameof(NotificationDto.CreatedAt))]
    public static partial NotificationDto ToDto(this Notification source);
}
