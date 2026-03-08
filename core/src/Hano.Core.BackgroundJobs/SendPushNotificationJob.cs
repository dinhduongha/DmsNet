using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Hano.Core.BackgroundJobs.Args;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace Hano.Core.BackgroundJobs;

/// <summary>
/// Gửi push notification qua FCM.
/// </summary>
public class SendPushNotificationJob : AsyncBackgroundJob<SendPushNotificationJobArgs>, ITransientDependency
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SendPushNotificationJob(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public override async Task ExecuteAsync(SendPushNotificationJobArgs args)
    {
        if (string.IsNullOrEmpty(args.FcmToken))
        {
            Logger.LogWarning("No FCM token for user {UserId}, skip push", args.TargetUserId);
            return;
        }

        try
        {
            var client = _httpClientFactory.CreateClient("FCM");

            var fcmPayload = new
            {
                message = new
                {
                    token = args.FcmToken,
                    notification = new
                    {
                        title = args.Title,
                        body = args.Body,
                    },
                    data = new
                    {
                        notification_id = args.NotificationId.ToString(),
                    }
                }
            };

            var response = await client.PostAsJsonAsync("/v1/projects/hanoimilk/messages:send", fcmPayload);
            response.EnsureSuccessStatusCode();

            Logger.LogInformation("Push sent to {UserId}", args.TargetUserId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Push failed for {UserId}", args.TargetUserId);
            // FCM push failures are not retried — notification is still in DB
        }
    }
}
