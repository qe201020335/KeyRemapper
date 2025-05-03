using KeyRemapper.BindingHandlers;
using Zenject;

namespace KeyRemapper.Installers;

internal class GameplayInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.BindInterfacesTo<PauseHandler>().AsSingle().NonLazy();
    }
}