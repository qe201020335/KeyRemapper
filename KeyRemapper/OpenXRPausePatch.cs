using SiraUtil.Affinity;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.XR;
using SiraUtil.Zenject;
using Zenject;
using HarmonyLib;

namespace KeyRemapper
{
    internal class OpenXRPausePatch : IAffinity, ITickable
    {
        [Inject] private readonly IGamePause           _gamePause;
        [Inject] private readonly PauseController      _pauseController;
        [Inject] private readonly BeatmapObjectManager _beatmapObjectManager;
        [Inject] private readonly PauseMenuManager     _pauseMenuManager;

        // 拿到暂停界面的Continue按钮的方法
        // 用这个东西的原因是如果只调用PauseController的恢复方法，其他mod似乎收不到Continue的相关事件
        // 所以用这个方法假装模拟我们按了继续按钮
        private static readonly MethodInfo _continueMethod =
            AccessTools.Method(typeof(PauseMenuManager),
                "ContinueButtonPressed");

        // 左右手各自的设备列表
        private readonly List<InputDevice> _left  = new();
        private readonly List<InputDevice> _right = new();

        private bool _prevPressed;   // 上一帧“任意键是否按下”

        public void Tick()
        {
            if (!Valid(_left))  Refresh(_left,  true);
            if (!Valid(_right)) Refresh(_right, false);

            bool pressedNow = AnyButtonDown(_left) || AnyButtonDown(_right);

            // Edge detection
            if (pressedNow && !_prevPressed)
            {
                if (_gamePause.isPaused)
                    ResumeViaPublicAPI();
                else
                    _pauseController.Pause();
            }
            _prevPressed = pressedNow;
        }

        // // —— 把官方菜单键吃掉，防止双触发 ———————
        // [AffinityPrefix]
        // [AffinityPatch(typeof(PauseController), nameof(PauseController.Pause))]
        // private bool BlockOriginal() => false;

        private static bool Valid(List<InputDevice> list)
            => list.Count > 0 && list[0].isValid;

        private static void Refresh(List<InputDevice> list, bool isLeft)
        {
            list.Clear();
            InputDevices.GetDevicesWithCharacteristics(
                (isLeft ? InputDeviceCharacteristics.Left : InputDeviceCharacteristics.Right) |
                 InputDeviceCharacteristics.Controller, list);
        }

        /// <summary>检测 primary/secondary 两个按钮是否被按下</summary>
        private static bool AnyButtonDown(List<InputDevice> list)
        {
            foreach (var d in list)
            {
                // primaryButton  = A / X
                if (d.TryGetFeatureValue(CommonUsages.primaryButton, out var v1) && v1)
                    return true;
                // secondaryButton = B / Y
                if (d.TryGetFeatureValue(CommonUsages.secondaryButton, out var v2) && v2)
                    return true;
            }
            return false;
        }

        private void ResumeViaPublicAPI()
        {
            _continueMethod?.Invoke(_pauseMenuManager, null);
        }
    }
}
