using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    internal sealed partial class ToolkitPager
    {
        private void Tick()
        {
            if (_disposed || _root.panel == null || _root.resolvedStyle.display == DisplayStyle.None) return;

            var width = _root.layout.width;
            var fontKey = HashCode.Combine(ToolkitFontMetrics.GetKey(_txtPage), ToolkitFontMetrics.GetKey(_inputJump));

            if (!Finite(width) || width <= 0) return;

            if (_dirty || _width != width || _fontKey != fontKey)
            {
                _fontKey = fontKey;
                Layout(width);
            }
        }

        // 标签弹窗先取得页脚高度, 再按剩余空间分页; 普通面板使用同一个布局入口.
        internal float Layout(float width)
        {
            var style = _root.resolvedStyle;
            var left = style.paddingLeft + style.borderLeftWidth;
            var top = style.paddingTop + style.borderTopWidth;
            var available = Mathf.Max(1, width - left - style.paddingRight - style.borderRightWidth);
            var digits = Math.Max(_pages, _pageCountHint).ToString();
            var captionWidth = Mathf.Min(available, Mathf.Max(48, Measure(digits + " / " + digits) + 4));
            var inputWidth = Mathf.Ceil(MeasureInputWidth()) + 12;
            var jumpWidth = Mathf.Min(available, inputWidth + GAP + CONTROL_WIDTH);
            var navigationWidth = CONTROL_WIDTH * 4 + captionWidth + GAP * 4;
            var together = navigationWidth + 2 * (jumpWidth + GAP) <= available;
            var navigationHeight = LayoutNavigation(available, captionWidth);
            var jumpHeight = LayoutJump(jumpWidth, inputWidth);
            var jumpTop = top;

            if (!together)
            {
                jumpTop += navigationHeight + GAP;
            }

            SetRect(_navigation, left, top, available, navigationHeight);
            SetRect(_jumpGroup, left + available - jumpWidth, jumpTop, jumpWidth, jumpHeight);
            var height = Math.Max(top + navigationHeight, jumpTop + jumpHeight) + style.paddingBottom + style.borderBottomWidth;

            _root.style.height = height;
            _root.style.minHeight = height;
            _width = width;
            _dirty = false;
            return height;
        }

        private float LayoutNavigation(float available, float captionWidth)
        {
            var elements = new VisualElement[] { _buttons[0], _buttons[1], _txtPage, _buttons[2], _buttons[3] };
            var widths = new[] { CONTROL_WIDTH, CONTROL_WIDTH, captionWidth, CONTROL_WIDTH, CONTROL_WIDTH };
            var first = 0;
            var used = 0f;
            var top = 0f;

            for (var index = 0; index < elements.Length; index++)
            {
                widths[index] = Mathf.Min(available, widths[index]);
                var gap = index > first ? GAP : 0;

                if (index > first && used + gap + widths[index] > available + .01f)
                {
                    PositionRow(elements, widths, first, index, used, available, top);
                    top += CONTROL_HEIGHT + GAP;
                    first = index;
                    used = 0;
                    gap = 0;
                }

                used += gap + widths[index];
            }

            PositionRow(elements, widths, first, elements.Length, used, available, top);
            return top + CONTROL_HEIGHT;
        }

        private float LayoutJump(float available, float inputWidth)
        {
            if (inputWidth + GAP + CONTROL_WIDTH <= available + .01f)
            {
                SetRect(_inputJump, 0, 0, inputWidth, CONTROL_HEIGHT);
                SetRect(_buttons[4], available - CONTROL_WIDTH, 0, CONTROL_WIDTH, CONTROL_HEIGHT);
                return CONTROL_HEIGHT;
            }

            SetRect(_inputJump, 0, 0, available, CONTROL_HEIGHT);
            SetRect(_buttons[4], Mathf.Max(0, available - CONTROL_WIDTH), CONTROL_HEIGHT + GAP, Mathf.Min(available, CONTROL_WIDTH), CONTROL_HEIGHT);
            return CONTROL_HEIGHT * 2 + GAP;
        }

        private static void PositionRow(VisualElement[] elements, float[] widths, int start, int end, float used, float available, float top)
        {
            var left = (available - used) * .5f;

            for (var index = start; index < end; index++)
            {
                SetRect(elements[index], left, top, widths[index], CONTROL_HEIGHT);
                left += widths[index] + GAP;
            }
        }

        private float Measure(string text)
        {
            var width = _txtPage.MeasureTextSize(text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined).x;

            if (!Finite(width)) return text.Length * 8f;

            return Mathf.Ceil(width);
        }

        private float MeasureInputWidth()
        {
            var text = _inputJump.Q<TextElement>(className: "unity-text-element");

            if (text == null) return _inputJump.resolvedStyle.fontSize * 2.4f;

            var width = text.MeasureTextSize("0000", 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined).x;

            if (!Finite(width)) return _inputJump.resolvedStyle.fontSize * 2.4f;

            return width;
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private static void SetRect(VisualElement element, float left, float top, float width, float height)
        {
            element.style.left = left;
            element.style.top = top;
            element.style.width = width;
            element.style.minWidth = width;
            element.style.maxWidth = width;
            element.style.height = height;
            element.style.minHeight = height;
            element.style.maxHeight = height;
        }
    }
}
