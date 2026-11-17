using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 使用三个英文句点生成展示文本, 不修改原始数据或截断富文本标签. </summary>
    internal static class ToolkitTextEllipsis
    {
        private const string _suffix = "...";

        private static readonly Regex _tags = new(@"<(/?)([a-zA-Z]+)(?:=[^>]*|\s[^>]*)?>", RegexOptions.Compiled);

        internal static Vector2 Measure(TextElement element, string text, float width, bool wrap)
        {
            return ToolkitMixedText.Measure(element, text, width, wrap);
        }

        internal static string Fit(string source, float width, float height, Func<string, Vector2> measure, bool richText = false, int maxCharacters = int.MaxValue, bool forceEllipsis = false)
        {
            source ??= string.Empty;
            if (source.Length == 0 && !forceEllipsis) return source;

            var tags = richText
                ? _tags
                : ToolkitIcons.tokens;
            var visible = tags.Replace(source, match => ToolkitIcons.tokens.IsMatch(match.Value) ? "\uFFFC" : string.Empty);
            var indices = StringInfo.ParseCombiningCharacters(visible);

            if (!forceEllipsis && indices.Length <= maxCharacters && Fits(measure(source), width, height)) return source;

            if (!Fits(measure(_suffix), width, height)) return string.Empty;

            var low = 0;
            var high = Math.Min(indices.Length, Math.Max(0, maxCharacters));

            while (low < high)
            {
                var middle = low + (high - low + 1) / 2;
                var prefix = Prefix(source, visible, indices, middle, tags);

                if (Fits(measure(prefix + _suffix), width, height))
                {
                    low = middle;
                }
                else
                {
                    high = middle - 1;
                }
            }

            return Prefix(source, visible, indices, low, tags) + _suffix;
        }

        // 误差只容忍字体测量的小数舍入, 不允许省略号超出边界.
        private static bool Fits(Vector2 size, float width, float height) => size.x <= width + 0.01f && size.y <= height + 0.01f;

        private static string Prefix(string source, string visible, int[] indices, int count, Regex tags)
        {
            var length = count >= indices.Length
                ? visible.Length
                : indices[count];
            var result = new StringBuilder();
            var open = new List<string>();
            var offset = 0;
            var consumed = 0;

            foreach (Match match in tags.Matches(source))
            {
                var take = Math.Min(length - consumed, match.Index - offset);

                if (take > 0)
                {
                    result.Append(source, offset, take);
                }

                consumed += take;
                if (consumed >= length)
                {
                    break;
                }

                result.Append(match.Value);
                if (ToolkitIcons.tokens.IsMatch(match.Value))
                {
                    consumed++;
                }
                else
                {
                    var name = match.Groups[2].Value.ToLowerInvariant();

                    if (match.Groups[1].Value.Length == 0)
                    {
                        open.Add(name);
                    }
                    else if (open.Count > 0 && open[open.Count - 1] == name)
                    {
                        open.RemoveAt(open.Count - 1);
                    }
                }

                offset = match.Index + match.Length;
            }

            if (consumed < length)
            {
                result.Append(source, offset, length - consumed);
            }

            for (var i = open.Count - 1; i >= 0; i--)
            {
                result.Append("</")
                    .Append(open[i])
                    .Append('>');
            }

            return result.ToString()
                .TrimEnd();
        }
    }
}
