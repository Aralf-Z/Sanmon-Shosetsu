using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace GameConsole
{
    /// <summary> JSON 校验和格式化只处理数据, 不访问 Unity 或执行命令. </summary>
    internal static class ToolkitLocalizationData
    {
        private static readonly Regex _placeholder = new(@"(?<!\{)\{([A-Za-z_][A-Za-z0-9_]*)\}(?!\})", RegexOptions.Compiled); // 只识别命名占位符.
        private static readonly Regex _reference = new(@"(?<!\{)\{([A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z0-9_-]+)+)\}(?!\})", RegexOptions.Compiled); // 带点的占位符引用语言词条, 与运行时参数分开处理.

        /// <summary> 拒绝重复键和非字符串值, 不静默覆盖错误定义. </summary>
        internal static Dictionary<string, string> Read(string json, string source)
        {
            try
            {
                var result = new Dictionary<string, string>(StringComparer.Ordinal);
                var root = JObject.Parse(json, new JsonLoadSettings { DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error });
                foreach (var property in root.Properties())
                {
                    if (string.IsNullOrWhiteSpace(property.Name) || property.Value.Type != JTokenType.String)
                        throw new FormatException("Localization keys must have string values: " + property.Name);
                    result.Add(property.Name, property.Value.Value<string>());
                }
                return result;
            }
            catch (Exception exception) { throw new FormatException(source + ": " + exception.Message, exception); }
        }

        /// <summary> 每个语言包内各模块的键必须唯一. </summary>
        internal static void AddModule(Dictionary<string, string> target, Dictionary<string, string> module, string source)
        {
            foreach (var pair in module)
                if (!target.TryAdd(pair.Key, pair.Value)) throw new FormatException(source + ": duplicate localization key " + pair.Key);
        }

        /// <summary> 替换词条引用后校验运行时参数, 翻译参数与默认语言保持一致. </summary>
        internal static void Validate(Dictionary<string, string> values, IReadOnlyDictionary<string, string> fallback)
        {
            var resolved = ResolveReferences(values, fallback);
            var defaults = ResolveReferences(fallback);
            foreach (var pair in values)
                if (defaults.TryGetValue(pair.Key, out var original) && !Placeholders(resolved[pair.Key]).SetEquals(Placeholders(original)))
                    throw new FormatException("Localization placeholder mismatch: " + pair.Key);
        }

        /// <summary> 单层取用词条原值, 不解析引用值中的占位符, 缺失时保留原占位符. </summary>
        internal static Dictionary<string, string> ResolveReferences(IReadOnlyDictionary<string, string> values, IReadOnlyDictionary<string, string> fallback = null)
        {
            var source = new Dictionary<string, string>(StringComparer.Ordinal);
            if (fallback != null)
                foreach (var pair in fallback) source[pair.Key] = pair.Value;
            foreach (var pair in values) source[pair.Key] = pair.Value;
            var resolved = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var pair in source)
                resolved[pair.Key] = _reference.Replace(pair.Value, match => source.TryGetValue(match.Groups[1].Value, out var value) ? value : match.Value);
            return resolved;
        }

        /// <summary> 单次替换, 参数中的文本不会再次作为占位符解析. </summary>
        internal static string Format(string source, params (string name, object value)[] arguments)
        {
            var values = new Dictionary<string, object>(StringComparer.Ordinal);
            foreach (var argument in arguments) values[argument.name] = argument.value;
            return Format(source, values);
        }

        /// <summary> 复用固定参数表, 未提供的占位符保留给后续显示参数. </summary>
        internal static string Format(string source, IReadOnlyDictionary<string, object> values)
        {
            return _placeholder.Replace(source, match => values.TryGetValue(match.Groups[1].Value, out var value) ? Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty : match.Value);
        }

        // 同名占位符重复出现不改变模板签名.
        private static HashSet<string> Placeholders(string text) => _placeholder.Matches(text).Cast<Match>().Select(match => match.Groups[1].Value).ToHashSet(StringComparer.Ordinal);
    }
}
