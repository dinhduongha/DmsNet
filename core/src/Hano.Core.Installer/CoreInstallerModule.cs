using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace Hano.Core;

[DependsOn(
    typeof(AbpVirtualFileSystemModule)
    )]
public class CoreInstallerModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<CoreInstallerModule>();
        });
    }
}
