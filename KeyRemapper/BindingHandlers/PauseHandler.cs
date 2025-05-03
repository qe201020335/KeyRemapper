using KeyRemapper.Configuration;
using SiraUtil.Logging;
using SiraUtil.Tools.SongControl;
using Zenject;

namespace KeyRemapper.BindingHandlers;

internal class PauseHandler : HandlerBase
{
    [Inject]
    private readonly SiraLog _logger = null!;

    [Inject]
    private readonly ISongControl _gamePause = null!;

    protected override ActionBinding BindingConfig { get; }

    public PauseHandler(PluginConfig config)
    {
        BindingConfig = config.Actions.Pause;
    }

    protected override void Handle()
    {
        _logger.Debug("PauseHandler triggered");
        if (_gamePause.IsPaused)
        {
            _gamePause.Continue();
        }
        else
        {
            _gamePause.Pause();
        }
    }
}