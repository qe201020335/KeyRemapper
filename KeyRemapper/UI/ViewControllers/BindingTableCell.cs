using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using HMUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using KeyRemapper.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KeyRemapper.UI.ViewControllers
{
    internal class BindingTableCell : TableCell, INotifyPropertyChanged
    {
        [UIComponent("buttonDropdown")] 
        private DropDownListSetting buttonDropdown;
        
        [UIComponent("deleteButton")] 
        private Button deleteButton;
        
        private ControllerButton currentButton;
        private Action<BindingTableCell> onDeleteClicked;
        private Action<BindingTableCell, ControllerButton> onButtonChanged;
        
        // 私有字段（不需要UIValue属性）
        private List<object> buttonOptions = new();
        private string selectedButton = "";
        
        // 公共属性（带UIValue属性）
        [UIValue("button-options")]
        public List<object> ButtonOptions
        {
            get
            {
                Plugin.Log?.Debug($"ButtonOptions getter called, count: {buttonOptions?.Count ?? 0}");
                if (buttonOptions != null && buttonOptions.Count > 0)
                {
                    Plugin.Log?.Debug($"First option: {buttonOptions[0]}");
                }
                return buttonOptions;
            }
        }
        
        [UIValue("selected-button")]
        public string SelectedButton
        {
            get
            {
                Plugin.Log?.Debug($"SelectedButton getter called, value: {selectedButton}");
                return selectedButton;
            }
            set
            {
                Plugin.Log?.Debug($"SelectedButton setter called, value: {value}");
                selectedButton = value;
                NotifyPropertyChanged();
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        [UIAction("button-changed")]
        private void OnButtonChanged(string newValue)
        {
            Plugin.Log?.Debug($"Dropdown value changed to: {newValue}");
            if (Enum.TryParse<ControllerButton>(newValue, out var newButton))
            {
                currentButton = newButton;
                onButtonChanged?.Invoke(this, newButton);
            }
        }
        
        [UIAction("delete-clicked")]
        private void OnDeleteClicked()
        {
            Plugin.Log?.Debug("Delete button clicked");
            onDeleteClicked?.Invoke(this);
        }
        
        public ControllerButton CurrentButton => currentButton;
        
        public void PopulateWithButton(
            ControllerButton button, 
            List<object> availableOptions,
            Action<BindingTableCell> deleteCallback,
            Action<BindingTableCell, ControllerButton> changeCallback)
        {
            Plugin.Log?.Debug($"PopulateWithButton called for {button}");
            Plugin.Log?.Debug($"Available options count: {availableOptions?.Count ?? 0}");
            
            if (availableOptions != null && availableOptions.Count > 0)
            {
                Plugin.Log?.Debug($"Options: {string.Join(", ", availableOptions)}");
            }
            
            currentButton = button;
            selectedButton = button.ToString();
            buttonOptions = availableOptions ?? new List<object>();
            onDeleteClicked = deleteCallback;
            onButtonChanged = changeCallback;
            
            Plugin.Log?.Debug($"Before NotifyPropertyChanged - selectedButton: {selectedButton}, buttonOptions count: {buttonOptions.Count}");
            
            // 触发UI更新
            NotifyPropertyChanged(nameof(ButtonOptions));
            NotifyPropertyChanged(nameof(SelectedButton));
            
            Plugin.Log?.Debug("NotifyPropertyChanged called for both properties");
            
            // 手动刷新下拉框
            if (buttonDropdown != null)
            {
                Plugin.Log?.Debug("Attempting to manually refresh dropdown");
                
                try
                {
                    // 直接设置Values属性
                    buttonDropdown.Values = buttonOptions;
                    Plugin.Log?.Debug($"Set Values property, count: {buttonOptions.Count}");
                    
                    // 调用UpdateChoices方法更新显示
                    buttonDropdown.UpdateChoices();
                    Plugin.Log?.Debug("Called UpdateChoices method");
                    
                    // 设置当前选中的值
                    buttonDropdown.Value = selectedButton;
                    Plugin.Log?.Debug($"Set Value to: {selectedButton}");
                    
                    // 如果需要，也可以尝试通过反射访问内部的dropdown
                    var dropdownField = buttonDropdown.GetType().GetField("dropdown", 
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    
                    if (dropdownField != null)
                    {
                        var simpleDropdown = dropdownField.GetValue(buttonDropdown);
                        if (simpleDropdown != null)
                        {
                            Plugin.Log?.Debug("Got internal SimpleTextDropdown");
                            
                            // 查找SetTexts方法并调用
                            var setTextsMethod = simpleDropdown.GetType().GetMethod("SetTexts", 
                                BindingFlags.Public | BindingFlags.Instance);
                            if (setTextsMethod != null)
                            {
                                var textList = buttonOptions.Select(o => o.ToString()).ToList();
                                setTextsMethod.Invoke(simpleDropdown, new object[] { textList });
                                Plugin.Log?.Debug("Called SetTexts on SimpleTextDropdown");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Log?.Error($"Error refreshing dropdown: {ex.Message}");
                    Plugin.Log?.Error($"Stack trace: {ex.StackTrace}");
                }
            }
            else
            {
                Plugin.Log?.Warn("buttonDropdown is null, cannot refresh");
            }
        }

        protected override void SelectionDidChange(TransitionType transitionType)
        {
            RefreshBackground();
        }

        protected override void HighlightDidChange(TransitionType transitionType)
        {
            RefreshBackground();
        }
        
        private void RefreshBackground()
        {
            // 可以在这里调整背景颜色
        }
    }
} 