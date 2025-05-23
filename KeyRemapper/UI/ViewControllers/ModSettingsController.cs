using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using KeyRemapper.Configuration;
using SiraUtil.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace KeyRemapper.UI.ViewControllers;

[HotReload(RelativePathToLayout = @"..\Views\keyRemapperSettings.bsml")]
[ViewDefinition("KeyRemapper.UI.Views.keyRemapperSettings.bsml")]
internal class ModSettingsController : BSMLAutomaticViewController
{
    [Inject] private readonly PluginConfig _config;
    [Inject] private readonly SiraLog _logger;

    // UI组件引用（将在后续实现中使用）
    [UIComponent("pause-bindings-container")] 
    private Transform pauseBindingsContainer;
    
    [UIComponent("restart-bindings-container")] 
    private Transform restartBindingsContainer;

    // 数据列表
    private readonly List<BindingRowData> pauseBindingRows = new();
    private readonly List<BindingRowData> restartBindingRows = new();

    #region 数据结构定义

    // 绑定行数据包装类
    private class BindingRowData
    {
        public ControllerButton Button { get; set; }
        public GameObject UIObject { get; set; }
        public Transform Container { get; set; }
        public BindingRowController Controller { get; set; }
    }

    // 用于BSMLParser的数据上下文类
    private class BindingRowController
    {
        [UIValue("button-options")]
        public List<object> ButtonOptions { get; set; } = new();
        
        [UIValue("selected-button")]
        public string SelectedButton { get; set; } = "";
        
        [UIAction("button-changed")]
        public void OnButtonChanged(string newValue)
        {
            ButtonChangedCallback?.Invoke(newValue);
        }
        
        [UIAction("delete-clicked")]
        public void OnDeleteClicked()
        {
            DeleteClickedCallback?.Invoke();
        }
        
        public Action<string> ButtonChangedCallback { get; set; }
        public Action DeleteClickedCallback { get; set; }
    }

    #endregion

    #region UI属性绑定

    [UIValue("pause-enabled")]
    public bool PauseEnabled
    {
        get => _config.Actions.Pause.Enabled;
        set
        {
            _config.Actions.Pause.Enabled = value;
            NotifyPropertyChanged();
        }
    }

    [UIValue("pause-block-builtin")]
    public bool PauseBlockBuiltIn
    {
        get => _config.Actions.Pause.BlockBuiltIn;
        set
        {
            _config.Actions.Pause.BlockBuiltIn = value;
            NotifyPropertyChanged();
        }
    }

    [UIValue("restart-enabled")]
    public bool RestartEnabled
    {
        get => _config.Actions.Restart.Enabled;
        set
        {
            _config.Actions.Restart.Enabled = value;
            NotifyPropertyChanged();
        }
    }

    #endregion

    #region 事件处理方法

    [UIAction("add-pause-binding")]
    private void AddPauseBinding()
    {
        _logger.Debug("AddPauseBinding clicked");
        
        // 找到第一个未使用的按键
        var availableButtons = GetAvailableButtons(pauseBindingRows);
        if (!availableButtons.Any())
        {
            _logger.Warn("No available buttons to add");
            return;
        }
        
        var firstAvailable = Enum.Parse<ControllerButton>((string)availableButtons.First());
        
        // 移除空状态文本
        RemoveEmptyStateText(pauseBindingsContainer);
        
        // 创建新行
        var newRow = CreateBindingRow(firstAvailable, pauseBindingsContainer, pauseBindingRows);
        if (newRow != null)
        {
            pauseBindingRows.Add(newRow);
            // 保存配置
            SaveConfig();
        }
        else
        {
            _logger.Error("Failed to create pause binding row");
            // 如果创建失败且列表为空，恢复空状态文本
            if (!pauseBindingRows.Any())
            {
                ShowEmptyStateText(pauseBindingsContainer);
            }
        }
    }

    [UIAction("add-restart-binding")]
    private void AddRestartBinding()
    {
        _logger.Debug("AddRestartBinding clicked");
        
        // 找到第一个未使用的按键
        var availableButtons = GetAvailableButtons(restartBindingRows);
        if (!availableButtons.Any())
        {
            _logger.Warn("No available buttons to add");
            return;
        }
        
        var firstAvailable = Enum.Parse<ControllerButton>((string)availableButtons.First());
        
        // 移除空状态文本
        RemoveEmptyStateText(restartBindingsContainer);
        
        // 创建新行
        var newRow = CreateBindingRow(firstAvailable, restartBindingsContainer, restartBindingRows);
        if (newRow != null)
        {
            restartBindingRows.Add(newRow);
            // 保存配置
            SaveConfig();
        }
        else
        {
            _logger.Error("Failed to create restart binding row");
            // 如果创建失败且列表为空，恢复空状态文本
            if (!restartBindingRows.Any())
            {
                ShowEmptyStateText(restartBindingsContainer);
            }
        }
    }

    #endregion

    #region 生命周期方法

    protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        _logger.Debug("ModSettingsController activated");
        base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
        
        // 每次激活时都加载配置，而不仅仅是第一次
        // 这样可以确保重新进入界面时绑定正确显示
        if (firstActivation || !pauseBindingRows.Any() && !restartBindingRows.Any())
        {
            _logger.Debug("Loading bindings from config");
            LoadFromConfig();
        }
    }

    protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
    {
        _logger.Debug("ModSettingsController deactivated");
        base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
        
        // 不再清理UI，保持绑定行的状态
        // 这样重新进入时UI仍然存在
        // ClearBindingRows(pauseBindingRows);
        // ClearBindingRows(restartBindingRows);
    }

    #endregion

    #region 辅助方法

    private const string BindingRowResourceName = "KeyRemapper.UI.Views.bindingRow.bsml";

    private BindingRowData CreateBindingRow(ControllerButton button, Transform parent, List<BindingRowData> bindingList)
    {
        _logger.Debug($"Creating binding row for button: {button}");
        
        // 创建数据上下文
        var controller = new BindingRowController
        {
            SelectedButton = button.ToString(),
            ButtonOptions = GetAvailableButtons(bindingList, button)
        };
        
        // 记录解析前的子对象数量
        var childCountBefore = parent.childCount;
        _logger.Debug($"Parent child count before: {childCountBefore}");
        
        // 使用BSMLParser解析BSML
        var parserParams = BSMLParser.Instance.Parse(
            Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), BindingRowResourceName),
            parent.gameObject,
            controller
        );
        
        _logger.Debug($"Parent child count after: {parent.childCount}");
        
        // 获取新创建的GameObject（最后一个子对象）
        GameObject uiObject = null;
        if (parent.childCount > childCountBefore)
        {
            uiObject = parent.GetChild(parent.childCount - 1).gameObject;
            _logger.Debug($"Created UI object: {uiObject.name}");
        }
        
        if (uiObject == null)
        {
            _logger.Error("Failed to create binding row UI");
            return null;
        }
        
        // 创建数据包装
        var rowData = new BindingRowData
        {
            Button = button,
            UIObject = uiObject,
            Container = parent,
            Controller = controller
        };
        
        // 设置回调
        controller.ButtonChangedCallback = (newValue) => {
            if (Enum.TryParse<ControllerButton>(newValue, out var newButton))
            {
                _logger.Debug($"Button changed from {rowData.Button} to {newButton}");
                rowData.Button = newButton;
                UpdateAllDropdowns(bindingList);
                SaveConfig();
            }
        };
        
        controller.DeleteClickedCallback = () => {
            _logger.Debug($"Delete button clicked for {rowData.Button}");
            DeleteBindingRow(rowData, bindingList);
        };
        
        return rowData;
    }

    // 获取可用按键列表
    private List<object> GetAvailableButtons(List<BindingRowData> currentBindings, ControllerButton? currentButton = null)
    {
        var usedButtons = currentBindings
            .Where(b => currentButton == null || b.Button != currentButton)
            .Select(b => b.Button)
            .ToHashSet();
        
        return Enum.GetValues(typeof(ControllerButton))
            .Cast<ControllerButton>()
            .Where(b => currentButton == b || !usedButtons.Contains(b))
            .Select(b => (object)b.ToString())
            .ToList();
    }

    // 更新所有下拉列表的可用选项
    private void UpdateAllDropdowns(List<BindingRowData> bindingList)
    {
        foreach (var row in bindingList)
        {
            row.Controller.ButtonOptions = GetAvailableButtons(bindingList, row.Button);
            // 触发UI更新
            NotifyPropertyChanged(nameof(row.Controller.ButtonOptions));
        }
    }

    // 删除绑定行
    private void DeleteBindingRow(BindingRowData row, List<BindingRowData> bindingList)
    {
        // 从列表移除
        bindingList.Remove(row);
        
        // 销毁UI对象
        if (row.UIObject != null)
            Destroy(row.UIObject);
        
        // 如果列表为空，显示空状态文本
        if (!bindingList.Any())
        {
            ShowEmptyStateText(row.Container);
        }
        
        // 更新其他行的可用选项
        UpdateAllDropdowns(bindingList);
        
        // 保存配置
        SaveConfig();
    }

    // 移除空状态文本
    private void RemoveEmptyStateText(Transform container)
    {
        // 查找名为"EmptyText"的子对象
        for (int i = 0; i < container.childCount; i++)
        {
            var child = container.GetChild(i);
            if (child.name == "EmptyText")
            {
                _logger.Debug("Removing empty state text");
                Destroy(child.gameObject);
                return;
            }
        }
        
        // 兼容旧方式 - 如果没有找到EmptyText，尝试找包含特定文本的对象
        var emptyText = container.GetComponentInChildren<TextMeshProUGUI>();
        if (emptyText != null && emptyText.text == "No key bindings added")
        {
            _logger.Debug("Removing empty state text (legacy)");
            Destroy(emptyText.gameObject);
        }
    }

    // 显示空状态文本
    private void ShowEmptyStateText(Transform container)
    {
        var textGO = new GameObject("EmptyText");
        textGO.transform.SetParent(container, false);
        
        var text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = "No key bindings added";
        text.fontSize = 3.5f;
        text.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        text.alignment = TextAlignmentOptions.Center;
        
        var rectTransform = textGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
    }

    // 清空绑定行
    private void ClearBindingRows(List<BindingRowData> bindingList)
    {
        foreach (var row in bindingList)
        {
            if (row.UIObject != null)
                Destroy(row.UIObject);
        }
        bindingList.Clear();
    }

    // 从配置加载
    private void LoadFromConfig()
    {
        _logger.Debug("Loading bindings from config");
        
        // 检查容器引用
        if (pauseBindingsContainer == null)
        {
            _logger.Error("pauseBindingsContainer is null!");
            return;
        }
        if (restartBindingsContainer == null)
        {
            _logger.Error("restartBindingsContainer is null!");
            return;
        }
        
        _logger.Debug($"pauseBindingsContainer: {pauseBindingsContainer}");
        _logger.Debug($"restartBindingsContainer: {restartBindingsContainer}");
        
        // 清空现有UI
        ClearBindingRows(pauseBindingRows);
        ClearBindingRows(restartBindingRows);
        
        // 加载Pause绑定
        if (_config.Actions.Pause.Bindings.Any())
        {
            RemoveEmptyStateText(pauseBindingsContainer);
            foreach (var button in _config.Actions.Pause.Bindings)
            {
                var row = CreateBindingRow(button, pauseBindingsContainer, pauseBindingRows);
                if (row != null)
                {
                    pauseBindingRows.Add(row);
                }
                else
                {
                    _logger.Error($"Failed to create pause binding row for button: {button}");
                }
            }
            _logger.Debug($"Loaded {pauseBindingRows.Count} pause bindings");
            
            // 如果所有绑定都创建失败，显示空状态文本
            if (!pauseBindingRows.Any())
            {
                ShowEmptyStateText(pauseBindingsContainer);
            }
        }
        
        // 加载Restart绑定
        if (_config.Actions.Restart.Bindings.Any())
        {
            RemoveEmptyStateText(restartBindingsContainer);
            foreach (var button in _config.Actions.Restart.Bindings)
            {
                var row = CreateBindingRow(button, restartBindingsContainer, restartBindingRows);
                if (row != null)
                {
                    restartBindingRows.Add(row);
                }
                else
                {
                    _logger.Error($"Failed to create restart binding row for button: {button}");
                }
            }
            _logger.Debug($"Loaded {restartBindingRows.Count} restart bindings");
            
            // 如果所有绑定都创建失败，显示空状态文本
            if (!restartBindingRows.Any())
            {
                ShowEmptyStateText(restartBindingsContainer);
            }
        }
    }

    // 保存配置
    private void SaveConfig()
    {
        _logger.Debug("Saving bindings to config");
        
        // 保存Pause绑定
        _config.Actions.Pause.SetBindings(pauseBindingRows.Select(r => r.Button));
        
        // 保存Restart绑定
        _config.Actions.Restart.SetBindings(restartBindingRows.Select(r => r.Button));
        
        _logger.Debug("Config saved");
    }

    // 按键选项列表（供后续下拉列表使用）
    private static readonly List<object> ButtonOptionsCache = 
        Enum.GetNames(typeof(ControllerButton))
            .Select(x => (object)x)
            .ToList();

    #endregion
}
