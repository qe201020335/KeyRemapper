using System;
using KeyRemapper.Configuration;
using KeyRemapper.InputManagers;
using SiraUtil.Affinity;
using Zenject;

namespace KeyRemapper.Patches;

internal class RemapBaseGameMenuButton : IAffinity, IInitializable, IDisposable, ILateTickable
{
    [Inject]
    private readonly IInputManager _inputManager = null!;

    private readonly PauseBinding _binding;

    private bool _wasPauseButtonPressed;

    private RemapBaseGameMenuButton(PluginConfig config)
    {
        _binding = config.Actions.Pause;
    }

    void IInitializable.Initialize()
    {
        _inputManager.ButtonPressed += OnButtonPressed;
    }

    void IDisposable.Dispose()
    {
        _inputManager.ButtonPressed -= OnButtonPressed;
    }

    void ILateTickable.LateTick()
    {
        _wasPauseButtonPressed = false;
    }

    private void OnButtonPressed(ControllerButton button)
    {
        if (_binding.Contains(button)) _wasPauseButtonPressed = true;
    }

    [AffinityPostfix]
    [AffinityPatch(typeof(UnityXRHelper), nameof(UnityXRHelper.GetMenuButtonDown))]
    // 是否在本帧被按下过
    private void PatchGetMenuButtonDown(ref bool __result)
    {
        __result = (!_binding.BlockBuiltIn && __result) || _wasPauseButtonPressed;
    }

    [AffinityPostfix]
    [AffinityPatch(typeof(UnityXRHelper), nameof(UnityXRHelper.GetMenuButton))]
    // 现在是否被按下
    private void PatchGetMenuButton(ref bool __result)
    {
        // Enable 判断
        if (!_binding.Enabled) return;

        // 没有禁用内置按键
        if (!_binding.BlockBuiltIn && __result) return;

        // 避免使用LINQ以防止动态内存分配
        // 直接遍历集合
        foreach (var button in _binding.Bindings)
        {
            if (_inputManager.IsButtonPressedRightNow(button))
            {
                // 修改原始结果并返回
                __result = true;
                return;
            }
        }

        // 没有按下任何绑定的按钮，修改原始结果
        __result = false;
    }
}