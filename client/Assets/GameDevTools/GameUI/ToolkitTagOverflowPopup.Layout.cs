using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    internal sealed partial class ToolkitTagOverflowPopup
    {
        private float HorizontalInset => _popup.resolvedStyle.borderLeftWidth + _popup.resolvedStyle.borderRightWidth + _grid.resolvedStyle.paddingLeft + _grid.resolvedStyle.paddingRight;

        private float VerticalInset => _popup.resolvedStyle.borderTopWidth + _popup.resolvedStyle.borderBottomWidth + _grid.resolvedStyle.paddingTop + _grid.resolvedStyle.paddingBottom;

        internal static Rect CalculateBounds(Rect viewport, Rect anchor, float width, float desiredHeight)
        {
            width = Mathf.Min(width, Mathf.Max(0f, viewport.width - EDGE_MARGIN * 2));
            var top = Mathf.Clamp(anchor.yMin, viewport.yMin + EDGE_MARGIN, Mathf.Max(viewport.yMin + EDGE_MARGIN, viewport.yMax - EDGE_MARGIN));
            var bottom = Mathf.Clamp(anchor.yMax, viewport.yMin + EDGE_MARGIN, Mathf.Max(viewport.yMin + EDGE_MARGIN, viewport.yMax - EDGE_MARGIN));
            var below = Mathf.Max(0f, viewport.yMax - EDGE_MARGIN - bottom);
            var above = Mathf.Max(0f, top - viewport.yMin - EDGE_MARGIN);
            var openBelow = desiredHeight <= below || below >= above;
            var height = Mathf.Min(desiredHeight, openBelow ? below : above);
            var x = Mathf.Clamp(anchor.xMax - width, viewport.xMin + EDGE_MARGIN, Mathf.Max(viewport.xMin + EDGE_MARGIN, viewport.xMax - EDGE_MARGIN - width));
            var y = openBelow ? bottom : top - height;

            return new Rect(x, Mathf.Clamp(y, viewport.yMin + EDGE_MARGIN, Mathf.Max(viewport.yMin + EDGE_MARGIN, viewport.yMax - EDGE_MARGIN - height)), width, height);
        }

        private void MeasureButtons()
        {
            for (var index = 0; index < _buttons.Count; index++)
            {
                var btnTag = _buttons[index];
                var style = btnTag.resolvedStyle;
                var size = btnTag.MeasureTextSize(btnTag.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);

                size.x += style.paddingLeft + style.paddingRight + style.borderLeftWidth + style.borderRightWidth;
                size.y += style.paddingTop + style.paddingBottom + style.borderTopWidth + style.borderBottomWidth;
                _preferred[index] = new Vector2(Mathf.Max(1f, Finite(size.x, 48f)), Mathf.Max(26f, Finite(size.y, 26f)));
            }

            _measureDirty = false;
            _layoutDirty = true;
        }

        private Vector2 MeasureItem(int index, float available)
        {
            var size = _preferred[index];

            size.x = Mathf.Min(available, Mathf.Ceil(size.x));
            if (_preferred[index].x <= available) return size;

            var btnTag = _buttons[index];
            var style = btnTag.resolvedStyle;
            var horizontal = style.paddingLeft + style.paddingRight + style.borderLeftWidth + style.borderRightWidth;
            var vertical = style.paddingTop + style.paddingBottom + style.borderTopWidth + style.borderBottomWidth;
            var measured = btnTag.MeasureTextSize(btnTag.text, Mathf.Max(1f, size.x - horizontal), VisualElement.MeasureMode.Exactly, 0, VisualElement.MeasureMode.Undefined);

            size.y = Mathf.Max(26f, Finite(measured.y + vertical, 26f));
            return size;
        }

        private void Reflow(float minimum, float maximum, float height)
        {
            var availableHeight = Mathf.Max(1f, height - VerticalInset);

            _width = minimum;
            var paged = MeasureFlow(maximum, availableHeight, false) > availableHeight + .01f;

            if (paged)
            {
                _width = maximum;
            }
            else if (MeasureFlow(minimum, availableHeight, false) > availableHeight + .01f)
            {
                _width = FindWidth(minimum, maximum, availableHeight);
            }

            var footer = 0f;

            if (paged)
            {
                footer = _pager.Layout(Mathf.Max(1f, _width - _popup.resolvedStyle.borderLeftWidth - _popup.resolvedStyle.borderRightWidth));
            }

            _contentHeight = Mathf.Max(1f, availableHeight - footer);
            _pagerRoot.style.display = paged ? DisplayStyle.Flex : DisplayStyle.None;
            MeasureFlow(_width, _contentHeight, true);
            BuildPages(paged);
            ApplyPage();
            _layoutDirty = false;
        }

        private float FindWidth(float minimum, float maximum, float height)
        {
            while (maximum - minimum > .5f)
            {
                var middle = (minimum + maximum) * .5f;

                if (MeasureFlow(middle, height, false) <= height + .01f)
                {
                    maximum = middle;
                }
                else
                {
                    minimum = middle;
                }
            }

            return maximum;
        }

        private float MeasureFlow(float width, float maximumItemHeight, bool store)
        {
            var available = Mathf.Max(1f, width - HorizontalInset);
            var x = 0f;
            var y = 0f;
            var rowHeight = 0f;
            var start = 0;

            if (store)
            {
                _rows.Clear();
            }

            for (var index = 0; index < _buttons.Count; index++)
            {
                var size = MeasureItem(index, available);

                if (x > 0 && x + size.x > available + .01f)
                {
                    if (store)
                    {
                        _rows.Add(new Row(start, index, y, rowHeight));
                    }

                    start = index;
                    x = 0;
                    y += rowHeight + BUTTON_GAP;
                    rowHeight = 0;
                }

                if (store)
                {
                    size.y = Mathf.Min(size.y, maximumItemHeight);
                    _rects[index] = new Rect(x, y, size.x, size.y);
                }

                x += size.x + BUTTON_GAP;
                rowHeight = Mathf.Max(rowHeight, size.y);
            }

            if (store)
            {
                _rows.Add(new Row(start, _buttons.Count, y, rowHeight));
            }

            return y + rowHeight;
        }

        private static float Finite(float value, float fallback)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return fallback;

            return value;
        }

        private readonly struct Row
        {
            internal readonly int start;

            internal readonly int end;

            internal readonly float top;

            internal readonly float height;

            internal Row(int start, int end, float top, float height)
            {
                this.start = start;
                this.end = end;
                this.top = top;
                this.height = height;
            }
        }
    }
}
