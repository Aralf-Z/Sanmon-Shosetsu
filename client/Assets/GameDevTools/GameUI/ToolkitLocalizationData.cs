using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using GameConsole.UI;
using UnityEngine;

namespace GameConsole
{
    /// <summary> 读取语言模块并校验词条, 引用及格式占位符. </summary>
    internal static class ToolkitLocalizationData
    {
        private static readonly Regex _placeholder = new(@"(?<!\{)\{([A-Za-z_][A-Za-z0-9_]*)\}(?!\})", RegexOptions.Compiled);

        private static readonly Regex _reference = new(@"(?<!\{)\{([A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z0-9_-]+)+)\}(?!\})", RegexOptions.Compiled);

        internal static Dictionary<string, string> Read(string json, string source)
        {
            try
            {
                var module = JsonUtility.FromJson<LanguageModule>(json);
                var fields = JsonShape.ReadFields(json);
                var entries = JsonShape.Require(fields, "entries", '[');
                var result = new Dictionary<string, string>(module.entries.Length, StringComparer.Ordinal);

                foreach (var item in JsonShape.ReadItems(entries))
                {
                    var entryFields = JsonShape.ReadFields(item);

                    JsonShape.Require(entryFields, "key", '"');
                    JsonShape.Require(entryFields, "value", '"');
                }

                foreach (var entry in module.entries)
                {
                    if (string.IsNullOrWhiteSpace(entry.key))
                    {
                        throw new FormatException("Empty localization key.");
                    }

                    if (!result.TryAdd(entry.key, entry.value))
                    {
                        throw new FormatException("Duplicate localization key: " + entry.key);
                    }
                }

                return result;
            }
            catch (Exception exception)
            {
                throw new FormatException(source + ": " + exception.Message, exception);
            }
        }

        /// <summary> 每个语言包内各模块的键必须唯一. </summary>
        internal static void AddModule(Dictionary<string, string> target, Dictionary<string, string> module, string source)
        {
            foreach (var pair in module)
            {
                if (!target.TryAdd(pair.Key, pair.Value))
                {
                    throw new FormatException(source + ": duplicate localization key " + pair.Key);
                }
            }
        }

        internal static void Validate(Dictionary<string, string> values, IReadOnlyDictionary<string, string> fallback)
        {
            var resolved = ResolveReferences(values, fallback);
            var defaults = ResolveReferences(fallback);

            foreach (var pair in values)
            {
                if (defaults.TryGetValue(pair.Key, out var original) && !Placeholders(resolved[pair.Key])
                    .SetEquals(Placeholders(original)))
                {
                    throw new FormatException("Localization placeholder mismatch: " + pair.Key);
                }
            }
        }

        /// <summary> 普通词条只替换一层引用, 图标保留展示标记, 缺失时保留原占位符. </summary>
        internal static Dictionary<string, string> ResolveReferences(IReadOnlyDictionary<string, string> values, IReadOnlyDictionary<string, string> fallback = null)
        {
            var source = new Dictionary<string, string>(StringComparer.Ordinal);

            if (fallback != null)
            {
                foreach (var pair in fallback)
                {
                    source[pair.Key] = pair.Value;
                }
            }

            foreach (var pair in values)
            {
                source[pair.Key] = pair.Value;
            }

            var resolved = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var pair in source)
            {
                resolved[pair.Key] = pair.Key.StartsWith("icon.", StringComparison.Ordinal)
                    ? ToolkitIcons.Token(pair.Value)
                    : _reference.Replace(pair.Value, match => ResolveReference(source, match));
            }

            return resolved;
        }

        /// <summary> 单次替换, 参数中的文本不会再次作为占位符解析. </summary>
        internal static string Format(string source, params (string name, object value)[] arguments)
        {
            var values = new Dictionary<string, object>(StringComparer.Ordinal);

            foreach (var argument in arguments)
            {
                values[argument.name] = argument.value;
            }

            return Format(source, values);
        }

        /// <summary> 复用固定参数表, 未提供的占位符保留给后续显示参数. </summary>
        internal static string Format(string source, IReadOnlyDictionary<string, object> values)
        {
            return _placeholder.Replace(source, match => values.TryGetValue(match.Groups[1].Value, out var value) ? Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty : match.Value);
        }

        // 保持原有单层词条引用规则, 图标保留独立标记直到展示时再按风格解析.
        private static string ResolveReference(IReadOnlyDictionary<string, string> source, Match match)
        {
            var key = match.Groups[1].Value;

            if (!source.TryGetValue(key, out var value)) return match.Value;

            if (key.StartsWith("icon.", StringComparison.Ordinal)) return ToolkitIcons.Token(value);

            return value;
        }

        // 同名占位符重复出现不改变模板签名.
        private static HashSet<string> Placeholders(string text) => _placeholder.Matches(text)
            .Cast<Match>()
            .Select(match => match.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);

        [Serializable]
        private sealed class LanguageModule
        {
            public LanguageEntry[] entries;
        }

        [Serializable]
        private sealed class LanguageEntry
        {
            public string key;

            public string value;
        }
    }
}
