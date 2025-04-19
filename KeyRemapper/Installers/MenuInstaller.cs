using BeatSaberMarkupLanguage;
using KeyRemapper.UI.ViewControllers;
using UnityEngine;
using Zenject;
using BeatSaberMarkupLanguage.MenuButtons;

namespace KeyRemapper.Installers;

public class MenuInstaller : Installer
{
    // public override void InstallBindings()
    // {
    //     // 将 BSML 界面插入 Mods 侧栏
    //     BSMLGameSettings.instance.AddTab(
    //         "Key Remapper",                                   // 页签标题
    //         "KeyRemapper.UI.Views.ModSettings",               // .bsml 资源路径（去掉 Resources/ 与 .bsml 后缀）
    //         typeof(ModSettingsController));
    // }
    public override void InstallBindings()
    {
        // 1. 在主菜单左侧 Mods 栏添加按钮
        MenuButtons.Instance.RegisterButton(new MenuButton(
            "Key Remapper",                       // 按钮标题
            "Configure controller bindings",      // 悬浮提示
            ShowSettings                         // 点击回调
        ));
    }


    // 2. 点击按钮时弹出设置面板
    public void ShowSettings()
    {
        // 创建宿主对象
        var host = new GameObject("KeyRemapperSettings");
        var ctrl = host.AddComponent<KeyRemapper.UI.ViewControllers.ModSettingsController>();

        // 加载并解析 BSML（确保路径正确：Resources/KeyRemapper/UI/Views/ModSettings.bsml）
        var bsmlTxt = Resources.Load<TextAsset>("KeyRemapper.UI.Views.ModSettings");
        BSMLParser.Instance.Parse(bsmlTxt.text, host, ctrl);

        // 如需依赖注入，可手动 Container.Inject(ctrl);
    }
}