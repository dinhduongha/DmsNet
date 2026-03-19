using System;
using System.Linq;
using System.Threading.Tasks;
using Hano.Core.Application.Contracts.Notifications;
using Hano.Core.Application.Mappers;
using Hano.Core.Domain.Notifications;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Hano.Core.Application.Notifications;

public class NotificationAppService
    : AbstractKeyReadOnlyAppService<Notification, NotificationDto, Guid, NotifFilterDto>,
      INotificationAppService
{
    private readonly IRepository<Notification, Guid> _repo;

    public NotificationAppService(IRepository<Notification, Guid> repository)
        : base(repository)
    {
        _repo = repository;
    }

    // ── Mapperly overrides ──

    protected override NotificationDto MapToGetOutputDto(Notification entity) => entity.ToDto();
    protected override NotificationDto MapToGetListOutputDto(Notification entity) => entity.ToDto();

    // ── Override CreateFilteredQueryAsync ──

    protected override async Task<IQueryable<Notification>> CreateFilteredQueryAsync(NotifFilterDto input)
    {
        var q = await base.CreateFilteredQueryAsync(input);
        q = q.Where(x => x.TargetUserId == CurrentUser.Id!.Value);

        if (input.IsRead.HasValue)
            q = q.Where(x => x.IsRead == input.IsRead.Value);
        if (input.Priority.HasValue)
            q = q.Where(x => x.Priority == input.Priority.Value);

        return q.OrderByDescending(x => x.CreationTime);
    }

    // ── Custom business methods ──

    public async Task MarkAsReadAsync(Guid id)
    {
        var n = await _repo.GetAsync(id);
        n.IsRead = true;
        n.ReadAt = DateTime.UtcNow;
        await _repo.UpdateAsync(n);
    }

    public async Task MarkAllAsReadAsync()
    {
        var unread = await _repo.GetListAsync(
            x => x.TargetUserId == CurrentUser.Id!.Value && !x.IsRead);
        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
        }
        await _repo.UpdateManyAsync(unread);
    }

    public async Task<int> GetUnreadCountAsync()
    {
        return (int)await _repo.CountAsync(
            x => x.TargetUserId == CurrentUser.Id!.Value && !x.IsRead);
    }

    public async Task SendAsync(SendNotifDto input)
    {
        foreach (var userId in input.TargetUserIds)
        {
            await _repo.InsertAsync(new Notification
            {
                Id = GuidGenerator.Create(),
                TargetUserId = userId,
                Type = "CUSTOM",
                Title = input.Title,
                Body = input.Message,
                Priority = input.Priority,
                SenderUserId = CurrentUser.Id,
            });
        }
    }

    protected override Task<Notification> GetEntityByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}
