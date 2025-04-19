using SiraUtil.Affinity;
using System.Collections.Generic;
using UnityEngine.XR;
using SiraUtil.Zenject;
using Zenject;

namespace KeyRemapper
{
    internal class OpenXRPausePatch : IAffinity, ITickable
    {
        [Inject] private readonly IGamePause _gamePause;   // Zenject 从游戏里拿单例
        [Inject] private readonly PauseController _pauseController;
        [Inject] private readonly BeatmapObjectManager _beatmapObjectManager;
        [Inject] private readonly PauseMenuManager _pauseMenuManager;

        private readonly List<InputDevice> _left = new();
        private bool _prev;

        public void Tick()
        {
            if (!Valid()) Refresh();
            if (Edge(CommonUsages.primaryButton))
            {
                if (_gamePause.isPaused)
                    ResumeViaPublicAPI();
                else
                    _pauseController.Pause();
            }
        }

        // // —— 把官方菜单键吃掉，防止双触发 ———————
        // [AffinityPrefix]
        // [AffinityPatch(typeof(PauseController), nameof(PauseController.Pause))]
        // private bool BlockOriginal() => false;

        // === helper ===
        private bool Valid() => _left.Count > 0 && _left[0].isValid;
        private void Refresh()
        {
            _left.Clear();
            InputDevices.GetDevicesWithCharacteristics(
                InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller, _left);
        }
        private bool Edge(InputFeatureUsage<bool> key)
        {
            foreach (var d in _left)
                if (d.TryGetFeatureValue(key, out var v) && v)
                {
                    bool down = !_prev;
                    _prev = true;
                    return down;
                }
            _prev = false;
            return false;
        }

        private void ResumeViaPublicAPI()
        {
            // 等价于 HandlePauseMenuManagerDidPressContinueButton()
            _beatmapObjectManager.HideAllBeatmapObjects(false);
            _beatmapObjectManager.PauseAllBeatmapObjects(false);

            _gamePause.WillResume();              // 告诉系统即将恢复
            _pauseMenuManager.StartResumeAnimation();
        }
    }
}
