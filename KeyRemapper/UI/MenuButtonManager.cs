using BeatSaberMarkupLanguage.MenuButtons;
using KeyRemapper.UI.FlowCoordinators;
using SiraUtil.Logging;
using Zenject;

namespace KeyRemapper.UI;

public class MenuButtonManager : IInitializable
{
    [Inject]
    private readonly SiraLog _logger = null!;

    [Inject]
    private readonly MainFlowCoordinator _mainFlowCoordinator = null!;

    [Inject]
    private readonly SettingsFlow _menuFlowCoordinator = null!;

    [Inject]
    private readonly MenuButtons _menuButtons = null!;

    private readonly MenuButton _menuButton;

    public MenuButtonManager()
    {
        _menuButton = new MenuButton("Key Remapper", "Configure controller bindings", OnMenuButtonClick);
    }

    public void Initialize()
    {
        _menuButtons.RegisterButton(_menuButton);
        _logger.Info("Key Remapper button registered.");
    }

    private void OnMenuButtonClick()
    {
        // 这个按钮只会在主菜单触发，所以这个设置页面的上级 FlowCoordinator 一定是 MainFlowCoordinator
        _mainFlowCoordinator.PresentFlowCoordinator(_menuFlowCoordinator);
    }
}