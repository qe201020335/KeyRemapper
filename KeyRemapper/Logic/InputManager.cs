using System;
using System.Collections.Generic;
using System.Linq;
using IPA.Logging;
using UnityEngine.XR;
using Zenject;
using KeyRemapper.Configuration;

namespace KeyRemapper.Logic;

/// <summary>
/// 负责在运行时判断“某个动作是否被触发”以及对 Config 做读写。
/// </summary>
public class InputMapManager : IInitializable, IDisposable
{
    private readonly PluginConfig _config;
    private readonly Dictionary<string, InputDevice> _deviceCache = new();

    // 运行期缓存：动作 → 解析好的 ButtonBinding 列表
    private readonly Dictionary<string, List<ButtonBinding>> _runtime = new();


    // 枚举放这里，UI 和 Patch 共用
    public enum RemapAction
    {
        Pause,
        Restart
    }

    public InputMapManager(PluginConfig cfg) => _config = cfg;

    public void Initialize()
    {
        Plugin.Log.Info("Initializing KeyRemapper");
        BuildRuntimeCache();
        RefreshDevices();
        Plugin.Log.Info("Initialized KeyRemapper");
    }

    public void Dispose() => _deviceCache.Clear();

    /// <summary>外部调用：判断指定动作是否被按下（边沿检测由调用方完成）</summary>
    public bool GetActionState(RemapAction act)
    {
        foreach (var bind in _runtime[act.ToString()])
        {
            if (!TryGetDevice(bind, out var dev)) continue;
            if (dev.TryGetFeatureValue(bind.Feature, out var v) && v)
                return true;
        }

        return false;
    }


    #region public‑API for UI

    // public IReadOnlyList<ButtonBinding> GetBindings(RemapAction action) =>
    //     _config.Bindings[action.ToString()];
    //
    // public void AddBinding(RemapAction action, ButtonBinding newBind)
    // {
    //     var list = _config.Bindings[action.ToString()];
    //     if (!list.Contains(newBind))
    //         list.Add(newBind);
    //     _config.Changed();
    // }

    public void RemoveBinding(RemapAction action, ButtonBinding bind)
    {
        // _config.Bindings[action.ToString()].Remove(bind);
        // _config.Changed();
    }

    public void SetDisableBuiltIn(bool disabled)
    {
        // _config.DisableBuiltInPause = disabled;
        // _config.Changed();
    }

    #endregion

    // ---------- helpers ----------

    private bool TryGetDevice(ButtonBinding bind, out InputDevice dev)
    {
        if (!_deviceCache.TryGetValue(bind.DeviceKey, out dev) || !dev.isValid)
        {
            dev = FindDevice(bind);
            if (dev.isValid) _deviceCache[bind.DeviceKey] = dev;
        }

        return dev.isValid;
    }

    private static InputDevice FindDevice(ButtonBinding bind)
    {
        var list = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(bind.Characteristics, list);
        // 简化：取第一个
        return list.FirstOrDefault();
    }

    private void BuildRuntimeCache()
    {
        _runtime.Clear();

        foreach (var (name, setting) in _config.Actions)
        {
            var list = new List<ButtonBinding>();

            foreach (var token in setting.Bindings)
            {
                Plugin.Log.Info($"{name}: {token}");
                // 只解析 Bindings；BuiltInKeys 由 Patch 拦截逻辑处理
                if (ButtonCatalog.TryGet(token, out var bind))
                {
                    list.Add(bind);
                    Plugin.Log.Info($"try get success {name}: {token}");
                }
            }

            _runtime[name] = list;
        }
    }

    private bool IsBuiltInPressed()
    {
        var listLeft = new List<InputDevice>();
        var listRight = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller, listLeft);
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, listRight);
        return listLeft.Concat(listRight).Any(d =>
            d.TryGetFeatureValue(CommonUsages.menuButton, out var v) && v);
    }

    /// <summary>
    /// 清空并重新扫描所有已配置绑定对应的 <see cref="InputDevice"/>。
    /// 调用时机：Initialize()、配置更新后、或收到设备热插拔事件。
    /// </summary>
    public void RefreshDevices()
    {
        Plugin.Log.Info("Refreshing device cache…");
        _deviceCache.Clear();

        // 1. 预先拉取左右手柄列表（可减少后面重复扫描）
        var leftCandidates  = new List<InputDevice>();
        var rightCandidates = new List<InputDevice>();

        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Left  | InputDeviceCharacteristics.Controller,
            leftCandidates);

        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller,
            rightCandidates);

        Plugin.Log.Debug($"Found {leftCandidates.Count} left & {rightCandidates.Count} right controllers.");

        // 2. 遍历所有已解析的 ButtonBinding
        foreach (var (action, binds) in _runtime)
        {
            foreach (var bind in binds)
            {
                // 已缓存则跳过
                if (_deviceCache.ContainsKey(bind.DeviceKey))
                    continue;

                InputDevice dev = default;

                // 2‑1 先从缓存好的列表里找
                if ((bind.Characteristics & InputDeviceCharacteristics.Left) != 0)
                    dev = leftCandidates.FirstOrDefault(d => d.isValid);
                else if ((bind.Characteristics & InputDeviceCharacteristics.Right) != 0)
                    dev = rightCandidates.FirstOrDefault(d => d.isValid);

                // 2‑2 如果没找到，再做一次精确查询（保险）
                if (!dev.isValid)
                    dev = FindDevice(bind);

                // 2‑3 结果写入缓存
                if (dev.isValid)
                {
                    _deviceCache[bind.DeviceKey] = dev;
                    Plugin.Log.Debug($"[{action}] cached device '{dev.name}' for key {bind.DeviceKey}");
                }
                else
                {
                    Plugin.Log.Warn($"[{action}] no valid device for key {bind.DeviceKey} ({bind.Characteristics})");
                }
            }
        }
    }

}

/// <summary>可序列化的单个按钮描述</summary>
[Serializable]
public class ButtonBinding : IEquatable<ButtonBinding>
{
    public uint CharacteristicsRaw; // Serialized enum
    public string UsageName; // "primaryButton" ...

    [NonSerialized] public InputDeviceCharacteristics Characteristics;
    [NonSerialized] public InputFeatureUsage<bool> Feature;

    public string DeviceKey => CharacteristicsRaw.ToString(); // 用于缓存字典

    public ButtonBinding()
    {
    } // for JSON

    public ButtonBinding(InputDeviceCharacteristics c, InputFeatureUsage<bool> f)
    {
        CharacteristicsRaw = (uint)c;
        UsageName = f.name;
        Characteristics = c;
        Feature = f;
    }

    public void ResolveRuntime()
    {
        Characteristics = (InputDeviceCharacteristics)CharacteristicsRaw;
        Feature = new InputFeatureUsage<bool>(UsageName);
    }

    public bool Equals(ButtonBinding other) =>
        other != null && CharacteristicsRaw == other.CharacteristicsRaw && UsageName == other.UsageName;

    public override int GetHashCode() => HashCode.Combine(CharacteristicsRaw, UsageName);
    public override string ToString() => $"{Characteristics}.{UsageName}";
}

internal static class ButtonCatalog
{
    private static readonly Dictionary<string, ButtonBinding> _map = new()
    {
        ["L_X"] = New(L, CommonUsages.primaryButton),
        ["L_Y"] = New(L, CommonUsages.secondaryButton),
        ["R_A"] = New(R, CommonUsages.primaryButton),
        ["R_B"] = New(R, CommonUsages.secondaryButton),
        ["L_Grip"] = New(L, CommonUsages.gripButton),
        ["R_Grip"] = New(R, CommonUsages.gripButton),
        ["L_Trigger"] = New(L, CommonUsages.triggerButton),
        ["R_Trigger"] = New(R, CommonUsages.triggerButton),
        ["L_Stick"] = New(L, CommonUsages.primary2DAxisClick),
        ["R_Stick"] = New(R, CommonUsages.primary2DAxisClick),
        ["L_Menu"] = New(L, CommonUsages.menuButton),
        ["R_Menu"] = New(R, CommonUsages.menuButton),
        ["Menu"] = New(L | R, CommonUsages.menuButton) // 两手共用
    };

    private const InputDeviceCharacteristics
        L = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;

    private const InputDeviceCharacteristics R = InputDeviceCharacteristics.Right |
                                                 InputDeviceCharacteristics.Controller;

    private static ButtonBinding New(InputDeviceCharacteristics c, InputFeatureUsage<bool> f)
        => new(c, f);

    public static bool TryGet(string token, out ButtonBinding bind) => _map.TryGetValue(token, out bind);
}