using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
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
using KeyRemapper.UI.helpers;

namespace KeyRemapper.UI.ViewControllers;

[HotReload(RelativePathToLayout = @"..\Views\keyRemapperSettings.bsml")]
[ViewDefinition("KeyRemapper.UI.Views.keyRemapperSettings.bsml")]
internal class ModSettingsController : BSMLAutomaticViewController, TableView.IDataSource, INotifyPropertyChanged
{
    [Inject] private readonly PluginConfig _config;
    [Inject] private readonly SiraLog _logger;

    // UI组件引用
    [UIComponent("pauseBindingsList")] 
    private CustomListTableData pauseBindingsList;
    
    [UIComponent("restartBindingsList")] 
    private CustomListTableData restartBindingsList;
    
    [UIComponent("pauseScrollBarContainer")]
    private Transform pauseScrollBarContainer;
    
    [UIComponent("restartScrollBarContainer")]
    private Transform restartScrollBarContainer;
    
    [UIComponent("pauseEmptyBindingsText")]
    private TextMeshProUGUI pauseEmptyBindingsText;
    
    [UIComponent("restartEmptyBindingsText")]
    private TextMeshProUGUI restartEmptyBindingsText;

    // 数据列表
    private readonly List<ControllerButton> pauseBindings = new();
    private readonly List<ControllerButton> restartBindings = new();
    
    // 当前显示的列表类型
    private enum ListType { None, Pause, Restart }
    private ListType currentListType = ListType.None;

    #region INotifyPropertyChanged 实现
    
    public event PropertyChangedEventHandler PropertyChanged;
    
    protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    #endregion

    #region TableView.IDataSource 实现

    public float CellSize(int idx) => 8f;

    public int NumberOfCells()
    {
        return currentListType switch
        {
            ListType.Pause => pauseBindings.Count,
            ListType.Restart => restartBindings.Count,
            _ => 0
        };
    }

    public TableCell CellForIdx(TableView tableView, int idx)
    {
        var cell = BindingTableData.GetCell(tableView);
        
        ControllerButton button;
        List<ControllerButton> bindingList;
        
        if (currentListType == ListType.Pause)
        {
            button = pauseBindings[idx];
            bindingList = pauseBindings;
        }
        else if (currentListType == ListType.Restart)
        {
            button = restartBindings[idx];
            bindingList = restartBindings;
        }
        else
        {
            _logger.Warn($"CellForIdx called with invalid ListType: {currentListType}");
            return cell;
        }
        
        _logger.Debug($"Creating cell for {currentListType} at index {idx}, button: {button}");
        
        cell.PopulateWithButton(
            button,
            (c) => OnDeleteBinding(c, button, bindingList),
            (c) => OnButtonClick(c, bindingList)
        );
        
        return cell;
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
        
        var availableButtons = GetAvailableButtons(pauseBindings);
        if (!availableButtons.Any())
        {
            _logger.Warn("No available buttons to add");
            return;
        }
        
        var firstAvailable = Enum.Parse<ControllerButton>((string)availableButtons.First());
        pauseBindings.Add(firstAvailable);
        
        UpdateUI(ListType.Pause);
        SaveConfig();
    }

    [UIAction("add-restart-binding")]
    private void AddRestartBinding()
    {
        _logger.Debug("AddRestartBinding clicked");
        
        var availableButtons = GetAvailableButtons(restartBindings);
        if (!availableButtons.Any())
        {
            _logger.Warn("No available buttons to add");
            return;
        }
        
        var firstAvailable = Enum.Parse<ControllerButton>((string)availableButtons.First());
        restartBindings.Add(firstAvailable);
        
        UpdateUI(ListType.Restart);
        SaveConfig();
    }

    #endregion

    #region 生命周期方法

    [UIAction("#post-parse")]
    private void Parsed()
    {
        _logger.Debug("ModSettingsController parsed");
        
        // 设置Pause列表数据源
        if (pauseBindingsList != null)
        {
            currentListType = ListType.Pause;
            pauseBindingsList.TableView.SetDataSource(this, false);
            _logger.Debug($"Pause list data source set, tableView: {pauseBindingsList.TableView}");
            
            // 添加滚动条
            if (pauseScrollBarContainer != null)
            {
                ScrollbarHelper.AddScrollbar(pauseBindingsList.TableView.gameObject, pauseScrollBarContainer);
                _logger.Debug("Pause scrollbar added");
            }
        }
        else
        {
            _logger.Error("pauseBindingsList is null!");
        }
        
        // 设置Restart列表数据源
        if (restartBindingsList != null)
        {
            currentListType = ListType.Restart;
            restartBindingsList.TableView.SetDataSource(this, false);
            _logger.Debug($"Restart list data source set, tableView: {restartBindingsList.TableView}");
            
            // 添加滚动条
            if (restartScrollBarContainer != null)
            {
                ScrollbarHelper.AddScrollbar(restartBindingsList.TableView.gameObject, restartScrollBarContainer);
                _logger.Debug("Restart scrollbar added");
            }
        }
        else
        {
            _logger.Error("restartBindingsList is null!");
        }
        
        currentListType = ListType.None;
        
        // 加载配置
        LoadFromConfig();
    }

    protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        _logger.Debug("ModSettingsController activated");
        base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
    }

    protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
    {
        _logger.Debug("ModSettingsController deactivated");
        base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
    }

    #endregion

    #region 辅助方法

    private void OnDeleteBinding(BindingTableCell cell, ControllerButton buttonToDelete, List<ControllerButton> bindingList)
    {
        var index = bindingList.IndexOf(buttonToDelete);
        
        if (index >= 0)
        {
            bindingList.RemoveAt(index);
            UpdateUI(bindingList == pauseBindings ? ListType.Pause : ListType.Restart);
            SaveConfig();
        }
    }
    
    private void OnButtonClick(BindingTableCell cell, List<ControllerButton> bindingList)
    {
        _logger.Debug($"Button clicked for {cell.CurrentButton}");
        
        // 获取当前按钮的索引
        var currentIndex = bindingList.IndexOf(cell.CurrentButton);
        if (currentIndex < 0) return;
        
        // 获取所有可用的按钮（排除已使用的）
        var availableButtons = GetAvailableButtons(bindingList, cell.CurrentButton);
        
        if (availableButtons.Count <= 1)
        {
            _logger.Warn("No other buttons available to switch to");
            return;
        }
        
        // 获取当前按钮在可用列表中的位置
        var currentButtonStr = cell.CurrentButton.ToString();
        var availableIndex = availableButtons.FindIndex(b => b.ToString() == currentButtonStr);
        
        // 循环到下一个可用按钮
        var nextIndex = (availableIndex + 1) % availableButtons.Count;
        var nextButtonStr = availableButtons[nextIndex].ToString();
        
        if (Enum.TryParse<ControllerButton>(nextButtonStr, out var nextButton))
        {
            // 更新绑定列表
            bindingList[currentIndex] = nextButton;
            
            // 更新UI
            UpdateUI(bindingList == pauseBindings ? ListType.Pause : ListType.Restart);
            
            // 保存配置
            SaveConfig();
            
            _logger.Debug($"Changed button from {cell.CurrentButton} to {nextButton}");
        }
    }

    // 获取可用按键列表
    private List<object> GetAvailableButtons(List<ControllerButton> currentBindings, ControllerButton? currentButton = null)
    {
        var usedButtons = currentBindings
            .Where(b => currentButton == null || b != currentButton)
            .ToHashSet();
        
        return Enum.GetValues(typeof(ControllerButton))
            .Cast<ControllerButton>()
            .Where(b => currentButton == b || !usedButtons.Contains(b))
            .Select(b => (object)b.ToString())
            .ToList();
    }

    // 更新UI
    private void UpdateUI(ListType listType)
    {
        currentListType = listType;
        
        if (listType == ListType.Pause && pauseBindingsList != null)
        {
            pauseBindingsList.TableView.ReloadData();
            pauseEmptyBindingsText.gameObject.SetActive(pauseBindings.Count == 0);
            pauseBindingsList.gameObject.SetActive(pauseBindings.Count > 0);
            
            // 如果添加了新项，滚动到底部
            if (pauseBindings.Count > 0)
            {
                pauseBindingsList.TableView.ScrollToCellWithIdx(pauseBindings.Count - 1, TableView.ScrollPositionType.End, false);
            }
            
            _logger.Debug($"Updated Pause UI - Count: {pauseBindings.Count}, List visible: {pauseBindings.Count > 0}");
        }
        else if (listType == ListType.Restart && restartBindingsList != null)
        {
            restartBindingsList.TableView.ReloadData();
            restartEmptyBindingsText.gameObject.SetActive(restartBindings.Count == 0);
            restartBindingsList.gameObject.SetActive(restartBindings.Count > 0);
            
            // 如果添加了新项，滚动到底部
            if (restartBindings.Count > 0)
            {
                restartBindingsList.TableView.ScrollToCellWithIdx(restartBindings.Count - 1, TableView.ScrollPositionType.End, false);
            }
            
            _logger.Debug($"Updated Restart UI - Count: {restartBindings.Count}, List visible: {restartBindings.Count > 0}");
        }
        
        currentListType = ListType.None;
    }

    // 从配置加载
    private void LoadFromConfig()
    {
        _logger.Debug("Loading bindings from config");
        
        // 清空现有数据
        pauseBindings.Clear();
        restartBindings.Clear();
        
        // 加载Pause绑定
        pauseBindings.AddRange(_config.Actions.Pause.Bindings);
        _logger.Debug($"Loaded {pauseBindings.Count} pause bindings");
        
        // 加载Restart绑定
        restartBindings.AddRange(_config.Actions.Restart.Bindings);
        _logger.Debug($"Loaded {restartBindings.Count} restart bindings");
        
        // 更新UI
        UpdateUI(ListType.Pause);
        UpdateUI(ListType.Restart);
    }

    // 保存配置
    private void SaveConfig()
    {
        _logger.Debug("Saving bindings to config");
        
        // 保存Pause绑定
        _config.Actions.Pause.SetBindings(pauseBindings);
        
        // 保存Restart绑定  
        _config.Actions.Restart.SetBindings(restartBindings);
        
        _logger.Debug("Config saved");
    }

    #endregion
}
