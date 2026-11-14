using System;
using System.Collections.Generic;

namespace GameConsole
{
    internal static partial class ToolkitLocalization
    {
        // JSON 使用 {Config_常量名称} 引用限制值, 新增引用时在此登记, 不读取运行时配置或反射字段.
        private static readonly IReadOnlyDictionary<string, object> _configArguments = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["Config_" + nameof(Config.MIN_CHEAT_SEARCH_HISTORY_CAPACITY)] = Config.MIN_CHEAT_SEARCH_HISTORY_CAPACITY,
            ["Config_" + nameof(Config.MAX_CHEAT_SEARCH_HISTORY_CAPACITY)] = Config.MAX_CHEAT_SEARCH_HISTORY_CAPACITY,
            ["Config_" + nameof(Config.DEFAULT_CHEAT_SEARCH_HISTORY_CAPACITY)] = Config.DEFAULT_CHEAT_SEARCH_HISTORY_CAPACITY,
            ["Config_" + nameof(Config.SEARCH_HISTORY_VISIBLE_ITEMS)] = Config.SEARCH_HISTORY_VISIBLE_ITEMS,
            ["Config_" + nameof(Config.MIN_FLOATING_LOG_COUNT)] = Config.MIN_FLOATING_LOG_COUNT,
            ["Config_" + nameof(Config.MAX_FLOATING_LOG_COUNT)] = Config.MAX_FLOATING_LOG_COUNT,
            ["Config_" + nameof(Config.MIN_CHEAT_LOG_COUNT)] = Config.MIN_CHEAT_LOG_COUNT,
            ["Config_" + nameof(Config.MAX_CHEAT_LOG_COUNT)] = Config.MAX_CHEAT_LOG_COUNT,
            ["Config_" + nameof(Config.MAX_MACRO_BODY_LINES)] = Config.MAX_MACRO_BODY_LINES,
            ["Config_" + nameof(Config.MIN_MACRO_REPEAT_COUNT)] = Config.MIN_MACRO_REPEAT_COUNT,
            ["Config_" + nameof(Config.MAX_MACRO_REPEAT_COUNT)] = Config.MAX_MACRO_REPEAT_COUNT,
            ["Config_" + nameof(Config.CONSOLE_RECENT_LOG_COUNT)] = Config.CONSOLE_RECENT_LOG_COUNT,
            ["Config_" + nameof(Config.MIN_OPACITY)] = Config.MIN_OPACITY,
            ["Config_" + nameof(Config.MAX_OPACITY)] = Config.MAX_OPACITY,
            ["Config_" + nameof(Config.MIN_PANEL_OPACITY)] = Config.MIN_PANEL_OPACITY,
            ["Config_" + nameof(Config.DEFAULT_PANEL_OPACITY)] = Config.DEFAULT_PANEL_OPACITY,
            ["Config_" + nameof(Config.MIN_TEXT_OPACITY)] = Config.MIN_TEXT_OPACITY,
            ["Config_" + nameof(Config.CONFIG_SAVE_DELAY)] = Config.CONFIG_SAVE_DELAY,
            ["Config_" + nameof(Config.MIN_PERCENTAGE)] = Config.MIN_PERCENTAGE,
            ["Config_" + nameof(Config.MAX_PERCENTAGE)] = Config.MAX_PERCENTAGE,
        };

        // 只解析保留前缀的配置占位符, 普通文案和界面图标直接返回, 业务参数不参与再次解析.
        private static string FormatConfig(string source) => source.IndexOf("{Config_", StringComparison.Ordinal) < 0 ? source : ToolkitLocalizationData.Format(source, _configArguments);
    }
}
