using System;
using System.Collections.Generic;
using GameConsole.Extension;
using UnityEngine;

namespace GameConsole
{
    /// <summary> 开发工具的持久化数据, 由构造函数提供默认值. </summary>
    [Serializable]
    public sealed class GameDevToolsConfig
    {
        /// <summary> 工具打开, 还原或关闭的基础键, 可配置为打开时最小化. </summary>
        public KeyCode keyCode;
        /// <summary> 可选的单个前置修饰键. </summary>
        public ShortcutModifier keyModifier;
        /// <summary> 主窗口打开时再次按开关键改为最小化. </summary>
        public bool minimizeOnShortcut;
        /// <summary> 开发工具整体不透明度, 不包含浮窗日志. </summary>
        public float panelOpacity;
        /// <summary> 已选 UI 风格资源路径, 空值使用 Schema 风格列表首项. </summary>
        public string uiStyle;
        /// <summary> 已选字体的 Resources 路径, 空值使用 Schema 字体列表首项. </summary>
        public string uiFont;
        /// <summary> 工具语言代码, 空值使用 Schema 语言列表首项, 与游戏语言独立. </summary>
        public string uiLanguage;
        /// <summary> 输入历史容量. </summary>
        public int inputHistoryCapacity;
        /// <summary> 命令查询缓存容量. </summary>
        public int commandQueryCacheCapacity;
        /// <summary> 补全候选数量. </summary>
        public int alternativeCommandCount;
        /// <summary> Console 日志前显示类型图标, 即时生效. </summary>
        public bool consoleShowLogIcons;
        /// <summary> Console 日志显示接收时间, 即时作用于已缓存日志. </summary>
        public bool consoleShowTimestamps;
        /// <summary> 记录失败命令. </summary>
        public bool recordFailedCommand;
        /// <summary> 输出命令异常堆栈. </summary>
        public bool outputVmExceptionStack;
        /// <summary> 是否折叠连续相同日志. </summary>
        public bool consoleCollapseLogs;
        /// <summary> Cheat 页最新日志行数. </summary>
        public int cheatLogLineCount;
        /// <summary> Cheat 三类搜索共用的历史容量. </summary>
        public int cheatSearchHistoryCapacity;
        /// <summary> Cheat 搜索词历史, 最近使用的词在前. </summary>
        public List<string> cheatSearchHistory;
        /// <summary> 浮标是否显示最新日志. </summary>
        public bool floatingShowLatestLogs;
        /// <summary> 浮标最新日志条数. </summary>
        public int floatingLogLineCount;
        /// <summary> 浮标日志背景不透明度. </summary>
        public float floatingLogBackgroundOpacity;
        /// <summary> 浮标日志文字不透明度. </summary>
        public float floatingLogTextOpacity;
        /// <summary> 用户置顶命令, 列表顺序即显示位序. </summary>
        public List<string> cheatPinnedCommands;
        /// <summary> 用户置顶宏的持久化标识. </summary>
        public List<string> cheatPinnedMacros;
        /// <summary> WaitFor 语法行的预选颜色. </summary>
        public string macroWaitForColor;
        /// <summary> Repeat 与 EndRepeat 语法行的预选颜色. </summary>
        public string macroRepeatColor;
        /// <summary> 当前主页签, 与其他设置一同保存. </summary>
        public string mainTagger;

        /// <summary> 创建独立的默认配置和集合. </summary>
        public GameDevToolsConfig()
        {
            keyCode = Config.DEFAULT_SHORTCUT_KEY;
            keyModifier = ShortcutModifier.None;
            minimizeOnShortcut = false;
            panelOpacity = Config.DEFAULT_PANEL_OPACITY;
            uiStyle = string.Empty;
            uiFont = string.Empty;
            uiLanguage = string.Empty;
            inputHistoryCapacity = Config.DEFAULT_INPUT_HISTORY_CAPACITY;
            commandQueryCacheCapacity = Config.DEFAULT_COMMAND_QUERY_CAPACITY;
            alternativeCommandCount = Config.DEFAULT_ALTERNATIVE_COMMAND_COUNT;
            consoleShowLogIcons = true;
            consoleShowTimestamps = true;
            recordFailedCommand = true;
            outputVmExceptionStack = false;
            consoleCollapseLogs = false;
            cheatLogLineCount = Config.DEFAULT_CHEAT_LOG_COUNT;
            cheatSearchHistoryCapacity = Config.DEFAULT_CHEAT_SEARCH_HISTORY_CAPACITY;
            cheatSearchHistory = new List<string>();
            floatingShowLatestLogs = true;
            floatingLogLineCount = Config.DEFAULT_FLOATING_LOG_COUNT;
            floatingLogBackgroundOpacity = Config.DEFAULT_FLOATING_BACKGROUND_OPACITY;
            floatingLogTextOpacity = Config.DEFAULT_FLOATING_TEXT_OPACITY;
            cheatPinnedCommands = new List<string>();
            cheatPinnedMacros = new List<string>();
            macroWaitForColor = MacroSyntaxColors.defaultWait;
            macroRepeatColor = MacroSyntaxColors.defaultRepeat;
            mainTagger = Config.defaultMainTagger;
        }
    }
}
