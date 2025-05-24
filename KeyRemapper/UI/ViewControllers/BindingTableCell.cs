using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using KeyRemapper.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KeyRemapper.UI.ViewControllers
{
    internal class BindingTableCell : TableCell
    {
        [UIComponent("buttonText")] 
        private ClickableText buttonText;
        
        [UIComponent("deleteButton")] 
        private Button deleteButton;
        
        private ControllerButton currentButton;
        private Action<BindingTableCell> onDeleteClicked;
        private Action<BindingTableCell> onButtonClicked;
        
        [UIAction("button-clicked")]
        private void OnButtonClicked()
        {
            Plugin.Log?.Debug($"Button text clicked for {currentButton}");
            onButtonClicked?.Invoke(this);
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
            Action<BindingTableCell> deleteCallback,
            Action<BindingTableCell> clickCallback)
        {
            Plugin.Log?.Debug($"PopulateWithButton called for {button}");
            
            currentButton = button;
            onDeleteClicked = deleteCallback;
            onButtonClicked = clickCallback;
            
            // 设置按钮文本
            if (buttonText != null)
            {
                buttonText.text = button.ToString();
                Plugin.Log?.Debug($"Button text set to: {button}");
            }
            else
            {
                Plugin.Log?.Warn("buttonText component is null");
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