using KeyRemapper.UI;
using KeyRemapper.UI.FlowCoordinators;
using KeyRemapper.UI.ViewControllers;
using Zenject;

namespace KeyRemapper.Installers;

public class MenuBindingsInstaller : Installer
{
    public override void InstallBindings()
    {
        // 绑定 UI 组件
        Container.BindInterfacesTo<MenuButtonManager>().AsSingle();
        Container.BindInterfacesAndSelfTo<SettingsFlow>().FromNewComponentOnNewGameObject().AsSingle();
        Container.BindInterfacesAndSelfTo<ModSettingsController>().FromNewComponentAsViewController().AsSingle();
    }
}