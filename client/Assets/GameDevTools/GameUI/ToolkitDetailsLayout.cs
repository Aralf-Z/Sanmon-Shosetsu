using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 无滚动的悬停详情布局, 在主框下边界内延长并按完整行省略. </summary>
    internal sealed class ToolkitDetailsLayout
    {
        private readonly VisualElement _root; // 卡片外框, 默认 340 面板单位高.
        private readonly VisualElement _window; // 详情延长的下边界.
        private readonly Label _title; // 卡片固定标题.
        private readonly Image _icon; // 空间充足时显示的预设图标.
        private readonly Label[] _labels; // 按名称, 类型, 正文顺序分配高度.
        private readonly string[] _sources = new string[3]; // 保留未截断的内容.
        private readonly float[] _heights = new float[3]; // 此次布局中各字段的可见高度.
        private float _lastWidth = -1; // 上次测量的内容宽度.
        private float _lastAvailable = -1; // 上次测量的主框剩余高度.
        private int _lastFontKey; // 字体, 段落间距及字段边距改变后重新测量.
        private bool _dirty = true; // 合并同一帧的悬停内容变化.

        /// <summary> 绑定普通内容元素, 不创建 ScrollView. </summary>
        internal ToolkitDetailsLayout(VisualElement root)
        {
            _root = ToolkitView.Require<VisualElement>(root, "commandHoverDetails");
            _window = ToolkitView.Require<VisualElement>(root, "toolkitWindow");
            _title = ToolkitView.Require<Label>(root, "hoverDetailsTitle");
            _icon = ToolkitView.Require<Image>(root, "hoverDetailIcon");
            _labels = new[] { ToolkitView.Require<Label>(root, "hoverDetailName"), ToolkitView.Require<Label>(root, "hoverDetailKind"), ToolkitView.Require<Label>(root, "hoverDetailBody") };
        }

        /// <summary> 只保存最新原文, 实际测量交给可见帧. </summary>
        internal void SetText(string name, string kind, string body)
        {
            _sources[0] = name ?? string.Empty;
            _sources[1] = kind ?? string.Empty;
            _sources[2] = body ?? string.Empty;
            _dirty = true;
        }

        /// <summary> 默认高度不缩短, 长内容延长至主框底边为止. </summary>
        internal void Tick()
        {
            var width = _root.contentRect.width;
            var available = Mathf.Max(0, _window.worldBound.yMax - _root.worldBound.yMin);
            var fontSize = _labels[0].resolvedStyle.fontSize + _labels[1].resolvedStyle.fontSize + _labels[2].resolvedStyle.fontSize;
            var spacingKey = HashCode.Combine(_title.resolvedStyle.unityParagraphSpacing, _labels[0].resolvedStyle.unityParagraphSpacing, _labels[1].resolvedStyle.unityParagraphSpacing, _labels[2].resolvedStyle.unityParagraphSpacing, Margin(0), Margin(1), Margin(2));
            var fontKey = HashCode.Combine(ToolkitFontMetrics.GetKey(_title), ToolkitFontMetrics.GetKey(_labels[0]), ToolkitFontMetrics.GetKey(_labels[1]), ToolkitFontMetrics.GetKey(_labels[2]), spacingKey);
            if (_root.panel == null || width <= 0 || float.IsNaN(width) || float.IsNaN(available) || float.IsInfinity(width) || float.IsInfinity(available))
                return;
            if (!_dirty && Mathf.Approximately(width, _lastWidth) && Mathf.Approximately(available, _lastAvailable) && fontKey == _lastFontKey)
                return;
            _lastWidth = width;
            _lastAvailable = available;
            _lastFontKey = fontKey;
            _dirty = false;
            var shell = _root.resolvedStyle.paddingTop + _root.resolvedStyle.paddingBottom + _root.resolvedStyle.borderTopWidth + _root.resolvedStyle.borderBottomWidth + Mathf.Max(24f, ToolkitTextEllipsis.Measure(_title, _title.text, width, false).y) + 8f;
            var showIcon = _icon.sprite != null && available >= shell + 72f + fontSize;
            _icon.EnableInClassList("is-hidden", !showIcon);
            shell += showIcon ? 72f : 0;
            var natural = shell;
            for (var i = 0; i < _labels.Length; i++)
            {
                _heights[i] = string.IsNullOrEmpty(_sources[i]) ? 0 : ToolkitTextEllipsis.Measure(_labels[i], _sources[i], width, true).y;
                natural += _heights[i] + Margin(i);
            }
            var height = Mathf.Min(available, Mathf.Max(340f, natural));
            _root.style.height = height;
            var remaining = Mathf.Max(0, height - shell);
            var lastVisible = -1;
            for (var i = 0; i < _labels.Length; i++)
            {
                var minimum = ToolkitTextEllipsis.Measure(_labels[i], "...", width, true).y;
                var budget = Mathf.Max(0, remaining - Margin(i));
                _heights[i] = budget >= minimum ? Mathf.Min(_heights[i], budget) : 0;
                if (_heights[i] > 0)
                {
                    lastVisible = i;
                    remaining -= _heights[i] + Margin(i);
                }
            }
            for (var i = 0; i < _labels.Length; i++)
            {
                var label = _labels[i];
                label.EnableInClassList("detail-field-hidden", _heights[i] <= 0);
                label.style.height = _heights[i];
                var omittedFields = false;
                for (var next = i + 1; i == lastVisible && next < _labels.Length; next++)
                    omittedFields |= !string.IsNullOrEmpty(_sources[next]);
                label.text = _heights[i] <= 0 ? string.Empty : ToolkitTextEllipsis.Fit(_sources[i], width, _heights[i], candidate => ToolkitTextEllipsis.Measure(label, candidate, width, true), forceEllipsis: omittedFields);
            }
        }

        // 与 UI Builder 中的字段边距一致, 空字段不占空间.
        private float Margin(int index) => string.IsNullOrEmpty(_sources[index]) ? 0 : _labels[index].resolvedStyle.marginTop + _labels[index].resolvedStyle.marginBottom;
    }
}
