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
    // 多动作字典：action 名字 → 列表
    [UseConverter(typeof(DictionaryConverter<List<ButtonBinding>>))]
    public Dictionary<string, List<ButtonBinding>> Bindings { get; set; }
        = new()
        {
            { RemapAction.Pause.ToString(),      new List<ButtonBinding>() },
            { RemapAction.Restart.ToString(),    new List<ButtonBinding>() },
            { RemapAction.Screenshot.ToString(), new List<ButtonBinding>() }
        };

    public bool DisableBuiltInPause { get; set; } = false;

    public static PluginConfig Instance { get; set; }

    #region Hooks

    public virtual void OnReload()
    {
    } // BSIPA 回调

    public virtual void Changed()
    {
    } // 保存时调用

    public virtual void CopyFrom(PluginConfig other)
    {
        Bindings = JsonConvert.DeserializeObject<Dictionary<string, List<ButtonBinding>>>(
            JsonConvert.SerializeObject(other.Bindings)); // 深拷贝
        DisableBuiltInPause = other.DisableBuiltInPause;
    }

    #endregion
}