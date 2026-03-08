using System;
using System.Threading.Tasks;
using Hano.Core.Application.Contracts.Notifications.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Hano.Core.Application.Contracts.Notifications;

/// <summary>
/// Inherits: GetAsync, GetListAsync from IReadOnlyAppService.
/// Custom: MarkAsRead, MarkAllAsRead, GetUnreadCount, Send.
/// </summary>
public interface INotificationAppService
    : IReadOnlyAppService<NotificationDto, Guid, NotifFilterDto>
{
    Task MarkAsReadAsync(Guid id);
    Task MarkAllAsReadAsync();
    Task<int> GetUnreadCountAsync();
    Task SendAsync(SendNotifDto input);
}
