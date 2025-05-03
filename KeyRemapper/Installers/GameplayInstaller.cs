using KeyRemapper.Patches;
using Zenject;

namespace KeyRemapper.Installers;

internal class GameplayInstaller : Installer
{
    public override void InstallBindings()
    {
        if (Container.TryResolve<IVRPlatformHelper>()?.vrPlatformSDK == VRPlatformSDK.OpenXR)
        {
            Container.BindInterfacesTo<RemapBaseGameMenuButton>().AsSingle().NonLazy();
        }
    }
}