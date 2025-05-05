using System;
using System.Collections.Generic;
using KeyRemapper.Configuration;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.XR;
using Zenject;

namespace KeyRemapper.InputManagers;

/// <summary>
/// 实现 IInputManager 接口，在当前运行时是Unity OpenXR时使用。
/// </summary>
internal class UnityXRInputManager : IInputManager, IInitializable, IDisposable, ITickable
{
    [Inject]
    private readonly SiraLog _logger = null!;

    public event Action<ControllerButton>? ButtonPressed;

    private readonly Dictionary<ControllerButton, bool> _buttonStates = new()
    {
        { ControllerButton.L_X, false },
        { ControllerButton.L_Y, false },
        { ControllerButton.L_Trigger, false },
        { ControllerButton.L_Grip, false },
        { ControllerButton.L_Stick, false },
        { ControllerButton.L_Menu, false },
        { ControllerButton.R_A, false },
        { ControllerButton.R_B, false },
        { ControllerButton.R_Trigger, false },
        { ControllerButton.R_Grip, false },
        { ControllerButton.R_Stick, false },
        { ControllerButton.R_Menu, false }
    };

    private InputDevice _left;
    private InputDevice _right;

    void IInitializable.Initialize()
    {
        // UnityXRInputManager 初始化
        _logger.Trace("UnityXRInputManager initialize");
        InputTracking.nodeAdded += OnNodeAdded;
        InputTracking.nodeRemoved += OnNodeRemoved;

        RefreshControllers();
    }

    void IDisposable.Dispose()
    {
        _logger.Trace("UnityXRInputManager dispose");
        InputTracking.nodeAdded -= OnNodeAdded;
        InputTracking.nodeRemoved -= OnNodeRemoved;
    }

    void ITickable.Tick()
    {
        bool value;
        
        // 依次检测每个手柄的每个按钮
        // 虽然这样写很重复，但避免了动态内存分配，并且常数时间复杂度
        if (_left.isValid)
        { 
            if (_left.TryGetFeatureValue(CommonUsages.primaryButton, out value))
            {
                HandleButtonStateThisFrame(ControllerButton.L_X, value);
            }

            if (_left.TryGetFeatureValue(CommonUsages.secondaryButton, out value))
            {
                HandleButtonStateThisFrame(ControllerButton.L_Y, value);
            }

            if (_left.TryGetFeatureValue(CommonUsages.gripButton, out value))
            {
                HandleButtonStateThisFrame(ControllerButton.L_Grip, value);
            }

            if (_left.TryGetFeatureValue(CommonUsages.triggerButton, out value))
            {
                HandleButtonStateThisFrame(ControllerButton.L_Trigger, value);
            }

            if (_left.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out value))
            {
                HandleButtonStateThisFrame(ControllerButton.L_Stick, value);
            }

            if (_left.TryGetFeatureValue(CommonUsages.menuButton, out value))
            {
                HandleButtonStateThisFrame(ControllerButton.L_Menu, value);
            }
        }

        if (_right.isValid)
        {
            if (_right.TryGetFeatureValue(CommonUsages.primaryButton, out value))
            {
                HandleButtonStateThisFrame(ControllerButton.R_A, value);
            }

            if (_right.TryGetFeatureValue(CommonUsages.secondaryButton, out value))
            {
                HandleButtonStateThisFrame(ControllerButton.R_B, value);
            }

            if (_right.TryGetFeatureValue(CommonUsages.gripButton, out value))
            {
                HandleButtonStateThisFrame(ControllerButton.R_Grip, value);
            }

            if (_right.TryGetFeatureValue(CommonUsages.triggerButton, out value))
            {
                HandleButtonStateThisFrame(ControllerButton.R_Trigger, value);
            }

            if (_right.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out value))
            {
                HandleButtonStateThisFrame(ControllerButton.R_Stick, value);
            }

            if (_right.TryGetFeatureValue(CommonUsages.menuButton, out value))
            {
                HandleButtonStateThisFrame(ControllerButton.R_Menu, value);
            }
        }
    }

    public bool IsButtonPressedRightNow(ControllerButton button)
    {
        // 检测当前按钮是否被按下
        // 不需要检测是否Valid，因为内部会自动检测
        bool value;
        return button switch
        {
            ControllerButton.L_X => _left.TryGetFeatureValue(CommonUsages.primaryButton, out value) && value,
            ControllerButton.L_Y => _left.TryGetFeatureValue(CommonUsages.secondaryButton, out value) && value,
            ControllerButton.L_Grip => _left.TryGetFeatureValue(CommonUsages.gripButton, out value) && value,
            ControllerButton.L_Trigger => _left.TryGetFeatureValue(CommonUsages.triggerButton, out value) && value,
            ControllerButton.L_Stick => _left.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out value) && value,
            ControllerButton.L_Menu => _left.TryGetFeatureValue(CommonUsages.menuButton, out value) && value,
            ControllerButton.R_A => _right.TryGetFeatureValue(CommonUsages.primaryButton, out value) && value,
            ControllerButton.R_B => _right.TryGetFeatureValue(CommonUsages.secondaryButton, out value) && value,
            ControllerButton.R_Grip => _right.TryGetFeatureValue(CommonUsages.gripButton, out value) && value,
            ControllerButton.R_Trigger => _right.TryGetFeatureValue(CommonUsages.triggerButton, out value) && value,
            ControllerButton.R_Stick => _right.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out value) && value,
            ControllerButton.R_Menu => _right.TryGetFeatureValue(CommonUsages.menuButton, out value) && value,
            _ => false
        };
    }

    public bool IsButtonPressed(ControllerButton button)
    {
        return _buttonStates[button];
    }

    private void OnNodeAdded(XRNodeState nodeState)
    {
        if (nodeState.nodeType != XRNode.LeftHand && nodeState.nodeType != XRNode.RightHand) return;

        _logger.Info("Controller added, refreshing controllers.");
        RefreshControllers();
    }

    private void OnNodeRemoved(XRNodeState nodeState)
    {
        if (nodeState.nodeType != XRNode.LeftHand && nodeState.nodeType != XRNode.RightHand) return;

        _logger.Info("Controller removed, refreshing controllers.");
        RefreshControllers();
    }

    private void RefreshControllers()
    {
        _logger.Debug("Refreshing controllers.");
        // 刷新左右手控制器
        _left = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        _right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (_left.isValid)
        {
            _logger.Debug("Left controller found.");
        }
        else
        {
            _logger.Warn("Left controller not found.");
        }

        if (_right.isValid)
        {
            _logger.Debug("Right controller found.");
        }
        else
        {
            _logger.Warn("Right controller not found.");
        }
    }

    private void HandleButtonStateThisFrame(ControllerButton button, bool isPressed)
    {
        if (_buttonStates[button] == isPressed) return;

        // 按钮状态发生变化，触发事件
        _buttonStates[button] = isPressed;
        if (isPressed)
        {
            try
            {
                // 无需赋值变量并检查再调用，所有相关逻辑都应在主线程上运行
                ButtonPressed?.Invoke(button);
            }
            catch (Exception e)
            {
                _logger.Error("Error invoking ButtonPressed event");
                _logger.Error(e);
            }
        }
    }
}