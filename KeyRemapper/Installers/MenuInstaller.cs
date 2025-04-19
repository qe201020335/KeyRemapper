using Zenject;

namespace KeyRemapper.Installers;

public class MenuInstaller : Installer
{
    public override void InstallBindings()
    {
        // 将 BSML 界面插入 Mods 侧栏
        // BSMLParser.Parse(
        //     Resources.Load<TextAsset>("KeyRemapper.UI.Views.ModSettings").text,
        //     new GameObject("KeyRemapperSettings").AddComponent<ModSettingsController>(),
        //     this);
    }
}