using System;
using Microsoft.Extensions.DependencyInjection;

namespace Hano.Core.BackgroundJobs;

/// <summary>
/// Đăng ký ODS HttpClient trong Startup/Module:
///   context.Services.AddOdsHttpClient(odsBaseUrl);
/// </summary>
public static class OdsHttpClientExtensions
{
    public static IServiceCollection AddOdsHttpClient(this IServiceCollection services, string baseUrl)
    {
        services.AddHttpClient("ODS", client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            // TODO: Add API key header if required by ODS
        });

        services.AddHttpClient("FCM", client =>
        {
            client.BaseAddress = new Uri("https://fcm.googleapis.com");
            client.Timeout = TimeSpan.FromSeconds(10);
            // TODO: Add OAuth2 Bearer token for FCM v1
        });

        return services;
    }
}
