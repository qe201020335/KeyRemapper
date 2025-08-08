using HMUI;
using KeyRemapper.UI.ViewControllers;
using Zenject;

namespace KeyRemapper.UI.FlowCoordinators;

// ---------- 自定义 FlowCoordinator ----------
internal class SettingsFlow : FlowCoordinator
{
    [Inject]
    private readonly ModSettingsController _view = null!;
    
    [Inject]
    private readonly MainFlowCoordinator _mainFlowCoordinator = null!;

    protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        if (firstActivation)
        {
            // 顶栏标题
            SetTitle("Key Remapper");
            showBackButton = true;
            ProvideInitialViewControllers(_view);
        }
    }

    protected override void BackButtonWasPressed(ViewController topViewController)
    {
        base.BackButtonWasPressed(topViewController);
        //只会在主菜单触发显示，所以上级 FlowCoordinator 一定是 MainFlowCoordinator
        _mainFlowCoordinator.DismissFlowCoordinator(this);
    }
}