using Zenject;

namespace KeyRemapper.Installers;

public class AppInstaller : Installer
{
    public override void InstallBindings()
    {
        // Container.BindInterfacesAndSelfTo<InputMapManager>().AsSingle();
        // Container.BindInterfacesAndSelfTo<PluginConfig>().FromInstance(PluginConfig.Instance).AsSingle();
    }
}
