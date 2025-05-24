using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using KeyRemapper.Configuration;

namespace KeyRemapper.UI.helpers
{
    [HotReload(RelativePathToLayout = @"..\Views\buttonSelectorModal.bsml")]
    [ViewDefinition("KeyRemapper.UI.Views.buttonSelectorModal.bsml")]
    internal class ButtonSelectorModal : BSMLAutomaticViewController
    {
        [UIParams]
        private BSMLParserParams parserParams = null;

        [UIComponent("leftHandButtons")]
        private Transform leftHandContainer = null;

        [UIComponent("rightHandButtons")]
        private Transform rightHandContainer = null;

        [UIValue("currentButtonText")]
        private string currentButtonText = "当前: 无";

        private Action<ControllerButton> onButtonSelected;
        private List<ControllerButton> usedButtons;
        private ControllerButton currentButton;

        private bool isInitialized = false;

        // 左手控制器按钮
        private static readonly ControllerButton[] leftHandButtons = new[]
        {
            ControllerButton.L_Trigger,
            ControllerButton.L_Grip,
            ControllerButton.L_X,
            ControllerButton.L_Y,
            ControllerButton.L_Stick,
            ControllerButton.L_Menu
        };

        // 右手控制器按钮
        private static readonly ControllerButton[] rightHandButtons = new[]
        {
            ControllerButton.R_Trigger,
            ControllerButton.R_Grip,
            ControllerButton.R_A,
            ControllerButton.R_B,
            ControllerButton.R_Stick,
            ControllerButton.R_Menu
        };

        [UIAction("#post-parse")]
        private void PostParse()
        {
            // 在 BSML 解析完成后创建按钮列表
            if (leftHandContainer != null && rightHandContainer != null)
            {
                CreateButtonList();
                isInitialized = true;
            }
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
        }

        public void ShowModal(ControllerButton current, List<ControllerButton> used, Action<ControllerButton> callback)
        {
            currentButton = current;
            usedButtons = used ?? new List<ControllerButton>();
            onButtonSelected = callback;
            
            currentButtonText = $"当前: {current}";
            NotifyPropertyChanged(nameof(currentButtonText));
            
            // 确保按钮列表已创建
            if (leftHandContainer != null && rightHandContainer != null && leftHandContainer.childCount == 0)
            {
                CreateButtonList();
            }
            
            UpdateButtonStates();
            
            // 确保 parserParams 不为 null
            if (parserParams != null)
            {
                parserParams.EmitEvent("show-modal");
            }
            else
            {
                // 如果 parserParams 为 null，尝试手动显示模态框
                Plugin.Log?.Error("ButtonSelectorModal: parserParams is null! BSML may not have been parsed correctly.");
                
                // 尝试手动解析如果还没有初始化
                if (!isInitialized)
                {
                    Plugin.Log?.Warn("ButtonSelectorModal: Attempting manual BSML parse...");
                    try
                    {
                        var parserResult = BeatSaberMarkupLanguage.BSMLParser.Instance.Parse(
                            BeatSaberMarkupLanguage.Utilities.GetResourceContent(
                                System.Reflection.Assembly.GetExecutingAssembly(), 
                                "KeyRemapper.UI.Views.buttonSelectorModal.bsml"),
                            gameObject, 
                            this
                        );
                        
                        if (parserResult != null)
                        {
                            parserParams = parserResult;
                            PostParse(); // 手动调用 post-parse
                            parserParams.EmitEvent("show-modal");
                            Plugin.Log?.Info("ButtonSelectorModal: Manual BSML parse successful!");
                        }
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log?.Error($"ButtonSelectorModal: Failed to manually parse BSML: {ex}");
                    }
                }
            }
        }

        private void CreateButtonList()
        {
            // 检查容器是否已初始化
            if (leftHandContainer == null || rightHandContainer == null)
            {
                return;
            }
            
            // 创建左手按钮
            foreach (var button in leftHandButtons)
            {
                CreateButtonElement(button, leftHandContainer);
            }

            // 创建右手按钮
            foreach (var button in rightHandButtons)
            {
                CreateButtonElement(button, rightHandContainer);
            }
        }

        private void CreateButtonElement(ControllerButton button, Transform container)
        {
            // 创建按钮对象
            var buttonGO = new GameObject("ButtonOption");
            buttonGO.transform.SetParent(container, false);
            
            // 添加按钮组件和背景图片
            var btn = buttonGO.AddComponent<Button>();
            var image = buttonGO.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // 设置按钮大小
            var rect = buttonGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(50, 6);
            
            // 创建文本子对象
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);
            
            // 添加并设置文本组件
            var text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = button.ToString();
            text.fontSize = 3.5f;
            text.alignment = TextAlignmentOptions.Center;
            
            // 设置文本的 RectTransform 占满父对象
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            // 存储按钮数据
            buttonGO.name = button.ToString();
            
            // 添加点击事件
            btn.onClick.AddListener(() => OnButtonClicked(button));
        }

        private void UpdateButtonStates()
        {
            // 更新左手按钮状态
            UpdateContainerButtons(leftHandContainer);
            
            // 更新右手按钮状态
            UpdateContainerButtons(rightHandContainer);
        }

        private void UpdateContainerButtons(Transform container)
        {
            // 检查容器是否为空
            if (container == null)
            {
                return;
            }
            
            foreach (Transform child in container)
            {
                if (Enum.TryParse<ControllerButton>(child.name, out var button))
                {
                    var btn = child.GetComponent<Button>();
                    var image = child.GetComponent<Image>();
                    
                    // 文本组件在子对象中
                    var textTransform = child.Find("Text");
                    var text = textTransform != null ? textTransform.GetComponent<TextMeshProUGUI>() : null;
                    
                    if (btn == null || image == null || text == null)
                    {
                        continue;
                    }
                    
                    bool isUsed = usedButtons.Contains(button) && button != currentButton;
                    bool isCurrent = button == currentButton;
                    
                    btn.interactable = !isUsed;
                    
                    if (isCurrent)
                    {
                        text.color = Color.green;
                        image.color = new Color(0.1f, 0.4f, 0.1f, 0.8f);
                    }
                    else if (isUsed)
                    {
                        text.color = Color.gray;
                        image.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
                    }
                    else
                    {
                        text.color = Color.white;
                        image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                    }
                }
            }
        }

        private void OnButtonClicked(ControllerButton button)
        {
            if (button != currentButton && !usedButtons.Contains(button))
            {
                onButtonSelected?.Invoke(button);
                parserParams.EmitEvent("hide-modal");
            }
        }
    }
} 