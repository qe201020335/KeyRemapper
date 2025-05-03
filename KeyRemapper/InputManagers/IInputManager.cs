using System;
using KeyRemapper.Configuration;

namespace KeyRemapper.InputManagers;

/// <summary>
/// 输入管理器抽象接口，负责检测按键状态并触发事件。
/// </summary>
public interface IInputManager
{
    event Action<ControllerButton> ButtonPressed;
}