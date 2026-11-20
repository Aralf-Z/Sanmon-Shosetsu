using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 缓存原文与展示结果, 仅在内容或几何变化后执行字体测量. </summary>
    internal sealed class ToolkitTextDisplay : IDisposable
    {
        private readonly Dictionary<TextElement, Entry> _entries = new();

        /// <summary> 注册静态或虚拟化控件, 多行文本指定最大高度, truncatedClass 仅在正文被省略时启用. </summary>
        internal void Track(TextElement element, int maxCharacters = int.MaxValue, float maxHeight = float.PositiveInfinity, string truncatedClass = null)
        {
            if (_entries.ContainsKey(element)) return;

            var source = ToolkitMixedText.GetText(element);
            var entry = new Entry
            { element = element, source = source, rendered = source, maxCharacters = maxCharacters, maxHeight = maxHeight, truncatedClass = truncatedClass };

            entry.geometry = evt => entry.dirty = true;
            element.RegisterCallback(entry.geometry);
            _entries.Add(element, entry);
        }

        internal void SetText(TextElement element, string source)
        {
            ToolkitLocalizedBindings.UnbindText(element);
            if (!_entries.TryGetValue(element, out var entry))
            {
                throw new InvalidOperationException(ToolkitLocalization.Get("ui.TextDisplay.SetText"));
            }

            source ??= string.Empty;
            if (entry.source == source && ToolkitMixedText.GetText(element) == entry.rendered) return;

            entry.source = source;
            entry.dirty = true;
        }

        internal void Clear(TextElement element)
        {
            SetText(element, string.Empty);
            _entries[element].rendered = string.Empty;
            ToolkitMixedText.SetText(element, string.Empty);
        }

        internal void Untrack(TextElement element)
        {
            if (!_entries.TryGetValue(element, out var entry)) return;

            element.UnregisterCallback(entry.geometry);
            if (entry.truncatedClass != null)
            {
                element.RemoveFromClassList(entry.truncatedClass);
            }

            _entries.Remove(element);
        }

        internal void Tick()
        {
            foreach (var entry in _entries.Values)
            {
                var element = entry.element;

                if (entry.languageVersion != ToolkitLocalization.Version)
                {
                    var key = ToolkitLocalizedBindings.TextKey(element);

                    if (key != null)
                    {
                        entry.source = ToolkitLocalization.Get(key);
                    }

                    entry.languageVersion = ToolkitLocalization.Version;
                    entry.dirty = true;
                }

                if (ToolkitMixedText.GetText(element) != entry.rendered)
                {
                    entry.source = ToolkitMixedText.GetText(element);
                    entry.dirty = true;
                }

                var fontKey = ToolkitFontMetrics.GetKey(element);

                if (entry.fontKey != fontKey)
                {
                    entry.fontKey = fontKey;
                    entry.dirty = true;
                }

                if (!entry.dirty || element.panel == null || element.contentRect.width <= 0 || float.IsNaN(element.contentRect.width) || float.IsInfinity(element.contentRect.width) || element.resolvedStyle.display == DisplayStyle.None)
                {
                    continue;
                }

                var text = float.IsPositiveInfinity(entry.maxHeight)
                    ? entry.source.Replace('\r', ' ').Replace('\n', ' ')
                    : entry.source;

                entry.rendered = ToolkitTextEllipsis.Fit(text, element.contentRect.width, entry.maxHeight, candidate => ToolkitTextEllipsis.Measure(element, candidate, element.contentRect.width, !float.IsPositiveInfinity(entry.maxHeight)), element.enableRichText, entry.maxCharacters);
                ToolkitMixedText.SetText(element, entry.rendered);
                if (entry.truncatedClass != null)
                {
                    element.EnableInClassList(entry.truncatedClass, entry.rendered != text);
                }

                entry.dirty = false;
            }
        }

        /// <summary> 页面释放后不保留文本控件引用. </summary>
        public void Dispose()
        {
            foreach (var entry in _entries.Values)
            {
                entry.element.UnregisterCallback(entry.geometry);
                if (entry.truncatedClass != null)
                {
                    entry.element.RemoveFromClassList(entry.truncatedClass);
                }
            }

            _entries.Clear();
        }

        private sealed class Entry
        {
            internal TextElement element;

            internal string source;

            internal string rendered;

            /// <summary> 预设或宏名称的正文字符上限. </summary>
            internal int maxCharacters;

            /// <summary> 仅正文被截断时启用的展示样式, 为空时不修改控件样式类. </summary>
            internal string truncatedClass;

            internal float maxHeight;

            internal bool dirty = true;

            internal EventCallback<GeometryChangedEvent> geometry;

            internal int fontKey;

            /// <summary> 相同省略结果也不能保留旧语言的完整源文. </summary>
            internal int languageVersion = -1;
        }
    }
}
