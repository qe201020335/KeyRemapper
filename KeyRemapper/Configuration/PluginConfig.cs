// Configuration/PluginConfig.cs

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using KeyRemapper.Logic;
using Newtonsoft.Json;
using UnityEngine.XR;
using static KeyRemapper.Logic.InputMapManager;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace KeyRemapper.Configuration;

public class PluginConfig
{
    public static PluginConfig Instance { get; set; }

    public int Version { get; set; } = 2;

    [UseConverter(typeof(DictionaryConverter<PluginConfig.ActionSetting>))]
    public Dictionary<string, ActionSetting> Actions { get; set; }
        = ActionSetting.BuildDefault();

    public class ActionSetting
    {
        [UseConverter(typeof(ListConverter<string>))]
        public List<string> Bindings { get; set; } = new();

        [UseConverter(typeof(ListConverter<string>))]
        public List<string> BuiltInKeys { get; set; } = new();

        public bool BlockBuiltIn { get; set; } = false;

        // 默认模板
        public static Dictionary<string, ActionSetting> BuildDefault()
            => new()
            {
                {
                    RemapAction.Pause.ToString(),
                    new ActionSetting
                    {
                        Bindings = new() { },
                        BuiltInKeys = new() { },
                        BlockBuiltIn = false
                    }
                },
                {
                    RemapAction.Restart.ToString(),
                    new ActionSetting
                    {
                        Bindings = new() { },
                        BuiltInKeys = new() { },
                        BlockBuiltIn = false
                    }
                }
            };
    }


    #region Hooks  ---------------------------

    // BSIPA 在插件启动后读取 cfg 时调用
    public virtual void OnReload()
    {
        if (Actions == null || Actions.Count == 0)
            Actions = ActionSetting.BuildDefault();
        foreach (var kv in Actions)
        {
            kv.Value.Bindings ??= new List<string>();
            kv.Value.BuiltInKeys ??= new List<string>();
        }
    }

    // 当你手动调用 config.Save() 时被触发
    public virtual void Changed()
    {
    }

    // 深拷贝：用于 BSIPA 的热重载和 Undo
    public virtual void CopyFrom(PluginConfig other)
    {
        Version = other.Version;

        // 深拷贝 Actions（Json 仍是最省事的办法）
        Actions = JsonConvert.DeserializeObject<Dictionary<string, ActionSetting>>(
            JsonConvert.SerializeObject(other.Actions));
    }

    #endregion
}