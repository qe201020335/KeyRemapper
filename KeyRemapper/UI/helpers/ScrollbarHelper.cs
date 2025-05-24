using HMUI;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace KeyRemapper.UI.helpers
{
    internal static class ScrollbarHelper
    {
        private static GameObject scrollBarTemplate;
        
        public static void AddScrollbar(GameObject table, Transform targetContainer)
        {
            // 查找滚动条模板
            if (scrollBarTemplate == null)
            {
                scrollBarTemplate = Resources.FindObjectsOfTypeAll<VerticalScrollIndicator>()
                    .FirstOrDefault(x => x.enabled)?.transform.parent?.gameObject;
            }
            
            if (scrollBarTemplate == null) 
            {
                Plugin.Log?.Warn("Could not find scrollbar template");
                return;
            }
            
            var scrollView = table.GetComponentInChildren<ScrollView>();
            if (scrollView == null) 
            {
                Plugin.Log?.Warn("Could not find ScrollView in table");
                return;
            }
            
            // 实例化滚动条
            var listScrollBar = GameObject.Instantiate(scrollBarTemplate, targetContainer, false);
            listScrollBar.SetActive(true);
            
            // 获取私有字段的FieldInfo
            var scrollViewType = typeof(ScrollView);
            var verticalScrollIndicatorField = scrollViewType.GetField("_verticalScrollIndicator", BindingFlags.NonPublic | BindingFlags.Instance);
            var pageUpButtonField = scrollViewType.GetField("_pageUpButton", BindingFlags.NonPublic | BindingFlags.Instance);
            var pageDownButtonField = scrollViewType.GetField("_pageDownButton", BindingFlags.NonPublic | BindingFlags.Instance);
            
            // 连接垂直滚动指示器
            var vsi = listScrollBar.GetComponentInChildren<VerticalScrollIndicator>(true);
            if (vsi != null && verticalScrollIndicatorField != null)
            {
                verticalScrollIndicatorField.SetValue(scrollView, vsi);
            }
            
            // 连接翻页按钮
            var buttons = listScrollBar.GetComponentsInChildren<NoTransitionsButton>(true)
                .OrderByDescending(x => x.gameObject.name == "UpButton").ToArray();
                
            if (buttons.Length == 2 && pageUpButtonField != null && pageDownButtonField != null)
            {
                pageUpButtonField.SetValue(scrollView, buttons[0]);
                pageDownButtonField.SetValue(scrollView, buttons[1]);
                
                // 获取私有方法
                var pageUpMethod = scrollViewType.GetMethod("PageUpButtonPressed", BindingFlags.NonPublic | BindingFlags.Instance);
                var pageDownMethod = scrollViewType.GetMethod("PageDownButtonPressed", BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (pageUpMethod != null && pageDownMethod != null)
                {
                    buttons[0].onClick.AddListener(() => pageUpMethod.Invoke(scrollView, null));
                    buttons[1].onClick.AddListener(() => pageDownMethod.Invoke(scrollView, null));
                }
            }
            
            // 激活所有行为组件（修复模态框中滚动条不工作的问题）
            foreach (Transform child in listScrollBar.transform)
            {
                foreach (var behaviour in child.GetComponents<Behaviour>())
                {
                    behaviour.enabled = true;
                }
            }
            
            // 调用Update方法
            var updateMethod = scrollViewType.GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
            updateMethod?.Invoke(scrollView, null);
            
            // 添加刷新组件
            table.AddComponent<RefreshScrollbarOnFirstLoad>();
        }
        
        // 辅助组件，用于在第一次加载时刷新滚动条
        private class RefreshScrollbarOnFirstLoad : MonoBehaviour
        {
            void Start()
            {
                var scrollView = GetComponentInChildren<ScrollView>();
                if (scrollView != null)
                {
                    var updateMethod = typeof(ScrollView).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
                    updateMethod?.Invoke(scrollView, null);
                }
                Destroy(this);
            }
        }
    }
} 