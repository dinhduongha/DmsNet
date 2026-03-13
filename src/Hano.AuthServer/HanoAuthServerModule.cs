using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.RateLimiting;

using Localization.Resources.AbpUi;
using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Http;

using StackExchange.Redis;
using OpenIddict.Server.AspNetCore;

using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Mvc.UI;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Auditing;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Caching;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.Security.Claims;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.UI;
using Volo.Abp.VirtualFileSystem;
using Volo.Abp.Account.Localization;
using Microsoft.Extensions.Configuration;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.MultiTenancy;

using Hano.EntityFrameworkCore;
using Hano.Localization;
using Hano.MultiTenancy;

namespace Hano;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpCachingStackExchangeRedisModule),
    typeof(AbpDistributedLockingModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpAccountHttpApiModule),
    typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
    typeof(HanoEntityFrameworkCoreModule),
    typeof(AbpAspNetCoreSerilogModule)
    )]
public class HanoAuthServerModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("Hano");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });

        PreConfigure<OpenIddictServerBuilder>(builder =>
        {
            builder.SetAuthorizationCodeLifetime(TimeSpan.FromHours(1));
            builder.SetAccessTokenLifetime(TimeSpan.FromDays(7));
            builder.SetRefreshTokenLifetime(TimeSpan.FromDays(100));
            var issuer = configuration["AuthServer:Authority"];
            if (!string.IsNullOrWhiteSpace(issuer))
            {
                builder.SetIssuer(new Uri(issuer));
            }
        });

        //if (!hostingEnvironment.IsDevelopment())
        {
            var pfxPassword = configuration["AuthServer:PfxPassword"];
            PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
            {
                options.AddDevelopmentEncryptionAndSigningCertificate = false;
            });

            if (!string.IsNullOrWhiteSpace(pfxPassword))
            {

                PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
                {
                    serverBuilder.AddProductionEncryptionAndSigningCertificate("openiddict.pfx", pfxPassword);
                });
            }
        }

        var disableHttps = configuration.GetValue("AuthServer:DisableTransportSecurityRequirement", false);

        if (disableHttps)
        {
            PreConfigure<OpenIddictServerAspNetCoreBuilder>(builder =>
            {
                builder.DisableTransportSecurityRequirement();
            });
        }
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        Configure<OpenIddictServerBuilder>(builder =>
        {
            // Ép buộc Issuer từ appsettings.json
            var issuer = configuration["AuthServer:Authority"];
            if (!string.IsNullOrWhiteSpace(issuer))
            {
                builder.SetIssuer(new Uri(issuer));
            }
        });
        var disableHttps = configuration.GetValue("AuthServer:DisableTransportSecurityRequirement", false);
        if (disableHttps)
        {
            Configure<OpenIddictServerAspNetCoreOptions>(options =>
            {
                options.DisableTransportSecurityRequirement = disableHttps;
            });
        }
        var section = configuration.GetSection("Authentication:Google");
        if (section.Exists() && section.GetValue<bool>("Enable", false))
        {
            var clientId = section["ClientId"] ?? "";
            var clientSecret = section["ClientSecret"] ?? "";
            context.Services.AddAuthentication()
                .AddGoogle(options =>
            {
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.Events.OnRedirectToAuthorizationEndpoint = context =>
                {
                    var uri = context.RedirectUri + "&prompt=select_account";
                    context.Response.Redirect(uri);
                    return Task.CompletedTask;
                };
            });
        }
        section = configuration.GetSection("Authentication:Microsoft");
        if (section.Exists() && section.GetValue<bool>("Enable", false))
        {
            context.Services.AddAuthentication()
                .AddMicrosoftAccount(options =>
            {
                options.ClientId = section["ClientId"]!;
                options.ClientSecret = section["ClientSecret"]!;
            });
        }
        ConfigureTenantResolver(context, configuration);
        context.Services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("auth-limit", opt =>
            {
                opt.PermitLimit = 100;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 10;
            });
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });


        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<HanoResource>()
                .AddBaseTypes(
                    typeof(AbpUiResource),
                    typeof(AccountResource)
                );
        });

        Configure<AbpBundlingOptions>(options =>
        {
            options.StyleBundles.Configure(
                LeptonXLiteThemeBundles.Styles.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-styles.css");
                }
            );
        });

        Configure<AbpAuditingOptions>(options =>
        {
            //options.IsEnabledForGetRequests = true;
            options.ApplicationName = "AuthServer";
        });

        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<HanoDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}Hano.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<HanoDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}Hano.Domain"));
            });
        }

        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            options.RedirectAllowedUrls.AddRange(configuration["App:RedirectAllowedUrls"]?.Split(',') ?? Array.Empty<string>());

            options.Applications["Angular"].RootUrl = configuration["App:ClientUrl"];
            options.Applications["Angular"].Urls[AccountUrlNames.PasswordReset] = "account/reset-password";
        });

        Configure<AbpBackgroundJobOptions>(options =>
        {
            options.IsJobExecutionEnabled = false;
        });

        Configure<AbpDistributedCacheOptions>(options =>
        {
            options.KeyPrefix = "Hano:";
        });

        var dataProtectionBuilder = context.Services.AddDataProtection().SetApplicationName("Hano");
        if (!hostingEnvironment.IsDevelopment())
        {
            var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]!);
            dataProtectionBuilder.PersistKeysToStackExchangeRedis(redis, "Hano-Protection-Keys");
        }

        context.Services.AddSingleton<IDistributedLockProvider>(sp =>
        {
            var connection = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]!);
            return new RedisDistributedSynchronizationProvider(connection.GetDatabase());
        });

        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(
                        configuration["App:CorsOrigins"]?
                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(o => o.RemovePostFix("/"))
                            .ToArray() ?? Array.Empty<string>()
                    )
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
    }
    private void ConfigureTenantResolver(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.Configure<AbpAspNetCoreMultiTenancyOptions>(options =>
        {
            options.TenantKey = configuration["App:TenantKey"] ?? "tenant";
        });

        //bool enabledTenantLogin = Convert.ToBoolean(configuration["App:EnableTenantLogin"]);
        bool enabledTenantLogin = configuration.GetValue("App:EnableTenantLogin", false);
        if (!enabledTenantLogin)
        {
            Configure<AbpTenantResolveOptions>(options =>
            {
                options.TenantResolvers.Clear();
                options.TenantResolvers.Add(new CurrentUserTenantResolveContributor());
            });
        }
    }
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();
        var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.All
        });
        var logHeaders = configuration.GetValue<bool>("Logging:RequestLogging:LogHeaders");
        if (logHeaders)
        {
            // Lấy logger từ LoggerFactory (không phụ thuộc vào Program)
            var loggerFactory = context.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<HanoAuthServerModule>();

            app.Use(async (ctx, next) =>
            {
                logger.LogInformation("===== Incoming Request =====");
                logger.LogInformation("{Method} {Scheme}://{Host}{Path}{QueryString}",
                    ctx.Request.Method,
                    ctx.Request.Scheme,
                    ctx.Request.Host,
                    ctx.Request.Path,
                    ctx.Request.QueryString);

                foreach (var header in ctx.Request.Headers)
                {
                    logger.LogInformation("Header: {Key} = {Value}", header.Key, header.Value.ToString());
                }

                logger.LogInformation("============================");

                await next();
            });
        }

        ///// Always behind ssl proxy
        var useSSL = configuration.GetValue<bool>("App:UseSSL");
        if (useSSL)
        {
            app.Use((context, next) =>
            {
                var xproto = context.Request.Headers["X-Forwarded-Proto"].ToString();
                if (xproto != null && xproto.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    context.Request.Scheme = "https";
                }
                return next();
            });
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
        }

        app.UseCorrelationId();
        app.MapAbpStaticAssets();
        app.UseRateLimiter();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();

        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
