using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    internal sealed partial class ToolkitTagFilterBar
    {
        private const float TAG_WIDTH_RATIO = .6f;

        private const float MINIMUM_SEARCH_REGION_WIDTH = 400f;

        private const float MINIMUM_SEARCH_FIELD_WIDTH = 40f;

        private const float TOOLBAR_GAP = 8f;

        private void Layout()
        {
            var width = _toolbar.contentRect.width;
            var sharedWidth = Mathf.Max(0, width - TOOLBAR_GAP);
            var tagWidth = sharedWidth * TAG_WIDTH_RATIO;
            var searchWidth = sharedWidth - tagWidth;
            var stacked = searchWidth < MINIMUM_SEARCH_REGION_WIDTH;

            if (stacked)
            {
                tagWidth = width;
                searchWidth = width;
            }

            ApplyToolbarLayout(stacked, tagWidth, searchWidth);
            var allWidth = Measure(_btnAll.text);
            var moreWidth = Measure(_btnMore.text);

            SetWidth(_btnAll, allWidth);
            SetWidth(_btnMore, moreWidth);
            ApplyVisibleTags(tagWidth, allWidth, moreWidth);
            _dirty = false;
        }

        private void ApplyToolbarLayout(bool stacked, float tagWidth, float searchWidth)
        {
            _toolbar.EnableInClassList("tag-toolbar-stacked", stacked);
            _toolbar.style.flexDirection = stacked ? FlexDirection.Column : FlexDirection.Row;
            _toolbar.style.alignItems = stacked
                ? Align.FlexStart
                : Align.FlexEnd;
            _searchGroup.style.marginTop = stacked ? 6f : 0f;
            _searchGroup.style.marginLeft = stacked ? 0f : TOOLBAR_GAP;
            SetWidth(_host, tagWidth);
            SetWidth(_searchGroup, searchWidth);
            LayoutSearch(stacked, searchWidth);
        }

        private void LayoutSearch(bool stacked, float width)
        {
            var inset = 0f;

            if (!stacked)
            {
                inset = TOOLBAR_GAP + 1f;
            }

            var available = Mathf.Max(1f, width - inset);
            var controlsWidth = SearchControlsWidth();
            var wrap = available < controlsWidth + MINIMUM_SEARCH_FIELD_WIDTH;
            var fieldWidth = available;

            if (!wrap)
            {
                fieldWidth -= controlsWidth;
            }

            // 完整容纳时关闭自动换行, 避免 Unity 2022 的临界宽度误判.
            _searchGroup.style.flexWrap = wrap ? Wrap.Wrap : Wrap.NoWrap;
            foreach (var child in _searchGroup.Children())
            {
                if (child is TextField)
                {
                    SetWidth(child, fieldWidth);
                }
            }
        }

        private void ApplyVisibleTags(float available, float allWidth, float moreWidth)
        {
            var total = allWidth;

            foreach (var tag in _tags)
            {
                total += GAP + Measure(tag);
            }

            var count = _tags.Length;
            var overflow = total > available + .01f;

            if (overflow)
            {
                count = 0;
                var used = allWidth + GAP + moreWidth;

                foreach (var tag in _tags)
                {
                    used += GAP + Measure(tag);
                    if (used > available + .01f) break;

                    count++;
                }
            }

            ApplyButtons(count, overflow);
        }

        private float SearchControlsWidth()
        {
            var width = 0f;

            foreach (var child in _searchGroup.Children())
            {
                if (child is TextField) continue;

                var style = child.resolvedStyle;
                var desired = child.style.width.value.value;

                if (desired <= 0 || float.IsNaN(desired))
                {
                    desired = style.width;
                }

                if (float.IsNaN(desired) || float.IsInfinity(desired)) continue;

                // 读取控件的固定上限, 不把上一帧被压缩的滑块宽度当作新的正常宽度.
                var maximum = style.maxWidth;

                if (maximum.keyword == StyleKeyword.Undefined && !float.IsNaN(maximum.value) && !float.IsInfinity(maximum.value))
                {
                    desired = Mathf.Max(desired, maximum.value);
                }

                width += desired + style.marginLeft + style.marginRight;
            }

            return width;
        }

        private static void SetWidth(VisualElement element, float width)
        {
            element.style.width = width;
            element.style.minWidth = width;
            element.style.maxWidth = width;
            element.style.flexGrow = 0;
            element.style.flexShrink = 0;
        }
    }
}
