using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Hano.Core.Application.Contracts.Sync;
using Hano.Core.Application.Contracts.Sync.Dtos;
using Hano.Core.Domain.Sync;
using Hano.Core.Domain.Shared.Enums;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;

namespace Hano.Core.Application.Sync;

/// <summary>
/// Offline-first sync: mobile gửi batch items, server xử lý từng item.
/// Conflict detection: server_updated > client_timestamp → conflict.
/// </summary>
public class SyncAppService : HanoCoreAppServiceBase, ISyncAppService
{
    private readonly IRepository<SyncQueue, Guid> _syncQueueRepo;
    private readonly IBackgroundJobManager _jobManager;

    public SyncAppService(
        IRepository<SyncQueue, Guid> syncQueueRepo,
        IBackgroundJobManager jobManager)
    {
        _syncQueueRepo = syncQueueRepo;
        _jobManager = jobManager;
    }

    public async Task<SyncUploadResultDto> UploadAsync(SyncUploadDto input)
    {
        var result = new SyncUploadResultDto();
        var failed = new List<SyncFailedDto>();
        var conflicts = new List<SyncConflictDto>();
        var processed = 0;

        foreach (var item in input.Items)
        {
            try
            {
                // Check for duplicate (idempotency)
                var existing = await _syncQueueRepo.FirstOrDefaultAsync(x => x.Id == item.Uuid);
                if (existing != null)
                {
                    // Already processed — skip silently
                    processed++;
                    continue;
                }

                // Validate payload
                if (string.IsNullOrWhiteSpace(item.Data))
                {
                    failed.Add(new SyncFailedDto { Uuid = item.Uuid, Reason = "Empty payload" });
                    continue;
                }

                // Check for conflicts (server has newer data for this entity)
                // Simple conflict detection: if the entity was already synced after client timestamp
                var existingEntity = await _syncQueueRepo.FirstOrDefaultAsync(
                    x => x.EntityId == item.Uuid
                        && x.Status == SyncQueueStatus.SyncedToOds
                        && x.ServerReceivedAt > item.ClientTimestamp);

                if (existingEntity != null)
                {
                    conflicts.Add(new SyncConflictDto
                    {
                        Uuid = item.Uuid,
                        ConflictType = "SERVER_NEWER",
                    });
                    continue;
                }

                // Enqueue for processing
                var syncEntry = new SyncQueue
                {
                    Id = item.Uuid,
                    EntityType = item.EntityType,
                    EntityId = item.Uuid,
                    Action = item.Action,
                    Payload = item.Data,
                    ClientTimestamp = item.ClientTimestamp,
                    ServerReceivedAt = DateTime.UtcNow,
                    Status = SyncQueueStatus.Pending,
                };

                await _syncQueueRepo.InsertAsync(syncEntry);

                // Enqueue background job to sync to ODS
                await _jobManager.EnqueueAsync(
                    new Hano.Core.BackgroundJobs.Args.SyncToOdsJobArgs
                    {
                        EntityId = item.Uuid,
                        EntityType = item.EntityType,
                        Action = item.Action,
                    });

                processed++;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Sync item failed: {Uuid} {EntityType}", item.Uuid, item.EntityType);
                failed.Add(new SyncFailedDto
                {
                    Uuid = item.Uuid,
                    Reason = ex.Message,
                });
            }
        }

        result.Processed = processed;
        result.Failed = failed;
        result.Conflicts = conflicts;

        return result;
    }

    public async Task ResolveConflictAsync(ResolveConflictDto input)
    {
        var entry = await _syncQueueRepo.GetAsync(input.ConflictId);

        switch (input.Resolution.ToUpperInvariant())
        {
            case "CLIENT_WINS":
                // Re-process with client data overriding server
                entry.Status = SyncQueueStatus.Pending;
                entry.RetryCount = 0;
                await _syncQueueRepo.UpdateAsync(entry);

                await _jobManager.EnqueueAsync(
                    new Hano.Core.BackgroundJobs.Args.SyncToOdsJobArgs
                    {
                        EntityId = entry.EntityId,
                        EntityType = entry.EntityType,
                        Action = entry.Action,
                    });
                break;

            case "SERVER_WINS":
                // Discard client data — mark as resolved
                entry.Status = SyncQueueStatus.SyncedToOds; // effectively resolved
                entry.ErrorMessage = "Resolved: SERVER_WINS";
                await _syncQueueRepo.UpdateAsync(entry);
                break;

            default:
                throw new UserFriendlyException($"Resolution không hợp lệ: {input.Resolution}. Dùng CLIENT_WINS hoặc SERVER_WINS.");
        }
    }
}
