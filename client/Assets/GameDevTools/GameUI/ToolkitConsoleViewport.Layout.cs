using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    internal sealed partial class ToolkitConsoleViewport
    {
        private const int ICONS_FLAG = 1; // 类型图标参与正文可用宽度计算.
        private const int TIMESTAMPS_FLAG = 2; // 接收时间只参与显示, 不改变原始正文.
        private readonly Dictionary<long, RowLayout> _layouts = new(); // 稳定编号对应的正文, 格式化文本及有效行高.
        private Row _measureRow; // 唯一隐藏测量模板, 新节点不必逐个等待样式布局.
        private bool _showIcons = true; // 与工具栏默认值一致.
        private bool _showTimestamps = true; // 已缓存日志也使用相同显示条件.
        private int PresentationKey => (_showIcons ? ICONS_FLAG : 0) | (_showTimestamps ? TIMESTAMPS_FLAG : 0);

        /// <summary> 修改日志外观时保留筛选范围和阅读锚点, 按现有预算重新排版. </summary>
        internal void SetPresentation(bool icons, bool timestamps)
        {
            if (_showIcons == icons && _showTimestamps == timestamps) return;
            var anchor = CaptureAnchor();
            _showIcons = icons;
            _showTimestamps = timestamps;
            if (!Following && anchor.id != 0) _anchor = anchor;
            RebuildHeights();
            _dirty = true;
        }

        // 测量模板与实际行共用 UXML 和 USS, 不设置示例正文或修改业务日志.
        private bool EnsureMetrics()
        {
            if (_measureRow == null) return false;
            var style = _measureRow.text.resolvedStyle;
            if (!Valid(style.fontSize) || !Valid(_measureRow.marker.resolvedStyle.width)) return false;
            var sample = _measureRow.text.MeasureTextSize("Mg", 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
            if (!Valid(sample.y)) return false;
            var key = ToolkitFontMetrics.GetKey(_measureRow.text);
            if (_fontKey != key)
            {
                var anchor = CaptureAnchor();
                _fontKey = key;
                if (!Following && anchor.id != 0) _anchor = anchor;
                RebuildHeights();
            }
            return true;
        }

        private int DisplayCount(ConsoleLogItem item) => _model.Collapsed && item.Count > 1 ? item.Count : 0;

        // 初始范围只复用当前样式下仍然有效的高度, 其他行采用最低高度占位.
        private float CachedHeight(int index)
        {
            var item = _model.GetItem(index);
            return _layouts.TryGetValue(item.Id, out var layout) && layout.Matches(item, DisplayCount(item), _width, _fontKey, PresentationKey) ? layout.height : 24f;
        }

        // 只有正文, 折叠次数, 可用宽度或字体变化才重新测量.
        private RowLayout GetLayout(ConsoleLogItem item)
        {
            var count = DisplayCount(item);
            _layouts.TryGetValue(item.Id, out var previous);
            if (previous != null && previous.Matches(item, count, _width, _fontKey, PresentationKey)) return previous;
            var record = item.Record;
            var sameText = previous != null && previous.message == record.Message && previous.color == record.Color && previous.presentation == PresentationKey && (!_showTimestamps || previous.time == record.Time);
            var message = sameText ? null : _showTimestamps ? $"[{record.Time:HH:mm:ss}] {record.Message}" : record.Message;
            var text = sameText ? previous.text : string.IsNullOrEmpty(record.Color) ? message : $"<color={record.Color}>{message}</color>";
            var countText = previous != null && previous.count == count ? previous.countText : count > 0 ? $"×{count}" : string.Empty;
            var root = _measureRow.root.resolvedStyle;
            var label = _measureRow.text.resolvedStyle;
            var marker = _measureRow.marker.resolvedStyle;
            var badge = _measureRow.count.resolvedStyle;
            var countSize = count > 0 ? _measureRow.count.MeasureTextSize(countText, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined) : Vector2.zero;
            var countWidth = count > 0 ? Mathf.Max(badge.minWidth.value, countSize.x + badge.paddingLeft + badge.paddingRight + badge.borderLeftWidth + badge.borderRightWidth) + badge.marginLeft + badge.marginRight : 0;
            var markerWidth = _showIcons ? marker.width + marker.marginLeft + marker.marginRight : 0;
            var width = Mathf.Max(1f, _width - root.paddingLeft - root.paddingRight - root.borderLeftWidth - root.borderRightWidth - markerWidth - countWidth - label.marginLeft - label.marginRight - label.paddingLeft - label.paddingRight - label.borderLeftWidth - label.borderRightWidth);
            var measured = _measureRow.text.MeasureTextSize(text ?? string.Empty, width, VisualElement.MeasureMode.Exactly, 0, VisualElement.MeasureMode.Undefined);
            if (!Valid(measured.y)) measured.y = label.fontSize;
            var textHeight = Mathf.Ceil(Mathf.Max(label.minHeight.value, measured.y + label.paddingTop + label.paddingBottom + label.borderTopWidth + label.borderBottomWidth));
            var countHeight = count > 0 ? Mathf.Max(badge.minHeight.value, countSize.y + badge.paddingTop + badge.paddingBottom + badge.borderTopWidth + badge.borderBottomWidth) + badge.marginTop + badge.marginBottom : 0;
            var height = Mathf.Ceil(Mathf.Max(root.minHeight.value, Mathf.Max(textHeight + label.marginTop + label.marginBottom, countHeight) + root.paddingTop + root.paddingBottom + root.borderTopWidth + root.borderBottomWidth));
            var result = new RowLayout(record.Message, record.Time, record.Color, count, _width, _fontKey, PresentationKey, text, countText, Mathf.Max(24f, height), textHeight);
            _layouts[item.Id] = result;
            return result;
        }

        private void AcceptHeight(int index, float height)
        {
            var anchor = CaptureAnchor();
            if (!_heights.Set(index, height)) return;
            if (!Following && anchor.id != 0) _anchor = anchor;
        }

        // 缓存条目不可变, 活跃节点可以直接比较引用, 不重复设置富文本.
        private sealed class RowLayout
        {
            /// <summary> 不含显示层时间戳的原始正文. </summary>
            internal readonly string message;
            /// <summary> 实际接收时间, 开启时间戳时参与缓存校验. </summary>
            internal readonly DateTime time;
            /// <summary> 富文本外层颜色. </summary>
            internal readonly string color;
            /// <summary> 当前筛选下显示的折叠次数. </summary>
            internal readonly int count;
            /// <summary> 当前可视宽度. </summary>
            internal readonly float width;
            /// <summary> 实际字体和样式标识. </summary>
            internal readonly int fontKey;
            /// <summary> 图标和时间戳的显示条件. </summary>
            internal readonly int presentation;
            /// <summary> 仅格式化一次的富文本. </summary>
            internal readonly string text;
            /// <summary> 仅格式化一次的次数文本. </summary>
            internal readonly string countText;
            /// <summary> 包括内边距的实际行高. </summary>
            internal readonly float height;
            /// <summary> 正文标签高度, 不截断多行内容. </summary>
            internal readonly float textHeight;

            /// <summary> 保存一次有效测量和可复用的格式化结果. </summary>
            internal RowLayout(string message, DateTime time, string color, int count, float width, int fontKey, int presentation, string text, string countText, float height, float textHeight)
            {
                this.message = message;
                this.time = time;
                this.color = color;
                this.count = count;
                this.width = width;
                this.fontKey = fontKey;
                this.presentation = presentation;
                this.text = text;
                this.countText = countText;
                this.height = height;
                this.textHeight = textHeight;
            }

            /// <summary> 只有实际排版条件变化才使缓存失效. </summary>
            internal bool Matches(ConsoleLogItem item, int displayCount, float viewportWidth, int currentFont, int currentPresentation) => message == item.Record.Message && color == item.Record.Color && count == displayCount && Mathf.Abs(width - viewportWidth) < .25f && fontKey == currentFont && presentation == currentPresentation && ((presentation & TIMESTAMPS_FLAG) == 0 || time == item.Record.Time);
        }
    }
}
