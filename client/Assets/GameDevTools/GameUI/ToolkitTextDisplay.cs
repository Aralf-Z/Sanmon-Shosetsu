using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 缓存原文与展示结果, 仅在内容或几何变化后执行字体测量. </summary>
    internal sealed class ToolkitTextDisplay : IDisposable
    {
        private readonly Dictionary<TextElement, Entry> _entries = new(); // 每个需要省略的控件只有一个绑定.

        /// <summary> 注册静态或虚拟化控件, 多行文本指定最大高度, truncatedClass 仅在正文被省略时启用. </summary>
        internal void Track(TextElement element, int maxCharacters = int.MaxValue, float maxHeight = float.PositiveInfinity, string truncatedClass = null)
        {
            if (_entries.ContainsKey(element))
                return;
            var entry = new Entry { element = element, source = element.text ?? string.Empty, rendered = element.text ?? string.Empty, maxCharacters = maxCharacters, maxHeight = maxHeight, truncatedClass = truncatedClass };
            entry.geometry = evt => entry.dirty = true;
            element.RegisterCallback(entry.geometry);
            _entries.Add(element, entry);
        }

        /// <summary> 绑定回收按钮时显式更新原文, 不从已经截断的文字恢复数据. </summary>
        internal void SetText(TextElement element, string source)
        {
            ToolkitLocalizedBindings.UnbindText(element);
            if (!_entries.TryGetValue(element, out var entry))
                throw new InvalidOperationException(ToolkitLocalization.Get("ui.TextDisplay.SetText"));
            source ??= string.Empty;
            if (entry.source == source && element.text == entry.rendered)
                return;
            entry.source = source;
            entry.dirty = true;
        }

        /// <summary> 销毁虚拟化行时清除几何监听和缓存. </summary>
        internal void Untrack(TextElement element)
        {
            if (!_entries.TryGetValue(element, out var entry))
                return;
            element.UnregisterCallback(entry.geometry);
            if (entry.truncatedClass != null) element.RemoveFromClassList(entry.truncatedClass);
            _entries.Remove(element);
        }

        /// <summary> 每帧合并外部文本更新, 隐藏或未布局的控件不测量. </summary>
        internal void Tick()
        {
            foreach (var entry in _entries.Values)
            {
                var element = entry.element;
                if (entry.languageVersion != ToolkitLocalization.Version)
                {
                    var key = ToolkitLocalizedBindings.TextKey(element);
                    if (key != null) entry.source = ToolkitLocalization.Get(key);
                    entry.languageVersion = ToolkitLocalization.Version;
                    entry.dirty = true;
                }
                if (element.text != entry.rendered)
                {
                    entry.source = element.text ?? string.Empty;
                    entry.dirty = true;
                }
                var fontKey = ToolkitFontMetrics.GetKey(element);
                if (entry.fontKey != fontKey)
                {
                    entry.fontKey = fontKey;
                    entry.dirty = true;
                }
                if (!entry.dirty || element.panel == null || element.contentRect.width <= 0 || float.IsNaN(element.contentRect.width) || float.IsInfinity(element.contentRect.width) || element.resolvedStyle.display == DisplayStyle.None)
                    continue;
                var text = float.IsPositiveInfinity(entry.maxHeight) ? entry.source.Replace('\r', ' ').Replace('\n', ' ') : entry.source;
                entry.rendered = ToolkitTextEllipsis.Fit(text, element.contentRect.width, entry.maxHeight, candidate => ToolkitTextEllipsis.Measure(element, candidate, element.contentRect.width, !float.IsPositiveInfinity(entry.maxHeight)), element.enableRichText, entry.maxCharacters);
                element.text = entry.rendered;
                if (entry.truncatedClass != null)
                    element.EnableInClassList(entry.truncatedClass, entry.rendered != text);
                entry.dirty = false;
            }
        }

        /// <summary> 页面释放后不保留文本控件引用. </summary>
        public void Dispose()
        {
            foreach (var entry in _entries.Values)
            {
                entry.element.UnregisterCallback(entry.geometry);
                if (entry.truncatedClass != null) entry.element.RemoveFromClassList(entry.truncatedClass);
            }
            _entries.Clear();
        }

        // 原文缓存独立于控件的 text, 几何变化后可以恢复更长的内容.
        private sealed class Entry
        {
            /// <summary> 实际显示控件. </summary>
            internal TextElement element;
            /// <summary> 未省略的原始展示数据. </summary>
            internal string source;
            /// <summary> 最后一次写入控件的结果. </summary>
            internal string rendered;
            /// <summary> 预设或宏名称的正文字符上限. </summary>
            internal int maxCharacters;
            /// <summary> 仅正文被截断时启用的展示样式, 为空时不修改控件样式类. </summary>
            internal string truncatedClass;
            /// <summary> 多行状态提示的可用高度. </summary>
            internal float maxHeight;
            /// <summary> 同一帧只处理最终状态. </summary>
            internal bool dirty = true;
            /// <summary> 可注销的尺寸变化回调. </summary>
            internal EventCallback<GeometryChangedEvent> geometry;
            /// <summary> 字体资源或排版样式变化后重新测量完整原文. </summary>
            internal int fontKey;
            /// <summary> 相同省略结果也不能保留旧语言的完整源文. </summary>
            internal int languageVersion = -1;
        }
    }
}
