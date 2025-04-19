using System.ComponentModel;
using SiraUtil.Zenject;
using Zenject;

namespace KeyRemapper
{
    public class KeyRemapperInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<OpenXRPausePatch>().AsSingle();
        }
    }
}