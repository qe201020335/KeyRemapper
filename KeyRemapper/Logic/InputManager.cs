using System;
using System.Collections.Generic;
using System.Linq;
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

    // 枚举放这里，UI 和 Patch 共用
    public enum RemapAction
    {
        Pause,
        Restart,
        Screenshot
    }

    public InputMapManager(PluginConfig cfg) => _config = cfg;

    public void Initialize()
    {
        RefreshDevices();
    }
    public void Dispose() => _deviceCache.Clear();

    /// <summary>外部调用：判断指定动作是否被按下（边沿检测由调用方完成）</summary>
    public bool GetActionState(RemapAction act)
    {
        if (_config.DisableBuiltInPause && act == RemapAction.Pause && IsBuiltInPressed())
            return false; // 官方键被禁用

        foreach (var bind in _config.Bindings[act.ToString()])
        {
            if (!_deviceCache.TryGetValue(bind.DeviceKey, out var dev) || !dev.isValid)
                dev = _deviceCache[bind.DeviceKey] = FindDevice(bind);

            if (dev.isValid && dev.TryGetFeatureValue(bind.Feature, out var v) && v)
                return true;
        }

        return false;
    }

    #region public‑API for UI

    public IReadOnlyList<ButtonBinding> GetBindings(RemapAction action) =>
        _config.Bindings[action.ToString()];

    public void AddBinding(RemapAction action, ButtonBinding newBind)
    {
        var list = _config.Bindings[action.ToString()];
        if (!list.Contains(newBind))
            list.Add(newBind);
        _config.Changed();
    }

    public void RemoveBinding(RemapAction action, ButtonBinding bind)
    {
        _config.Bindings[action.ToString()].Remove(bind);
        _config.Changed();
    }

    public void SetDisableBuiltIn(bool disabled)
    {
        _config.DisableBuiltInPause = disabled;
        _config.Changed();
    }

    #endregion

    // ---------- helpers ----------
    private static InputDevice FindDevice(ButtonBinding bind)
    {
        var list = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(bind.Characteristics, list);
        // 简化：取第一个
        return list.FirstOrDefault();
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
    /// 清空并重新扫描所有已配置绑定对应的 InputDevice。
    /// 调用时机：Initialize()、配置更新后、设备热插拔（可在 UI 中手动触发）。
    /// </summary>
    public void RefreshDevices()
    {
        _deviceCache.Clear();

        foreach (var kv in _config.Bindings)
        {
            foreach (var bind in kv.Value)
            {
                bind.ResolveRuntime();                           // 还原运行时字段
                string key = bind.DeviceKey;

                if (_deviceCache.ContainsKey(key))
                    continue;   // 同一手柄只缓存一次

                var found = new List<InputDevice>();
                InputDevices.GetDevicesWithCharacteristics(
                    bind.Characteristics | InputDeviceCharacteristics.Controller, found);

                if (found.Any())
                    _deviceCache[key] = found[0];
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