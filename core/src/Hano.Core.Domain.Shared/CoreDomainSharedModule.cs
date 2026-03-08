using Volo.Abp.Modularity;
using Volo.Abp.Localization;
using Hano.Core.Localization;
using Volo.Abp.Domain;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Validation;
using Volo.Abp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;
using Bamboo.Shared.Common;

namespace Hano.Core;

[DependsOn(
    typeof(AbpValidationModule),
    typeof(AbpSharedCommonModule),
    typeof(AbpDddDomainSharedModule)
)]
public class CoreDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<CoreDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<CoreResource>("en")
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddVirtualJson("/Localization/Core");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("Core", typeof(CoreResource));
        });
    }
}
