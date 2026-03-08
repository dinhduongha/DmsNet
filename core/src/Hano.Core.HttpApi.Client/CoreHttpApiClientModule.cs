using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace Hano.Core;

[DependsOn(
    typeof(CoreApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class CoreHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(CoreApplicationContractsModule).Assembly,
            CoreRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<CoreHttpApiClientModule>();
        });

    }
}
