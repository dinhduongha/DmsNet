using Hano.Core.Domain.Notifications;
using Riok.Mapperly.Abstractions;

namespace Hano.Core.Application.Mappers;

[Mapper]
public static partial class NotificationMapper
{
    [MapProperty(nameof(Notification.CreationTime), nameof(NotificationDto.CreatedAt))]
    public static partial NotificationDto ToDto(this Notification source);
}
