using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 模板固定文案的弱引用绑定, 不扫描或替换用户输入值. </summary>
    internal static class ToolkitLocalizedBindings
    {
        private const string _prefix = "@loc:";

        private static readonly List<Binding> _bindings = new(); // 不延长回收节点或整个窗口的生命周期.

        /// <summary> 在业务绑定前捕获 UXML 标记, 后续不会把用户文字当作键. </summary>
        internal static void Bind(VisualElement root)
        {
            if (root == null) return;

            if (root is TextElement text && !IsEditableText(text) && text.text?.StartsWith(_prefix, StringComparison.Ordinal) == true)
            {
                Add(root, text.text.Substring(_prefix.Length), false);
            }

            if (root.tooltip?.StartsWith(_prefix, StringComparison.Ordinal) == true)
            {
                Add(root, root.tooltip.Substring(_prefix.Length), true);
            }

            for (var index = 0; index < root.hierarchy.childCount; index++)
            {
                Bind(root.hierarchy[index]);
            }
        }

        /// <summary> 固定槽位改为业务数据后解除文本绑定, tooltip 可独立保留. </summary>
        internal static void UnbindText(TextElement element) => _bindings.RemoveAll(binding => !binding.tooltip && binding.target.TryGetTarget(out var target) && target == element);

        /// <summary> 动态标题保留其初始键, 不从旧语言或省略文本推断. </summary>
        internal static string TextKey(TextElement element) => _bindings.Find(binding => !binding.tooltip && binding.target.TryGetTarget(out var target) && target == element)?.key;

        internal static void Reset() => _bindings.Clear();

        internal static void Refresh()
        {
            for (var index = _bindings.Count - 1; index >= 0; index--)
            {
                var binding = _bindings[index];

                if (!binding.target.TryGetTarget(out var target))
                {
                    _bindings.RemoveAt(index);
                    continue;
                }

                Apply(target, binding.key, binding.tooltip);
            }
        }

        // 同一节点和属性只保留一次绑定.
        private static void Add(VisualElement element, string key, bool tooltip)
        {
            _bindings.RemoveAll(binding => !binding.target.TryGetTarget(out var target) || target == element && binding.tooltip == tooltip);
            _bindings.Add(new Binding(element, key, tooltip));
            Apply(element, key, tooltip);
        }

        private static void Apply(VisualElement element, string key, bool tooltip)
        {
            var value = ToolkitLocalization.Get(key);

            if (tooltip)
            {
                element.tooltip = ToolkitIcons.PlainText(value);
            }
            else if (element is TextElement text)
            {
                ToolkitMixedText.SetText(text, value);
            }
        }

        // 字段标签可以本地化, 输入文本即使恰好带有标记也不能绑定.
        private static bool IsEditableText(TextElement element)
        {
            for (var parent = element.parent; parent != null; parent = parent.parent)
            {
                if (parent.ClassListContains("unity-base-text-field__input") || parent.ClassListContains("unity-text-input")) return true;

                if (parent is TextField field) return element != field.labelElement;
            }

            return false;
        }

        private sealed class Binding
        {
            internal readonly WeakReference<VisualElement> target;

            internal readonly string key;

            internal readonly bool tooltip;

            internal Binding(VisualElement element, string key, bool tooltip)
            {
                target = new(element);
                this.key = key;
                this.tooltip = tooltip;
            }
        }
    }
}
