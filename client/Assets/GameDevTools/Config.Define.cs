using UnityEngine;

namespace GameConsole
{
    /// <summary> 配置默认值, 范围和工具运行时限制的统一定义. </summary>
    public static partial class Config
    {
        /// <summary> 开发工具版本, 仅在代码中维护, 不写入配置文件. </summary>
        public const string version = "v1.0.0";

        /// <summary> 浮标日志缓存和显示上限. </summary>
        public const int MAX_FLOATING_LOG_COUNT = 10;

        /// <summary> 浮标日志显示下限. </summary>
        public const int MIN_FLOATING_LOG_COUNT = 1;

        /// <summary> 浮标日志默认条数. </summary>
        public const int DEFAULT_FLOATING_LOG_COUNT = 5;

        /// <summary> Cheat 最新日志显示上限. </summary>
        public const int MAX_CHEAT_LOG_COUNT = 3;

        /// <summary> Cheat 最新日志显示下限. </summary>
        public const int MIN_CHEAT_LOG_COUNT = 0;

        /// <summary> Cheat 最新日志默认条数. </summary>
        public const int DEFAULT_CHEAT_LOG_COUNT = 1;

        /// <summary> 默认开关键. </summary>
        public const KeyCode DEFAULT_SHORTCUT_KEY = KeyCode.BackQuote;

        /// <summary> 默认主页签. </summary>
        public const string defaultMainTagger = "console";

        /// <summary> 命令历史, 查询缓存和补全容量下限. </summary>
        public const int MIN_COMMAND_CAPACITY = 1;

        /// <summary> 命令历史, 查询缓存和补全容量上限. </summary>
        public const int MAX_COMMAND_CAPACITY = 64;

        /// <summary> 命令输入历史默认容量. </summary>
        public const int DEFAULT_INPUT_HISTORY_CAPACITY = 16;

        /// <summary> 命令查询缓存默认容量. </summary>
        public const int DEFAULT_COMMAND_QUERY_CAPACITY = 16;

        /// <summary> 命令补全默认数量. </summary>
        public const int DEFAULT_ALTERNATIVE_COMMAND_COUNT = 16;

        /// <summary> Cheat 搜索历史容量下限. </summary>
        public const int MIN_CHEAT_SEARCH_HISTORY_CAPACITY = 4;

        /// <summary> Cheat 搜索历史容量上限. </summary>
        public const int MAX_CHEAT_SEARCH_HISTORY_CAPACITY = 64;

        /// <summary> Cheat 搜索历史默认容量. </summary>
        public const int DEFAULT_CHEAT_SEARCH_HISTORY_CAPACITY = 16;

        /// <summary> 透明度下限. </summary>
        public const float MIN_OPACITY = 0f;

        /// <summary> 透明度上限. </summary>
        public const float MAX_OPACITY = 1f;

        /// <summary> 主界面可设置的透明度下限. </summary>
        public const float MIN_PANEL_OPACITY = .2f;

        /// <summary> 主界面默认透明度. </summary>
        public const float DEFAULT_PANEL_OPACITY = 1f;

        /// <summary> 普通界面文字可见度下限. </summary>
        public const float MIN_TEXT_OPACITY = .6f;

        /// <summary> 浮窗日志背景默认透明度. </summary>
        public const float DEFAULT_FLOATING_BACKGROUND_OPACITY = .4f;

        /// <summary> 浮窗日志文字默认透明度. </summary>
        public const float DEFAULT_FLOATING_TEXT_OPACITY = 1f;

        /// <summary> 百分比控件下限. </summary>
        public const int MIN_PERCENTAGE = 0;

        /// <summary> 百分比控件上限. </summary>
        public const int MAX_PERCENTAGE = 100;

        /// <summary> 预设和宏网格按钮尺寸等级下限. </summary>
        public const int MIN_PRESET_BUTTON_SIZE = 0;

        /// <summary> 预设和宏网格按钮尺寸等级上限. </summary>
        public const int MAX_PRESET_BUTTON_SIZE = 100;

        /// <summary> 预设按钮默认尺寸等级. </summary>
        public const int DEFAULT_PRESET_BUTTON_SIZE = 50;

        /// <summary> 宏按钮默认尺寸等级. </summary>
        public const int DEFAULT_MACRO_BUTTON_SIZE = 40;

        /// <summary> 宏编辑器空正文显示行数. </summary>
        public const int DEFAULT_MACRO_BODY_LINES = 1;

        /// <summary> 自定义宏正文行数上限, 包含空行和注释. </summary>
        public const int MAX_MACRO_BODY_LINES = 128;

        /// <summary> 宏循环次数下限. </summary>
        public const int MIN_MACRO_REPEAT_COUNT = 1;

        /// <summary> 宏循环次数上限. </summary>
        public const int MAX_MACRO_REPEAT_COUNT = 128;

        /// <summary> 控制台初始逻辑日志范围. </summary>
        public const int CONSOLE_RECENT_LOG_COUNT = 100;

        /// <summary> 控制台每帧准备行数上限. </summary>
        public const int CONSOLE_ROWS_PER_FRAME = 20;

        /// <summary> 控制台每帧准备时间预算, 单条排版不被中断. </summary>
        public const double CONSOLE_PREPARATION_SECONDS = .016;

        /// <summary> Cheat 每帧绑定命令数. </summary>
        public const int CHEAT_COMMANDS_PER_FRAME = 5;

        /// <summary> Cheat 每帧创建参数字段数. </summary>
        public const int CHEAT_PARAMETERS_PER_FRAME = 2;

        /// <summary> 预设和宏网格每帧绑定或加载图标数. </summary>
        public const int PRESET_ITEMS_PER_FRAME = 8;

        /// <summary> 选项每帧显示设置项数. </summary>
        public const int OPTIONS_ROWS_PER_FRAME = 3;

        /// <summary> 设置选择器可见项目数. </summary>
        public const int SELECTOR_VISIBLE_ITEMS = 5;

        /// <summary> 搜索历史下拉可见项目数. </summary>
        public const int SEARCH_HISTORY_VISIBLE_ITEMS = 10;

        /// <summary> 连续设置修改的合并保存延迟, 秒. </summary>
        public const float CONFIG_SAVE_DELAY = .2f;

        /// <summary> 保存失败后的重试间隔, 秒. </summary>
        public const float CONFIG_SAVE_RETRY_DELAY = 1f;
    }
}
