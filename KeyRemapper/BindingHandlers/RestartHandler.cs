using KeyRemapper.Configuration;
using SiraUtil.Logging;
using SiraUtil.Tools.SongControl;
using Zenject;

namespace KeyRemapper.BindingHandlers;

internal class RestartHandler : HandlerBase
{
    [Inject]
    private readonly SiraLog _logger = null!;

    [Inject]
    private readonly ISongControl _songControl = null!;

    private bool _triggered;

    protected override ActionBinding BindingConfig { get; }

    public RestartHandler(PluginConfig config)
    {
        BindingConfig = config.Actions.Restart;
    }

    protected override void Handle()
    {
        if (!BindingConfig.Enabled) return;
        if (_triggered) return;
        _triggered = true;
        _logger.Debug("RestartHandler triggered");
        _songControl.Restart();
    }
}