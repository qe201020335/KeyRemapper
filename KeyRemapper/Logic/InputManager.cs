using System;
using UnityEngine.XR;

namespace KeyRemapper.Logic;

// TODO: Delete this file after refactor UI to use the new IInputManager
[Obsolete]
internal class InputMapManager
{
    // 枚举放这里，UI 和 Patch 共用
    public enum RemapAction
    {
        Pause,
        Restart
    }

    public void RemoveBinding(RemapAction action, ButtonBinding bind)
    {
    }

    public void SetDisableBuiltIn(bool disabled)
    {
    }
}

[Obsolete]
public class ButtonBinding
{
    public ButtonBinding(InputDeviceCharacteristics c, InputFeatureUsage<bool> f)
    {
    }
}
