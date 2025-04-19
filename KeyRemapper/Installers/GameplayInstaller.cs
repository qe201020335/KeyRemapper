using Zenject;
using KeyRemapper.Logic;

namespace KeyRemapper.Installers;

public class GameplayInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<OpenXRPausePatch>().AsSingle();
    }
}