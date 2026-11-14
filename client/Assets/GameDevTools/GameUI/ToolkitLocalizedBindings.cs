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
                Add(root, text.text.Substring(_prefix.Length), false);
            if (root.tooltip?.StartsWith(_prefix, StringComparison.Ordinal) == true)
                Add(root, root.tooltip.Substring(_prefix.Length), true);
            for (var index = 0; index < root.hierarchy.childCount; index++) Bind(root.hierarchy[index]);
        }

        /// <summary> 固定槽位改为业务数据后解除文本绑定, tooltip 可独立保留. </summary>
        internal static void UnbindText(TextElement element) => _bindings.RemoveAll(binding => !binding.tooltip && binding.target.TryGetTarget(out var target) && target == element);

        /// <summary> 动态标题保留其初始键, 不从旧语言或省略文本推断. </summary>
        internal static string TextKey(TextElement element) => _bindings.Find(binding => !binding.tooltip && binding.target.TryGetTarget(out var target) && target == element)?.key;

        /// <summary> 会话重置时释放旧界面的弱绑定记录. </summary>
        internal static void Reset() => _bindings.Clear();

        /// <summary> 提交语言时只更新已登记的节点, 不逐帧遍历界面. </summary>
        internal static void Refresh()
        {
            for (var index = _bindings.Count - 1; index >= 0; index--)
            {
                var binding = _bindings[index];
                if (!binding.target.TryGetTarget(out var target)) { _bindings.RemoveAt(index); continue; }
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

        // Label, Button 和字段原生 labelElement 都是 TextElement, 不写 TextField.value.
        private static void Apply(VisualElement element, string key, bool tooltip)
        {
            var value = ToolkitLocalization.Get(key);
            if (tooltip) element.tooltip = value;
            else if (element is TextElement text) text.text = value;
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
            /// <summary> 允许窗口和节点被释放. </summary>
            internal readonly WeakReference<VisualElement> target;
            /// <summary> 不依赖翻译后的正文. </summary>
            internal readonly string key;
            /// <summary> 文本和提示分别绑定. </summary>
            internal readonly bool tooltip;
            /// <summary> 保存独立属性的固定语言绑定. </summary>
            internal Binding(VisualElement element, string key, bool tooltip) { target = new(element); this.key = key; this.tooltip = tooltip; }
        }
    }
}
