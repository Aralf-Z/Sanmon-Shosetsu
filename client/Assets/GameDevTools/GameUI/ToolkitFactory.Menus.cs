using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    public static partial class ToolkitFactory
    {
        internal static VisualElement PrepareMenu(GenericDropdownMenu menu)
        {
            menu.contentContainer.AddToClassList("context-menu");
            var root = menu.contentContainer;

            while (root != null && !root.ClassListContains(GenericDropdownMenu.ussClassName))
            {
                root = root.hierarchy.parent;
            }

            if (root == null) return null;

            root.AddToClassList("context-menu-root");
            foreach (var sheet in new[] { _layoutSheet, _styleSheet, _controlsSheet })
            {
                if (sheet != null)
                {
                    root.styleSheets.Add(sheet);
                }
            }

            NormalizeThemeSheets(root);
            ApplyFont(root);
            ToolkitIcons.EnableMenuIcons(root);
            ApplyMenuOpacity(root);
            TrackMenu(root);
            ExpandMenu(root);
            return root;
        }

        private static void ExpandMenu(VisualElement root)
        {
            var scroll = root.Q<ScrollView>();
            var outer = root.Q(className: GenericDropdownMenu.containerOuterUssClassName);

            if (scroll == null || outer == null) return;

            scroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            scroll.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            scroll.mouseWheelScrollSize = 0;
            root.schedule.Execute(() => FitMenu(root, outer, scroll)).Every(16);
        }

        // 原生菜单在视口边缘会压缩滚动区域, 在调度中按完整内容恢复高度并向内移动.
        private static void FitMenu(VisualElement root, VisualElement outer, ScrollView scroll)
        {
            if (root.panel == null || root.contentRect.height <= 0) return;

            var contentStyle = scroll.contentContainer.resolvedStyle;
            var height = contentStyle.paddingTop + contentStyle.paddingBottom + contentStyle.borderTopWidth + contentStyle.borderBottomWidth;

            foreach (var row in scroll.contentContainer.Children())
            {
                if (row.resolvedStyle.display == DisplayStyle.None) continue;

                var rowHeight = MenuRowHeight(row);

                SetMenuHeight(row, rowHeight);
                height += rowHeight + row.resolvedStyle.marginTop + row.resolvedStyle.marginBottom;
            }

            var scrollStyle = scroll.resolvedStyle;
            var outerStyle = outer.resolvedStyle;

            height += scrollStyle.paddingTop + scrollStyle.paddingBottom + scrollStyle.borderTopWidth + scrollStyle.borderBottomWidth;
            SetMenuHeight(scroll, height);
            height += outerStyle.paddingTop + outerStyle.paddingBottom + outerStyle.borderTopWidth + outerStyle.borderBottomWidth;
            SetMenuHeight(outer, height);
            var left = Mathf.Clamp(outer.style.left.value.value, 0, Mathf.Max(0, root.contentRect.width - outer.layout.width));
            var top = Mathf.Clamp(outer.style.top.value.value, 0, Mathf.Max(0, root.contentRect.height - height));

            if (!Mathf.Approximately(outer.style.left.value.value, left)) outer.style.left = left;

            if (!Mathf.Approximately(outer.style.top.value.value, top)) outer.style.top = top;

            if (scroll.scrollOffset != Vector2.zero) scroll.scrollOffset = Vector2.zero;
        }

        private static float MenuRowHeight(VisualElement row)
        {
            var style = row.resolvedStyle;
            var minimum = 30f;

            if (row.ClassListContains(GenericDropdownMenu.separatorUssClassName)) return Mathf.Max(2f, style.minHeight.value);

            var label = row.Q<Label>(className: GenericDropdownMenu.labelUssClassName);

            if (label != null)
            {
                var text = ToolkitMixedText.GetText(label);
                var size = ToolkitMixedText.Measure(label, text, 0, false);

                if (!float.IsNaN(size.y) && !float.IsInfinity(size.y))
                {
                    minimum = Mathf.Max(minimum, size.y + style.paddingTop + style.paddingBottom + style.borderTopWidth + style.borderBottomWidth);
                }
            }

            return Mathf.Ceil(Mathf.Max(minimum, style.minHeight.value));
        }

        private static void SetMenuHeight(VisualElement element, float height)
        {
            if (!Mathf.Approximately(element.style.height.value.value, height)) element.style.height = height;

            if (!Mathf.Approximately(element.style.minHeight.value.value, height)) element.style.minHeight = height;
        }
    }
}
