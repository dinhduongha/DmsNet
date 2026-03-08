using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenIddict.Abstractions;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.OpenIddict.Applications;
using Volo.Abp.OpenIddict.Scopes;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;

namespace Hano.OpenIddict;

/* Creates initial data that is needed to property run the application
 * and make client-to-server communication possible.
 */
public class OpenIddictDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IConfiguration _configuration;
    private readonly IOpenIddictApplicationRepository _openIddictApplicationRepository;
    private readonly IAbpApplicationManager _applicationManager;
    private readonly IOpenIddictScopeRepository _openIddictScopeRepository;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly IPermissionDataSeeder _permissionDataSeeder;
    private readonly IStringLocalizer<OpenIddictResponse> L;

    public OpenIddictDataSeedContributor(
        IConfiguration configuration,
        IOpenIddictApplicationRepository openIddictApplicationRepository,
        IAbpApplicationManager applicationManager,
        IOpenIddictScopeRepository openIddictScopeRepository,
        IOpenIddictScopeManager scopeManager,
        IPermissionDataSeeder permissionDataSeeder,
        IStringLocalizer<OpenIddictResponse> l)
    {
        _configuration = configuration;
        _openIddictApplicationRepository = openIddictApplicationRepository;
        _applicationManager = applicationManager;
        _openIddictScopeRepository = openIddictScopeRepository;
        _scopeManager = scopeManager;
        _permissionDataSeeder = permissionDataSeeder;
        L = l;
    }

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        await CreateScopesAsync();
        await CreateApplicationsAsync();
    }

    private async Task CreateScopesAsync()
    {
        if (await _openIddictScopeRepository.FindByNameAsync("Hano") == null)
        {
            await _scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "Hano",
                DisplayName = "Hano API",
                Resources = { "Hano" }
            });
        }
    }

    private async Task CreateApplicationsAsync()
    {
        var commonScopes = new List<string> {
            OpenIddictConstants.Permissions.Scopes.Address,
            OpenIddictConstants.Permissions.Scopes.Email,
            OpenIddictConstants.Permissions.Scopes.Phone,
            OpenIddictConstants.Permissions.Scopes.Profile,
            OpenIddictConstants.Permissions.Scopes.Roles,
            "Hano"
        };

        var configurationSection = _configuration.GetSection("OpenIddict:Applications");
        var sectionName = "Hano";

        //Web MVC Client
        var webClientId = configurationSection["Hano_Web:ClientId"];
        if (!webClientId.IsNullOrWhiteSpace())
        {
            var webClientRootUrl = configurationSection["Hano_Web:RootUrl"]!.EnsureEndsWith('/');
            var redirectUris = configurationSection.GetSection($"{sectionName}_Web:RedirectUris").Get<List<string>>();
            var postLogoutRedirectUris = configurationSection.GetSection($"{sectionName}_Web:PostLogoutRedirectUris").Get<List<string>>();

            /* Hano_Web client is only needed if you created a tiered
             * solution. Otherwise, you can delete this client. */
            await CreateApplicationAsync(
                name: webClientId!,
                type: OpenIddictConstants.ClientTypes.Confidential,
                consentType: OpenIddictConstants.ConsentTypes.Implicit,
                displayName: "Web Application",
                secret: configurationSection["Hano_Web:ClientSecret"] ?? "1q2w3e*",
                grantTypes: new List<string> //Hybrid flow
                {
                    OpenIddictConstants.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.GrantTypes.Implicit
                },
                scopes: commonScopes,
                redirectUri: $"{webClientRootUrl}signin-oidc",
                clientUri: webClientRootUrl,
                postLogoutRedirectUri: $"{webClientRootUrl}signout-callback-oidc",
                redirectUris: redirectUris,
                postLogoutRedirectUris: postLogoutRedirectUris
            );
        }

        // Blazor WebApp Tiered Client
        var blazorWebAppTieredClientId = configurationSection["Hano_BlazorWebAppTiered:ClientId"];
        if (!blazorWebAppTieredClientId.IsNullOrWhiteSpace())
        {
            var blazorWebAppTieredRootUrl = configurationSection["Hano_BlazorWebAppTiered:RootUrl"]!.EnsureEndsWith('/');
            var redirectUris = configurationSection.GetSection($"{sectionName}_BlazorWebAppTiered:RedirectUris").Get<List<string>>();
            var postLogoutRedirectUris = configurationSection.GetSection($"{sectionName}_BlazorWebAppTiered:PostLogoutRedirectUris").Get<List<string>>();

            await CreateApplicationAsync(
                name: blazorWebAppTieredClientId!,
                type: OpenIddictConstants.ClientTypes.Confidential,
                consentType: OpenIddictConstants.ConsentTypes.Implicit,
                displayName: "Blazor Server Application",
                secret: configurationSection["Hano_BlazorWebAppTiered:ClientSecret"] ?? "1q2w3e*",
                grantTypes: new List<string> //Hybrid flow
                {
                    OpenIddictConstants.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.GrantTypes.Implicit
                },
                scopes: commonScopes,
                redirectUri: $"{blazorWebAppTieredRootUrl}signin-oidc",
                clientUri: blazorWebAppTieredRootUrl,
                postLogoutRedirectUri: $"{blazorWebAppTieredRootUrl}signout-callback-oidc",
                redirectUris: redirectUris,
                postLogoutRedirectUris: postLogoutRedirectUris
            );
        }

        // Blazor Server Tiered Client
        var blazorServerTieredClientId = configurationSection[$"{sectionName}_BlazorServerTiered:ClientId"];
        if (!blazorServerTieredClientId.IsNullOrWhiteSpace())
        {
            var blazorServerTieredRootUrl = configurationSection[$"{sectionName}_BlazorServerTiered:RootUrl"]!.EnsureEndsWith('/');
            var redirectUris = configurationSection.GetSection($"{sectionName}_BlazorServerTiered:RedirectUris").Get<List<string>>();
            var postLogoutRedirectUris = configurationSection.GetSection($"{sectionName}_BlazorServerTiered:PostLogoutRedirectUris").Get<List<string>>();
            await CreateApplicationAsync(
                name: blazorServerTieredClientId!,
                type: OpenIddictConstants.ClientTypes.Confidential,
                consentType: OpenIddictConstants.ConsentTypes.Implicit,
                displayName: "Blazor Server Application",
                secret: configurationSection[$"{sectionName}_BlazorServerTiered:ClientSecret"] ?? "1q2w3E*",
                grantTypes: new List<string> //Hybrid flow
                {
                    OpenIddictConstants.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.GrantTypes.Implicit
                },
                scopes: commonScopes,
                redirectUri: $"{blazorServerTieredRootUrl}signin-oidc",
                clientUri: blazorServerTieredRootUrl,
                postLogoutRedirectUri: $"{blazorServerTieredRootUrl}signout-callback-oidc",
                redirectUris: redirectUris,
                postLogoutRedirectUris: postLogoutRedirectUris
            );
        }

        // Mobile Client
        var MobileClientId = configurationSection[$"{sectionName}_Mobile:ClientId"];
        if (!MobileClientId.IsNullOrWhiteSpace())
        {
            var MobileRootUrl = configurationSection[$"{sectionName}_Mobile:RootUrl"]!.EnsureEndsWith('/');
            var redirectUris = configurationSection.GetSection($"{sectionName}_Mobile:RedirectUris").Get<List<string>>();
            var postLogoutRedirectUris = configurationSection.GetSection($"{sectionName}_Mobile:PostLogoutRedirectUris").Get<List<string>>();

            var mobileScopes = commonScopes;
            mobileScopes.AddFirst("offline_access");
            await CreateApplicationAsync(
                name: MobileClientId,
                type: OpenIddictConstants.ClientTypes.Confidential,
                consentType: OpenIddictConstants.ConsentTypes.Implicit,
                displayName: "Mobile Application",
                scopes: mobileScopes,
                grantTypes: new List<string>
                {
                    OpenIddictConstants.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.GrantTypes.RefreshToken,
                    OpenIddictConstants.GrantTypes.Password
                },
                secret: configurationSection[$"{sectionName}_Mobile:ClientSecret"] ?? "1q2w3E*",
                clientUri: MobileRootUrl,
                redirectUri: $"{MobileRootUrl}authenticated",
                postLogoutRedirectUri: $"{MobileRootUrl}signout-callback-oidc",
                redirectUris: redirectUris,
                postLogoutRedirectUris: postLogoutRedirectUris
            );
        }

        // Internal Client
        var internalClientId = configurationSection[$"{sectionName}_Internal:ClientId"];
        if (!internalClientId.IsNullOrWhiteSpace())
        {
            var internalRootUrl = configurationSection[$"{sectionName}_Internal:RootUrl"]!.EnsureEndsWith('/');
            var redirectUris = configurationSection.GetSection($"{sectionName}_Internal:RedirectUris").Get<List<string>>();
            var postLogoutRedirectUris = configurationSection.GetSection($"{sectionName}_Internal:PostLogoutRedirectUris").Get<List<string>>();

            var mobileScopes = commonScopes;
            mobileScopes.AddFirst("offline_access");
            await CreateApplicationAsync(
                name: internalClientId,
                type: OpenIddictConstants.ClientTypes.Confidential,
                consentType: OpenIddictConstants.ConsentTypes.Implicit,
                displayName: "Internal Application",
                scopes: mobileScopes,
                grantTypes: new List<string>
                {
                    OpenIddictConstants.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.GrantTypes.Password,
                    OpenIddictConstants.GrantTypes.ClientCredentials,
                    OpenIddictConstants.GrantTypes.RefreshToken
                },
                secret: configurationSection[$"{sectionName}_Internal:ClientSecret"] ?? "1q2w3E*",
                clientUri: internalRootUrl,
                redirectUri: $"{internalRootUrl}authenticated",
                redirectUris: redirectUris,
                postLogoutRedirectUri: $"{internalRootUrl}signout-callback-oidc",
                postLogoutRedirectUris: postLogoutRedirectUris
            );
        }

        //Flutter Client
        var flutterClientId = configurationSection[$"{sectionName}_FlutterApp:ClientId"];
        if (!flutterClientId.IsNullOrWhiteSpace())
        {
            var flutterClientRootUrl = configurationSection[$"{sectionName}_FlutterApp:RootUrl"]?.TrimEnd('/');
            var redirectUris = configurationSection.GetSection($"{sectionName}_FlutterApp:RedirectUris").Get<List<string>>();
            var postLogoutRedirectUris = configurationSection.GetSection($"{sectionName}_FlutterApp:PostLogoutRedirectUris").Get<List<string>>();
            var mobileScopes = commonScopes;
            mobileScopes.AddFirst("offline_access");

            await CreateApplicationAsync(
                name: flutterClientId!,
                type: OpenIddictConstants.ClientTypes.Public,
                consentType: OpenIddictConstants.ConsentTypes.Implicit,
                displayName: "Flutter Application",
                //secret: configurationSection[$"{sectionName}_FlutterApp:ClientSecret"] ?? "1q2w3E*",
                secret: null,
                grantTypes: new List<string> {
                    OpenIddictConstants.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.GrantTypes.Password,
                    OpenIddictConstants.GrantTypes.ClientCredentials,
                    OpenIddictConstants.GrantTypes.RefreshToken
                },
                scopes: mobileScopes,
                redirectUri: $"{flutterClientRootUrl}/auth.html",
                clientUri: flutterClientRootUrl,
                postLogoutRedirectUri: flutterClientRootUrl,
                redirectUris: redirectUris,
                postLogoutRedirectUris: postLogoutRedirectUris
            );
        }

        // React Client
        var reactClientId = configurationSection[$"{sectionName}_React:ClientId"];
        if (!reactClientId.IsNullOrWhiteSpace())
        {
            var reactClientRootUrl = configurationSection[$"{sectionName}_React:RootUrl"]?.TrimEnd('/');
            var redirectUris = configurationSection.GetSection($"{sectionName}_React:RedirectUris").Get<List<string>>();
            var postLogoutRedirectUris = configurationSection.GetSection($"{sectionName}_React:PostLogoutRedirectUris").Get<List<string>>();
            try
            {
                await CreateApplicationAsync(
                    name: reactClientId,
                    type: OpenIddictConstants.ClientTypes.Public,
                    consentType: OpenIddictConstants.ConsentTypes.Implicit,
                    displayName: "React Application",
                    secret: null,
                    grantTypes: new List<string>
                    {
                        OpenIddictConstants.GrantTypes.AuthorizationCode,
                        OpenIddictConstants.GrantTypes.Password,
                        OpenIddictConstants.GrantTypes.ClientCredentials,
                        OpenIddictConstants.GrantTypes.RefreshToken,
                        "switch_tenant",
                        "siwx"
                    },
                    scopes: commonScopes,
                    clientUri: reactClientRootUrl,
                    redirectUri: $"{reactClientRootUrl}/auth/openiddict",
                    postLogoutRedirectUri: $"{reactClientRootUrl}/auth/openiddict/logout-callback",
                    redirectUris: redirectUris,
                    postLogoutRedirectUris: postLogoutRedirectUris
                    );
            }
            catch (Exception e)
            {
                throw;
            }
        }


        // Blazor Client
        var blazorClientId = configurationSection["Hano_Blazor:ClientId"];
        if (!blazorClientId.IsNullOrWhiteSpace())
        {
            var blazorRootUrl = configurationSection["Hano_Blazor:RootUrl"]?.TrimEnd('/');
            var redirectUris = configurationSection.GetSection($"{sectionName}_Blazor:RedirectUris").Get<List<string>>();
            var postLogoutRedirectUris = configurationSection.GetSection($"{sectionName}_Blazor:PostLogoutRedirectUris").Get<List<string>>();
            await CreateApplicationAsync(
                name: blazorClientId!,
                type: OpenIddictConstants.ClientTypes.Public,
                consentType: OpenIddictConstants.ConsentTypes.Implicit,
                displayName: "Blazor Application",
                secret: null,
                grantTypes: new List<string> { OpenIddictConstants.GrantTypes.AuthorizationCode, },
                scopes: commonScopes,
                redirectUri: $"{blazorRootUrl}/authentication/login-callback",
                clientUri: blazorRootUrl,
                postLogoutRedirectUri: $"{blazorRootUrl}/authentication/logout-callback",
                redirectUris: redirectUris,
                postLogoutRedirectUris: postLogoutRedirectUris
            );
        }

        //Console Test / Angular Client
        var consoleAndAngularClientId = configurationSection["Hano_App:ClientId"];
        if (!consoleAndAngularClientId.IsNullOrWhiteSpace())
        {
            var consoleAndAngularClientRootUrl = configurationSection["Hano_App:RootUrl"]?.TrimEnd('/');
            var redirectUris = configurationSection.GetSection($"{sectionName}_App:RedirectUris").Get<List<string>>();
            var postLogoutRedirectUris = configurationSection.GetSection($"{sectionName}_App:PostLogoutRedirectUris").Get<List<string>>();
            await CreateApplicationAsync(
                name: consoleAndAngularClientId!,
                type: OpenIddictConstants.ClientTypes.Public,
                consentType: OpenIddictConstants.ConsentTypes.Implicit,
                displayName: "Console Test / Angular Application",
                secret: null,
                grantTypes: new List<string> {
                    OpenIddictConstants.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.GrantTypes.Password,
                    OpenIddictConstants.GrantTypes.ClientCredentials,
                    OpenIddictConstants.GrantTypes.RefreshToken
                },
                scopes: commonScopes,
                redirectUri: consoleAndAngularClientRootUrl,
                clientUri: consoleAndAngularClientRootUrl,
                postLogoutRedirectUri: consoleAndAngularClientRootUrl,
                redirectUris: redirectUris,
                postLogoutRedirectUris: postLogoutRedirectUris
            );
        }

        // Swagger Client
        var swaggerClientId = configurationSection["Hano_Swagger:ClientId"];
        if (!swaggerClientId.IsNullOrWhiteSpace())
        {
            var swaggerRootUrl = configurationSection["Hano_Swagger:RootUrl"]?.TrimEnd('/');
            var redirectUris = configurationSection.GetSection($"{sectionName}_Swagger:RedirectUris").Get<List<string>>();
            var postLogoutRedirectUris = configurationSection.GetSection($"{sectionName}_Swagger:PostLogoutRedirectUris").Get<List<string>>();
            await CreateApplicationAsync(
                name: swaggerClientId!,
                type: OpenIddictConstants.ClientTypes.Public,
                consentType: OpenIddictConstants.ConsentTypes.Implicit,
                displayName: "Swagger Application",
                secret: null,
                grantTypes: new List<string> { OpenIddictConstants.GrantTypes.AuthorizationCode, },
                scopes: commonScopes,
                redirectUri: $"{swaggerRootUrl}/swagger/oauth2-redirect.html",
                clientUri: swaggerRootUrl,
                redirectUris: redirectUris,
                postLogoutRedirectUris: postLogoutRedirectUris
            );
        }
    }

    private async Task CreateApplicationAsync(
        [NotNull] string name,
        [NotNull] string type,
        [NotNull] string consentType,
        string displayName,
        string? secret,
        List<string> grantTypes,
        List<string> scopes,
        string? clientUri = null,
        string? redirectUri = null,
        string? postLogoutRedirectUri = null,
        List<string>? permissions = null,
        List<string>? redirectUris = null,
        List<string>? postLogoutRedirectUris = null)
    {
        if (!string.IsNullOrEmpty(secret) && string.Equals(type, OpenIddictConstants.ClientTypes.Public,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException(L["NoClientSecretCanBeSetForPublicApplications"]);
        }

        if (string.IsNullOrEmpty(secret) && string.Equals(type, OpenIddictConstants.ClientTypes.Confidential,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException(L["TheClientSecretIsRequiredForConfidentialApplications"]);
        }

        var client = await _openIddictApplicationRepository.FindByClientIdAsync(name);

        var application = new AbpApplicationDescriptor
        {
            ClientId = name,
            ClientType = type,
            ClientSecret = secret,
            ConsentType = consentType,
            DisplayName = displayName,
            ClientUri = clientUri,
        };

        Check.NotNullOrEmpty(grantTypes, nameof(grantTypes));
        Check.NotNullOrEmpty(scopes, nameof(scopes));

        if (new[] { OpenIddictConstants.GrantTypes.AuthorizationCode, OpenIddictConstants.GrantTypes.Implicit }.All(
                grantTypes.Contains))
        {
            application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.CodeIdToken);

            if (string.Equals(type, OpenIddictConstants.ClientTypes.Public, StringComparison.OrdinalIgnoreCase))
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.CodeIdTokenToken);
                application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.CodeToken);
            }
        }

        if (!redirectUri.IsNullOrWhiteSpace() || !postLogoutRedirectUri.IsNullOrWhiteSpace())
        {
            application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.EndSession);
        }

        var buildInGrantTypes = new[] {
            OpenIddictConstants.GrantTypes.Implicit, OpenIddictConstants.GrantTypes.Password,
            OpenIddictConstants.GrantTypes.AuthorizationCode, OpenIddictConstants.GrantTypes.ClientCredentials,
            OpenIddictConstants.GrantTypes.DeviceCode, OpenIddictConstants.GrantTypes.RefreshToken
        };

        foreach (var grantType in grantTypes)
        {
            if (grantType == OpenIddictConstants.GrantTypes.AuthorizationCode)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
                application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.Code);
            }

            if (grantType == OpenIddictConstants.GrantTypes.AuthorizationCode ||
                grantType == OpenIddictConstants.GrantTypes.Implicit)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Authorization);
            }

            if (grantType == OpenIddictConstants.GrantTypes.AuthorizationCode ||
                grantType == OpenIddictConstants.GrantTypes.ClientCredentials ||
                grantType == OpenIddictConstants.GrantTypes.Password ||
                grantType == OpenIddictConstants.GrantTypes.RefreshToken ||
                grantType == OpenIddictConstants.GrantTypes.DeviceCode)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
                application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Revocation);
                application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Introspection);
            }

            if (grantType == OpenIddictConstants.GrantTypes.ClientCredentials)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials);
            }

            if (grantType == OpenIddictConstants.GrantTypes.Implicit)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.Implicit);
            }

            if (grantType == OpenIddictConstants.GrantTypes.Password)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.Password);
            }

            if (grantType == OpenIddictConstants.GrantTypes.RefreshToken)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.RefreshToken);
            }

            if (grantType == OpenIddictConstants.GrantTypes.DeviceCode)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.DeviceCode);
                application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.DeviceAuthorization);
            }

            if (grantType == OpenIddictConstants.GrantTypes.Implicit)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.IdToken);
                if (string.Equals(type, OpenIddictConstants.ClientTypes.Public, StringComparison.OrdinalIgnoreCase))
                {
                    application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.IdTokenToken);
                    application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.Token);
                }
            }

            if (!buildInGrantTypes.Contains(grantType))
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.GrantType + grantType);
            }
        }

        var buildInScopes = new[] {
            OpenIddictConstants.Permissions.Scopes.Address, OpenIddictConstants.Permissions.Scopes.Email,
            OpenIddictConstants.Permissions.Scopes.Phone, OpenIddictConstants.Permissions.Scopes.Profile,
            OpenIddictConstants.Permissions.Scopes.Roles
        };

        foreach (var scope in scopes)
        {
            if (buildInScopes.Contains(scope))
            {
                application.Permissions.Add(scope);
            }
            else
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + scope);
            }
        }

        if (redirectUri != null)
        {
            if (!redirectUri.IsNullOrEmpty())
            {
                if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out var uri) || !uri.IsWellFormedOriginalString())
                {
                    throw new BusinessException(L["InvalidRedirectUri", redirectUri]);
                }

                if (application.RedirectUris.All(x => x != uri))
                {
                    application.RedirectUris.Add(uri);
                }
            }
        }
        if (redirectUris != null)
        {
            foreach (var r in redirectUris)
            {
                if (!r.IsNullOrEmpty())
                {
                    var redirect = r.TrimEnd('/');
                    if (!Uri.TryCreate(redirect, UriKind.Absolute, out var uri) || !uri.IsWellFormedOriginalString())
                    {
                        throw new BusinessException(L["InvalidRedirectUri", redirect]);
                    }

                    if (application.RedirectUris.All(x => x != uri))
                    {
                        application.RedirectUris.Add(uri);
                    }
                }
            }
        }
        if (postLogoutRedirectUri != null)
        {
            if (!postLogoutRedirectUri.IsNullOrEmpty())
            {
                if (!Uri.TryCreate(postLogoutRedirectUri, UriKind.Absolute, out var uri) ||
                    !uri.IsWellFormedOriginalString())
                {
                    throw new BusinessException(L["InvalidPostLogoutRedirectUri", postLogoutRedirectUri]);
                }

                if (application.PostLogoutRedirectUris.All(x => x != uri))
                {
                    application.PostLogoutRedirectUris.Add(uri);
                }
            }
        }

        if (permissions != null)
        {
            await _permissionDataSeeder.SeedAsync(
                ClientPermissionValueProvider.ProviderName,
                name,
                permissions,
                null
            );
        }

        if (client == null)
        {
            await _applicationManager.CreateAsync(application);
            return;
        }

        if (!HasSameRedirectUris(client, application))
        {
            client.RedirectUris = JsonSerializer.Serialize(application.RedirectUris.Select(q => q.ToString().TrimEnd('/')));
            client.PostLogoutRedirectUris = JsonSerializer.Serialize(application.PostLogoutRedirectUris.Select(q => q.ToString().TrimEnd('/')));

            await _applicationManager.UpdateAsync(client.ToModel());
        }

        if (!HasSameScopes(client, application))
        {
            client.Permissions = JsonSerializer.Serialize(application.Permissions.Select(q => q.ToString()));
            await _applicationManager.UpdateAsync(client.ToModel());
        }
    }

    private bool HasSameRedirectUris(OpenIddictApplication existingClient, AbpApplicationDescriptor application)
    {
        return existingClient.RedirectUris == JsonSerializer.Serialize(application.RedirectUris.Select(q => q.ToString().TrimEnd('/')));
    }

    private bool HasSameScopes(OpenIddictApplication existingClient, AbpApplicationDescriptor application)
    {
        return existingClient.Permissions == JsonSerializer.Serialize(application.Permissions.Select(q => q.ToString().TrimEnd('/')));
    }
}
