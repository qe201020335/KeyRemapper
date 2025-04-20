using KeyRemapper.Logic;
using Zenject;

namespace KeyRemapper.Installers;

/// <summary>
/// 把根容器里的单例转发到主菜单 Scene 的容器，让 UI 能解析到。
/// </summary>
public class MenuBindingsInstaller : Installer
{
    public override void InstallBindings()
    {
        // 复用 AppContext 已创建的单例
        // Container.Bind<InputMapManager>().FromResolve().AsSingle();
        // Container.Bind<Configuration.PluginConfig>().FromResolve().AsSingle();
    }
}