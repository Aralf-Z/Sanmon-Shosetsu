using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace GameConsole.UI
{
    /// <summary> 图文共用的行布局, 测量与实际 Image 和 Label 节点使用同一份结果. </summary>
    internal sealed class ToolkitInlineLayout
    {
        /// <summary> 显式换行总是保留, 自动换行只在 wrap 开启且宽度有效时发生. </summary>
        internal static ToolkitInlineLayout Build(string source, float width, float fontSize, float lineHeight, Func<string, float> measure, bool wrap)
        {
            var layout = new ToolkitInlineLayout(width, fontSize, lineHeight, measure, wrap);

            using var steps = layout.Append(source).GetEnumerator();
            while (steps.MoveNext())
            {
            }

            return layout;
        }

        internal IEnumerable<int> Append(string source)
        {
            var offset = 0;

            foreach (System.Text.RegularExpressions.Match match in ToolkitIcons.tokens.Matches(source))
            {
                foreach (var step in AppendText(source.Substring(offset, match.Index - offset)))
                {
                    yield return step;
                }

                AppendIcon(match.Groups[1].Value);
                offset = match.Index + match.Length;
                yield return 0;
            }

            foreach (var step in AppendText(source.Substring(offset)))
            {
                yield return step;
            }
        }

        private static bool IsWordCharacter(char character)
        {
            return character >= 'A' && character <= 'Z' || character >= 'a' && character <= 'z' || character >= '0' && character <= '9' || character == '_';
        }

        internal readonly List<Part> parts = new();

        /// <summary> 内容实际占用的宽高, 单位为面板像素. </summary>
        internal Vector2 Size => new(_maximumWidth, _lineTop + _lineHeight);

        private readonly Func<string, float> _measure;

        private readonly float _width;

        private readonly float _fontSize;

        private readonly float _lineHeight;

        private Part _text;

        private float _lineWidth;

        private float _maximumWidth;

        private float _lineTop;

        internal ToolkitInlineLayout(float width, float fontSize, float lineHeight, Func<string, float> measure, bool wrap)
        {
            _width = wrap && width > 0 ? width : float.PositiveInfinity;
            _fontSize = fontSize;
            _lineHeight = Mathf.Max(fontSize, lineHeight);
            _measure = measure;
        }

        internal void AlignRows(TextAnchor anchor)
        {
            var alignment = (int)anchor % 3;

            if (alignment == 0) return;

            var widths = new Dictionary<int, float>();

            foreach (var part in parts)
            {
                var row = Mathf.FloorToInt(part.rectangle.y / _lineHeight);

                widths.TryGetValue(row, out var width);
                widths[row] = Mathf.Max(width, part.rectangle.xMax);
            }

            foreach (var part in parts)
            {
                var row = Mathf.FloorToInt(part.rectangle.y / _lineHeight);

                part.rectangle.x += (_maximumWidth - widths[row]) * alignment * .5f;
            }
        }

        private IEnumerable<int> AppendText(string text)
        {
            var word = new StringBuilder();
            var normalized = text.Replace("\r\n", "\n")
                .Replace('\r', '\n')
                .Replace("\t", "    ");
            var elements = StringInfo.GetTextElementEnumerator(normalized);

            while (elements.MoveNext())
            {
                var element = elements.GetTextElement();

                if (element.Length == 1 && IsWordCharacter(element[0]))
                {
                    word.Append(element);
                    continue;
                }

                if (word.Length > 0)
                {
                    foreach (var step in FlushWord(word))
                    {
                        yield return step;
                    }
                }

                if (element == "\n")
                {
                    NewLine();
                }
                else
                {
                    AppendUnit(element);
                }

                yield return 0;
            }

            if (word.Length == 0) yield break;

            foreach (var step in FlushWord(word))
            {
                yield return step;
            }
        }

        private IEnumerable<int> FlushWord(StringBuilder word)
        {
            var text = word.ToString();

            word.Clear();
            if (_measure(text) <= _width)
            {
                AppendUnit(text);
                yield return 0;
                yield break;
            }

            var elements = StringInfo.GetTextElementEnumerator(text);

            while (elements.MoveNext())
            {
                AppendUnit(elements.GetTextElement());
                yield return 0;
            }
        }

        // 连续文本使用整个前缀测量, 保留字体字距和组合字符的实际宽度.
        private void AppendUnit(string unit)
        {
            var prefix = _text?.text ?? string.Empty;
            var oldWidth = _text?.rectangle.width ?? 0f;
            var newWidth = _measure(prefix + unit);

            if (_lineWidth > 0 && _lineWidth - oldWidth + newWidth > _width + .01f)
            {
                NewLine();
                newWidth = _measure(unit);
            }

            if (_text == null)
            {
                _text = new Part
                { rectangle = new Rect(_lineWidth, _lineTop, 0, _lineHeight) };
                parts.Add(_text);
            }

            _text.text += unit;
            _text.rectangle.width = newWidth;
            _lineWidth = _text.rectangle.x + newWidth;
            _maximumWidth = Mathf.Max(_maximumWidth, _lineWidth);
        }

        private void AppendIcon(string key)
        {
            if (_lineWidth > 0 && _lineWidth + _fontSize > _width + .01f)
            {
                NewLine();
            }

            var top = _lineTop + (_lineHeight - _fontSize) * .5f;

            parts.Add(new Part { icon = key, rectangle = new Rect(_lineWidth, top, _fontSize, _fontSize) });
            _lineWidth += _fontSize;
            _maximumWidth = Mathf.Max(_maximumWidth, _lineWidth);
            _text = null;
        }

        private void NewLine()
        {
            _lineTop += _lineHeight;
            _lineWidth = 0;
            _text = null;
        }

        internal sealed class Result
        {
            internal readonly Vector2 size;
            internal readonly IReadOnlyList<Part> parts;

            internal Result(Vector2 size, IReadOnlyList<Part> parts)
            {
                this.size = size;
                this.parts = parts;
            }
        }

        /// <summary> 一段连续文本或一个图标的显示区域. </summary>
        internal sealed class Part
        {
            internal string text = string.Empty;

            internal string icon;

            internal Rect rectangle;
        }
    }
}
