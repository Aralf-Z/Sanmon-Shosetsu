using System.Collections.Generic;
using GameConsole.Extension;

namespace GameConsole.Implementor
{
    public static partial class DefaultParamCollector
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public static List<ParamDefine> GetAchievementTagTestPresets()
        {
            var presets = new List<ParamDefine>();
            var keywords = new[] { "alpha", "beta", "gamma" };

            for (var tagIndex = 1; tagIndex <= 512; tagIndex++)
            {
                var tag = $"成就/分类/{tagIndex:000}";

                for (var sample = 0; sample < 6; sample++)
                {
                    var keyword = keywords[sample % keywords.Length];
                    var value = $"achievement_{tagIndex:000}_{sample}_{keyword}";

                    presets.Add(CreateTagTestPreset(value, $"成就 {tagIndex:000} / 预设 {sample + 1} / {keyword}", tag));
                }
            }

            presets.Add(CreateTagTestPreset("achievement_last_page_marker", "成就 / 最后一页标记", "～成就最后一个标签/LastPageMarker"));
            return presets;
        }

        public static List<ParamDefine> GetManyTagTestPresets()
        {
            var presets = GetTagTestPresets();
            var keywords = new[] { "alpha", "beta", "gamma" };

            for (var tagIndex = 1; tagIndex <= 256; tagIndex++)
            {
                for (var sample = 0; sample < 6; sample++)
                {
                    var keyword = keywords[sample % keywords.Length];
                    var tag = $"压力标签/{tagIndex:000}";
                    var value = $"stress_{tagIndex:000}_{sample}_{keyword}";

                    presets.Add(CreateTagTestPreset(value, $"压力预设 {tagIndex:000} / {sample} / {keyword}", tag));
                }
            }

            presets.Add(CreateTagTestPreset("last_page_marker", "最后一页 / marker", "～最后一个标签/LastPageMarker"));
            return presets;
        }

        public static List<ParamDefine> GetEmptyTagTestPresets() => new();

        public static List<ParamDefine> GetSingleTagTestPresets()
        {
            var presets = GetUntaggedTestPresets();

            for (var index = 0; index < 24; index++)
            {
                presets.Add(CreateTagTestPreset($"single_{index:00}", $"单一标签 / {index:00}", "唯一标签"));
            }

            return presets;
        }

        public static List<ParamDefine> GetDuplicateTagTestPresets()
        {
            var presets = new List<ParamDefine>();

            for (var index = 0; index < 96; index++)
            {
                presets.Add(CreateTagTestPreset($"duplicate_{index:00}", $"重复标签数据 / {index:00}", "重复标签"));
            }

            return presets;
        }

        public static List<ParamDefine> GetLongTagTestPresets()
        {
            var presets = new List<ParamDefine>();

            for (var index = 1; index <= 40; index++)
            {
                var tag = $"长名称/{index:00}/检查弹窗自动换行和分页/LongTagWithoutTruncationABCDEFGHIJKLMNOPQRSTUVWXYZ/{index:00}/完整尾部";

                presets.Add(CreateTagTestPreset($"long_{index:00}_alpha", $"超长预设名称 / {index:00} / alpha / 名称展示可以省略但详情应保留完整文本", tag));
            }

            return presets;
        }

        public static List<ParamDefine> GetExactTagTestPresets()
        {
            var presets = new List<ParamDefine>();
            var tags = new[] { "A", "a", "A,B", "A;B", "A|B", "A/B", "A B", " A", "A ", " A ", "A\tB", "A\nB", "全部", "更多", "<b>A</b>" };

            for (var index = 0; index < tags.Length; index++)
            {
                presets.Add(CreateTagTestPreset($"exact_{index:00}", $"完整匹配 / {index:00}", tags[index]));
            }

            return presets;
        }

        public static List<ParamDefine> GetPagingTagTestPresets()
        {
            var presets = new List<ParamDefine>();

            for (var index = 1; index <= 65; index++)
            {
                var tag = $"分页边界/{index:000}";

                presets.Add(CreateTagTestPreset($"page_{index:000}_alpha", $"边界 / {index:000} / alpha", tag));
                presets.Add(CreateTagTestPreset($"page_{index:000}_beta", $"边界 / {index:000} / beta", tag));
            }

            presets.Add(CreateTagTestPreset("paging_last_marker", "最后一页 / marker", "～最后一个标签/LastPageMarker"));
            return presets;
        }

        /// <summary> 代码文本作为字符串参数传入, 测试入口只打印, 不执行脚本. </summary>
        public static List<ParamDefine> GetCodeTagTestPresets()
        {
            return new List<ParamDefine>
            {
                CreateTagTestPreset("print(\"alpha beta\")", "Lua / 双引号", "代码/Lua"),
                CreateTagTestPreset("local value = 1\nprint(value)", "Lua / 多行", "代码/Lua"),
                CreateTagTestPreset("var message = \"alpha beta\";", "C# / 字符串", "代码/C#"),
                CreateTagTestPreset("{\"tag\":\"alpha\",\"path\":\"C:\\\\Temp\\\\test\"}", "JSON / 引号与反斜杠", "代码/JSON"),
                CreateTagTestPreset("C:\\Temp\\tag_test\\alpha.txt", "路径 / 反斜杠", "代码/路径"),
                CreateTagTestPreset("if value then\n\tprint('beta')\nend", "Lua / 缩进与单引号", "代码/Lua"),
                CreateTagTestPreset("alpha\r\nbeta\tgamma", "文本 / CRLF 与制表符", "代码/文本"),
                CreateTagTestPreset("", "文本 / 空字符串", "代码/文本"),
                CreateTagTestPreset("中文 日本語 한국어 alpha", "文本 / 多语言", "代码/文本")
            };
        }

        public static List<ParamDefine> GetTypedTagTestPresets()
        {
            return new List<ParamDefine>
            {
                new("0 0 false \"\"", "零值与空字符串", "int / float / bool / string", "类型/零值"),
                new("1 1.5 true \"alpha beta\"", "正数与空格", "int / float / bool / string", "类型/正数"),
                new("-1 -2.5 false \"negative\"", "负数", "int / float / bool / string", "类型/负数"),
                new("2147483647 3.25 true \"upper bound\"", "整数上界", "int / float / bool / string", "类型/边界"),
                new("7 0.125 true \"中文 日本語 한국어\"", "小数与多语言", "int / float / bool / string", "类型/文本"),
                new("2 4.5 false \"escaped \\\"quote\\\"\"", "嵌套引号", "int / float / bool / string", "类型/文本"),
                new("3 8 true \"line1\\nline2\"", "转义换行", "int / float / bool / string", "类型/文本"),
                new("4 2.25 false \"C:\\\\Temp\\\\tag_test\"", "路径", "int / float / bool / string", "类型/文本")
            };
        }

        public static List<ParamDefine> GetOptionalTagTestPresets()
        {
            return new List<ParamDefine>
            {
                new("", "全部使用默认值", "省略两个可选参数", "默认参数"),
                new("\"alpha\"", "只覆盖第一个参数", "第二个参数使用默认值", "默认参数"),
                new("\"beta gamma\" 0", "覆盖所有参数", "包含空格的字符串参数", "覆盖参数")
            };
        }

        public static List<ParamDefine> GetUnicodeTagTestPresets()
        {
            var presets = new List<ParamDefine>();
            var tags = new[] { "中文标签", "日本語タグ", "한국어 태그", "English tag", "中日韓 Mixed Tag" };

            for (var index = 0; index < tags.Length; index++)
            {
                presets.Add(CreateTagTestPreset($"多语言 {index} / alpha beta", tags[index], tags[index]));
            }

            return presets;
        }
#endif
    }
}
