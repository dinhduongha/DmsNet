using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Hano.Core.BackgroundJobs.Args;
using Hano.Core.Domain.Orders;
using Hano.Core.Domain.Sync;
using Hano.Core.Domain.Visits;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Hano.Core.BackgroundJobs;

/// <summary>
/// Đồng bộ dữ liệu sang ODS (TRAIDA).
/// Retry 3 lần: 30s → 2m → 10m backoff.
/// Sau 3 lần thất bại → mark FAILED, alert admin.
/// </summary>
public class SyncToOdsJob : AsyncBackgroundJob<SyncToOdsJobArgs>, ITransientDependency
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IRepository<SyncQueue, Guid> _syncQueueRepo;
    private readonly IRepository<Order, Guid> _orderRepo;
    private readonly IRepository<Visit, Guid> _visitRepo;
    private readonly IBackgroundJobManager _jobManager;

    private static readonly int[] RetryDelaysSeconds = [30, 120, 600];

    public SyncToOdsJob(
        IHttpClientFactory httpClientFactory,
        IRepository<SyncQueue, Guid> syncQueueRepo,
        IRepository<Order, Guid> orderRepo,
        IRepository<Visit, Guid> visitRepo,
        IBackgroundJobManager jobManager)
    {
        _httpClientFactory = httpClientFactory;
        _syncQueueRepo = syncQueueRepo;
        _orderRepo = orderRepo;
        _visitRepo = visitRepo;
        _jobManager = jobManager;
    }

    public override async Task ExecuteAsync(SyncToOdsJobArgs args)
    {
        Logger.LogInformation("SyncToOds: {EntityType} {EntityId} (attempt {Retry})",
            args.EntityType, args.EntityId, args.RetryCount + 1);

        try
        {
            var client = _httpClientFactory.CreateClient("ODS");

            // Build payload based on entity type
            object? payload = args.EntityType switch
            {
                "Order" => await _orderRepo.GetAsync(args.EntityId, includeDetails: true),
                "Visit" => await _visitRepo.GetAsync(args.EntityId),
                _ => null
            };

            if (payload == null)
            {
                Logger.LogWarning("Entity not found: {Type} {Id}", args.EntityType, args.EntityId);
                return;
            }

            // POST to ODS batch endpoint
            var endpoint = args.EntityType switch
            {
                "Visit" => "/api/visits/batch",
                "Order" => "/api/orders/batch",
                _ => $"/api/{args.EntityType.ToLower()}s/batch"
            };

            var response = await client.PostAsJsonAsync(endpoint, new[] { payload });
            response.EnsureSuccessStatusCode();

            // Update sync status
            await UpdateSyncStatus(args.EntityType, args.EntityId, SyncStatus.Synced);

            Logger.LogInformation("SyncToOds SUCCESS: {EntityType} {EntityId}", args.EntityType, args.EntityId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "SyncToOds FAILED: {EntityType} {EntityId}", args.EntityType, args.EntityId);

            if (args.RetryCount < RetryDelaysSeconds.Length)
            {
                // Schedule retry with backoff
                var delay = TimeSpan.FromSeconds(RetryDelaysSeconds[args.RetryCount]);
                args.RetryCount++;
                await _jobManager.EnqueueAsync(args, delay: delay);
            }
            else
            {
                // Max retries exceeded → mark as FAILED
                await UpdateSyncStatus(args.EntityType, args.EntityId, SyncStatus.Failed);
                Logger.LogCritical("SyncToOds MAX RETRIES: {EntityType} {EntityId} — needs manual intervention",
                    args.EntityType, args.EntityId);
                // TODO: Send alert to admin (notification / Slack webhook)
            }
        }
    }

    private async Task UpdateSyncStatus(string entityType, Guid entityId, SyncStatus status)
    {
        if (entityType == "Order")
        {
            var order = await _orderRepo.FindAsync(entityId);
            if (order != null) { order.SyncStatus = status; await _orderRepo.UpdateAsync(order); }
        }
        else if (entityType == "Visit")
        {
            var visit = await _visitRepo.FindAsync(entityId);
            if (visit != null) { visit.SyncStatus = status; await _visitRepo.UpdateAsync(visit); }
        }
    }
}
