using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace KeyRemapper.Configuration;

internal class PluginConfig
{
    public static PluginConfig Instance { get; set; }

    public virtual int Version { get; protected set; } = 2;

    public virtual ActionSettings Actions { get; protected set; } = new();
}

internal class ActionSettings
{
    public virtual ActionBinding Pause { get; protected set; } = new();

    public virtual ActionBinding Restart { get; protected set; } = new();

    public void Reset()
    {
        Pause = new ActionBinding();
        Restart = new ActionBinding();
    }
}

internal class ActionBinding
{
    [UseConverter(typeof(ListConverter<string>))]
    [SerializedName("Bindings")]
    protected virtual List<string> BindingsInternal { get; set; } = [];

    public virtual bool BlockBuiltIn { get; set; } = false;

    public IReadOnlyList<string> Bindings => BindingsInternal.AsReadOnly();

    public void AddBinding(string binding)
    {
        BindingsInternal.Add(binding);
        Changed(); // 触发保存
    }

    public void RemoveBinding(string binding)
    {
        BindingsInternal.Remove(binding);
        Changed(); // 触发保存
    }

    protected virtual void Changed()
    {
        // BSIPA 会重写此方法，调用时会触发保存
    }
}