using Hano.Core.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Hano.Core.Permissions;

public class CorePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(CorePermissions.GroupName, L("Permission:Core"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<CoreResource>(name);
    }
}
