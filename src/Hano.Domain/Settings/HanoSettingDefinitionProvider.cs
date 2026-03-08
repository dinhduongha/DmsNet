using Volo.Abp.Settings;

namespace Hano.Settings;

public class HanoSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(HanoSettings.MySetting1));
    }
}
