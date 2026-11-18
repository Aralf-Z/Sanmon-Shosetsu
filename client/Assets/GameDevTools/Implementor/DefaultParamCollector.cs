using System.Collections.Generic;
using GameConsole.Extension;

namespace GameConsole.Implementor
{
    /// <summary> Default param collector. </summary>
    public static partial class DefaultParamCollector
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary> 命令与预设共用的超长标签, 用于检查完整匹配和弹窗换行. </summary>
        internal const string TAG_TEST_LONG_NAME = "超长标签/用于验证更多弹窗中的自动换行和完整名称/LongTagWithoutTruncationABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789/日本語/한국어/末尾仍然能够选择";

        private const int TAG_TEST_TAG_COUNT = 96;

        /// <summary> 含重复, 空白, 大小写, 逗号, 首尾空格及长名称的综合预设数据. </summary>
        public static List<ParamDefine> GetTagTestPresets()
        {
            var presets = GetUntaggedTestPresets();
            var keywords = new[] { "alpha", "beta", "gamma" };

            presets.Add(CreateTagTestPreset("duplicate_alpha", "重复标签 alpha", "重复标签"));
            presets.Add(CreateTagTestPreset("duplicate_beta", "重复标签 beta", "重复标签"));
            presets.Add(CreateTagTestPreset("combined_alpha", "完整逗号标签 alpha", "组合,A"));
            presets.Add(CreateTagTestPreset("single_alpha", "逗号标签对照 alpha", "组合"));
            presets.Add(CreateTagTestPreset("uppercase_alpha", "大写标签 alpha", "TagCase"));
            presets.Add(CreateTagTestPreset("lowercase_beta", "小写标签 beta", "tagcase"));
            presets.Add(CreateTagTestPreset("padded_alpha", "保留首尾空格 alpha", " 标签空格 "));
            presets.Add(CreateTagTestPreset("unpadded_beta", "首尾空格对照 beta", "标签空格"));
            presets.Add(CreateTagTestPreset("long_gamma", "超长标签 gamma", TAG_TEST_LONG_NAME));
            for (var tagIndex = 1; tagIndex <= TAG_TEST_TAG_COUNT; tagIndex++)
            {
                foreach (var keyword in keywords)
                {
                    var tag = $"预设标签/{tagIndex:00}";
                    var value = $"group_{tagIndex:00}_{keyword}";

                    presets.Add(CreateTagTestPreset(value, $"预设 {tagIndex:00} / {keyword}", tag));
                }
            }

            return presets;
        }

        /// <summary> 无有效标签的预设, 标签栏应仅显示全部. </summary>
        public static List<ParamDefine> GetUntaggedTestPresets()
        {
            return new List<ParamDefine>
            {
                CreateTagTestPreset("untagged_null", "无标签 null / alpha", null),
                CreateTagTestPreset("untagged_empty", "空标签 empty / beta", string.Empty),
                CreateTagTestPreset("untagged_spaces", "空白标签 spaces / gamma", "   "),
                CreateTagTestPreset("untagged_tab", "空白标签 tab / alpha", "\t")
            };
        }

        /// <summary> 两个业务标签及重复标签, 用于对照完整容纳和溢出的边界. </summary>
        public static List<ParamDefine> GetSmallTagTestPresets()
        {
            return new List<ParamDefine>
            {
                CreateTagTestPreset("small_untagged", "无标签 alpha", null),
                CreateTagTestPreset("small_short_alpha", "短标签 alpha", "短"),
                CreateTagTestPreset("small_short_beta", "短标签 beta", "短"),
                CreateTagTestPreset("small_long_alpha", "较长标签 alpha", "名称稍长的标签")
            };
        }

        private static ParamDefine CreateTagTestPreset(string value, string name, string tag)
        {
            var parameter = QuoteTagTestValue(value);

            return new ParamDefine(parameter, name, "仅打印测试值. 可搜索 alpha/beta/gamma, 并与标签过滤组合.", tag);
        }

        private static string QuoteTagTestValue(string value)
        {
            var escaped = value.Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t");

            return $"\"{escaped}\"";
        }
#endif
    }
}
