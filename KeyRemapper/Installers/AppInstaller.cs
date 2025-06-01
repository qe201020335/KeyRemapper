using System;
using KeyRemapper.Configuration;
using KeyRemapper.InputManagers;
using UnityEngine.XR;
using Zenject;

namespace KeyRemapper.Installers;

internal class AppInstaller : Installer
{
    private readonly PluginConfig _config;
    
    public AppInstaller(PluginConfig config)
    {
        _config = config;
    }
    
    public override void InstallBindings()
    {
        Container.BindInstance(_config);

        var xrName = XRSettings.loadedDeviceName;
        if (xrName.IndexOf("OpenXR", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            Container.BindInterfacesTo<UnityXRInputManager>().AsSingle();
        }
        else
        {
            Plugin.Log.Warn($"Current runtime ({(string.IsNullOrWhiteSpace(xrName) ? "null" : xrName)}) is NOT supported, binding dummy input manager.");
            Container.BindInterfacesTo<DummyInputManager>().AsSingle();
        }
    }
}
