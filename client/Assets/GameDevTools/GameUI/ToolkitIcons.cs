using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 图标按风格加载普通 Sprite, 本地化只保存图文组件识别的文件键. </summary>
    internal static class ToolkitIcons
    {
        private const string ICON_ROOT = "Icon/";

        internal static readonly Regex tokens = new(@"<icon=([A-Za-z0-9_-]+)>", RegexOptions.Compiled);

        private static readonly Dictionary<string, Sprite> _cache = new(StringComparer.Ordinal);

        internal static string Token(string key)
        {
            if (string.IsNullOrEmpty(key) || !key.All(IsKeyCharacter)) return key;

            return "<icon=" + key + ">";
        }

        internal static string Resolve(string text)
        {
            return tokens.Replace(text, ResolveToken);
        }

        internal static string PlainText(string text)
        {
            return tokens.Replace(text ?? string.Empty, "$1");
        }

        /// <summary> 使用已提交风格, 缺失时回退 Shared, 两处均缺失返回 null. </summary>
        internal static Sprite Load(string key)
        {
            var folder = ToolkitFactory.ActiveStyle ?? string.Empty;

            if (folder.StartsWith("Style/", StringComparison.Ordinal))
            {
                folder = folder.Substring("Style/".Length);
            }

            var path = ICON_ROOT + folder + "/" + key;

            if (_cache.TryGetValue(path, out var cached)) return cached;

            var sprite = Resources.Load<Sprite>(path);

            if (sprite == null)
            {
                sprite = Resources.Load<Sprite>(ICON_ROOT + "Shared/" + key);
            }

            _cache[path] = sprite;
            return sprite;
        }

        internal static int[] TextBoundaries(string text)
        {
            var starts = StringInfo.ParseCombiningCharacters(text);
            var matches = tokens.Matches(text)
                .Cast<Match>()
                .ToArray();

            return starts.Where(start => !matches.Any(match => start > match.Index && start < match.Index + match.Length))
                .Concat(new[] { text.Length })
                .ToArray();
        }

        /// <summary> 原生菜单保留原交互, 只替换菜单标签的图文展示. </summary>
        internal static void EnableMenuIcons(VisualElement root)
        {
            var labels = root.Query<Label>()
                .ToList();

            foreach (var label in labels)
            {
                if (tokens.IsMatch(label.text ?? string.Empty))
                {
                    ToolkitMixedText.SetText(label, label.text);
                }
            }
        }

        private static bool IsKeyCharacter(char character)
        {
            return character >= 'A' && character <= 'Z' || character >= 'a' && character <= 'z' || character >= '0' && character <= '9' || character == '_' || character == '-';
        }

        private static string ResolveToken(Match match)
        {
            var key = match.Groups[1].Value;

            if (Load(key) == null) return key;

            return match.Value;
        }

        // 关闭 Domain Reload 时也不保留旧界面和上一次运行的缺失结果.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession()
        {
            ToolkitMixedText.Reset();
            _cache.Clear();
        }
    }
}
