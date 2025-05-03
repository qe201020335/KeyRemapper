using System;
using KeyRemapper.Configuration;

namespace KeyRemapper.InputManagers;

/// <summary>
/// 在不支持当前运行时的情况下使用的管理器。什么都不做。
/// 目前只支持 OpenXR，其他运行时都使用这个管理器。
/// </summary>
internal class DummyInputManager : IInputManager
{
    public event Action<ControllerButton>? ButtonPressed;
}