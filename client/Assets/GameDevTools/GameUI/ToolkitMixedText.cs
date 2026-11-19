using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> Auto 识别纯图标按钮, Inline 跟随字号, Control 使用固定图标框. </summary>
    internal enum ToolkitMixedTextMode
    {
        Auto,
        Inline,
        Control
    }

    /// <summary> 在原文本控件内组合 Image 和 Label, 保留按钮及字段标签的原有交互. </summary>
    internal static partial class ToolkitMixedText
    {
        /// <summary> 所有纯图标控件共用的 Image 边长, 单位为面板像素. </summary>
        internal const float CONTROL_ICON_SIZE = 24f;

        private const string CONTROL_CLASS = "toolkit-icon-control";

        private const string INDICATOR_CLASS = "toolkit-icon-indicator";
        private const int GLYPH_BATCH_SIZE = 4;

        private static readonly ConditionalWeakTable<TextElement, Binding> _bindings = new(); // 控件回收后不延长其生命周期.

        private static readonly List<WeakReference<Binding>> _live = new();

        internal static void SetText(TextElement element, string source, ToolkitMixedTextMode mode = ToolkitMixedTextMode.Auto, ToolkitInlineLayout.Result preparedLayout = null)
        {
            source ??= string.Empty;
            if (!ToolkitIcons.tokens.IsMatch(source))
            {
                Unbind(element);
                element.text = source;
                return;
            }

            if (!_bindings.TryGetValue(element, out var binding))
            {
                binding = new Binding(element);
                _bindings.Add(element, binding);
                _live.Add(new WeakReference<Binding>(binding));
            }

            binding.SetText(source, mode, preparedLayout);
        }

        internal static string GetText(TextElement element)
        {
            if (_bindings.TryGetValue(element, out var binding) && string.IsNullOrEmpty(element.text)) return binding.Source;

            return element.text ?? string.Empty;
        }

        internal static void SetFoldoutText(Foldout foldout, string source)
        {
            foldout.text = ToolkitIcons.PlainText(source);
            var label = foldout.Q<TextElement>(className: "unity-toggle__text");

            if (label != null)
            {
                SetText(label, source, ToolkitMixedTextMode.Inline);
            }
        }

        internal static Vector2 Measure(TextElement element, string text, float width, bool wrap)
        {
            text = ToolkitIcons.Resolve(text);
            if (_bindings.TryGetValue(element, out var binding) && binding.IsControl && IsSingleIcon(text)) return new Vector2(CONTROL_ICON_SIZE, CONTROL_ICON_SIZE);

            if (ToolkitIcons.tokens.IsMatch(text)) return BuildLayout(element, text, width, wrap).Size;

            return element.MeasureTextSize(text, wrap ? width : 0, wrap ? VisualElement.MeasureMode.Exactly : VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
        }

        internal static void RefreshAll()
        {
            Visit(binding => binding.Refresh());
        }

        internal static void Release(VisualElement root)
        {
            if (root == null) return;

            Visit(binding =>
            {
                if (binding.Element == root || root.Contains(binding.Element))
                {
                    Unbind(binding.Element);
                }
            });
        }

        /// <summary> 关闭 Domain Reload 时清理上一次运行的图文节点. </summary>
        internal static void Reset()
        {
            Visit(binding => Unbind(binding.Element));
            _live.Clear();
        }

        internal static IEnumerable<int> MeasureHeightSteps(TextElement element, string text, float width, float availableHeight, IDictionary<string, float> widths, HashSet<string> warmedGlyphs, Action<float, ToolkitInlineLayout.Result> complete)
        {
            text = ToolkitIcons.Resolve(text);
            if (!ToolkitIcons.tokens.IsMatch(text))
            {
                foreach (var step in WarmGlyphs(element, text, warmedGlyphs))
                {
                    yield return step;
                }

                var size = element.MeasureTextSize(text, width, VisualElement.MeasureMode.Exactly, 0, VisualElement.MeasureMode.Undefined);

                complete(size.y, null);
                yield return 0;
                yield break;
            }

            var fontSize = LayoutFontSize(element);
            var line = NativeMeasure(element, "Mg", fontSize);
            var layout = new ToolkitInlineLayout(width, fontSize, line.y, value => CachedWidth(element, value, fontSize, widths), true);

            yield return 0;
            foreach (var step in layout.Append(text))
            {
                yield return step;
                if (layout.Size.y > availableHeight)
                {
                    complete(layout.Size.y, null);
                    yield break;
                }
            }

            layout.AlignRows(element.resolvedStyle.unityTextAlign);
            complete(layout.Size.y, new ToolkitInlineLayout.Result(layout.Size, layout.parts));
        }

        // 通过正文自身的字体及后备链，每步只生成少量冷字形，再做整段原生换行测量。
        private static IEnumerable<int> WarmGlyphs(TextElement element, string text, HashSet<string> warmedGlyphs)
        {
            var characters = element.resolvedStyle.unityFontDefinition.fontAsset?.characterLookupTable;
            var elements = StringInfo.GetTextElementEnumerator(text);
            var batch = new StringBuilder();
            var count = 0;

            while (elements.MoveNext())
            {
                var glyph = elements.GetTextElement();
                if (string.IsNullOrWhiteSpace(glyph) || (glyph.Length == 1 && characters?.ContainsKey(glyph[0]) == true) || !warmedGlyphs.Add(glyph)) continue;
                batch.Append(glyph);
                count++;
                if (count < GLYPH_BATCH_SIZE) continue;

                element.MeasureTextSize(batch.ToString(), 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                batch.Clear();
                count = 0;
                yield return 0;
            }

            if (count == 0) yield break;
            element.MeasureTextSize(batch.ToString(), 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
            yield return 0;
        }

        private static float CachedWidth(TextElement element, string text, float fontSize, IDictionary<string, float> widths)
        {
            if (widths.TryGetValue(text, out var width)) return width;

            width = NativeMeasure(element, text, fontSize).x;
            widths[text] = width;
            return width;
        }

        private static float LayoutFontSize(TextElement element)
        {
            var fontSize = element.resolvedStyle.fontSize;

            return Finite(fontSize) && fontSize > 0 ? fontSize : 14f;
        }

        private static ToolkitInlineLayout BuildLayout(TextElement element, string text, float width, bool wrap)
        {
            var fontSize = LayoutFontSize(element);
            var line = NativeMeasure(element, "Mg", fontSize);

            return ToolkitInlineLayout.Build(text, width, fontSize, line.y, value => NativeMeasure(element, value, fontSize).x, wrap);
        }

        private static Vector2 NativeMeasure(TextElement element, string text, float fontSize)
        {
            var measured = element.MeasureTextSize(text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);

            if (Finite(measured.x) && Finite(measured.y) && measured.y > 0) return measured;

            return new Vector2(text.Length * fontSize * .5f, fontSize);
        }

        private static bool Finite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static bool IsSingleIcon(string source)
        {
            var text = source.Trim();
            var match = ToolkitIcons.tokens.Match(text);

            return match.Success && match.Index == 0 && match.Length == text.Length;
        }

        private static VisualElement FindControl(TextElement element)
        {
            if (element is Button) return element;

            for (var parent = element.parent; parent != null; parent = parent.parent)
            {
                if (parent is TextField) return null;

                if (parent is Toggle) return parent;
            }

            return null;
        }

        private static void Unbind(TextElement element)
        {
            if (!_bindings.TryGetValue(element, out var binding)) return;

            _bindings.Remove(element);
            binding.Dispose();
        }

        private static void Visit(Action<Binding> action)
        {
            for (var index = _live.Count - 1; index >= 0; index--)
            {
                if (!_live[index].TryGetTarget(out var binding) || binding.IsDisposed)
                {
                    _live.RemoveAt(index);
                    continue;
                }

                action(binding);
            }
        }
    }
}
