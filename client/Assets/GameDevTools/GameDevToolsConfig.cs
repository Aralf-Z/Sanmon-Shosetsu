using System;
using System.Collections.Generic;
using UnityEngine;
using GameConsole.Extension;

namespace GameConsole
{
    /// <summary> 开发工具的持久化数据, 由构造函数提供默认值. </summary>
    [Serializable]
    public sealed class GameDevToolsConfig
    {
        public KeyCode keyCode;

        public ShortcutModifier keyModifier;

        public bool minimizeOnShortcut;

        /// <summary> 开发工具整体不透明度, 不包含浮窗日志. </summary>
        public float panelOpacity;

        public float wheelScrollSensitivity;

        public string uiStyle;

        public string uiFont;

        public string uiLanguage;

        public int inputHistoryCapacity;

        public int commandQueryCacheCapacity;

        public int alternativeCommandCount;

        public bool consoleShowLogIcons;

        public bool consoleShowTimestamps;

        /// <summary> 记录失败命令. </summary>
        public bool recordFailedCommand;

        /// <summary> 输出命令异常堆栈. </summary>
        public bool outputVmExceptionStack;

        public bool consoleCollapseLogs;

        public int cheatLogLineCount;

        public int cheatSearchHistoryCapacity;

        public List<string> cheatSearchHistory;

        public bool floatingShowLatestLogs;

        public bool floatingShowRealtimeFps;

        public bool floatingShowAverageFps;

        public bool floatingShowLowFps;

        public bool floatingShowFrameTimeGraph;

        public float floatingMonitorBackgroundOpacity;

        public int floatingLogLineCount;

        public float floatingLogBackgroundOpacity;

        public float floatingLogTextOpacity;

        public List<string> cheatPinnedCommands;

        public List<string> cheatPinnedMacros;

        public string macroWaitForColor;

        public string macroRepeatColor;

        public string mainTagger;

        public GameDevToolsConfig()
        {
            keyCode = Config.DEFAULT_SHORTCUT_KEY;
            keyModifier = ShortcutModifier.None;
            minimizeOnShortcut = false;
            panelOpacity = Config.DEFAULT_PANEL_OPACITY;
            wheelScrollSensitivity = Config.DEFAULT_WHEEL_SCROLL_SENSITIVITY;
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
            floatingShowRealtimeFps = true;
            floatingShowAverageFps = true;
            floatingShowLowFps = true;
            floatingShowFrameTimeGraph = false;
            floatingMonitorBackgroundOpacity = Config.DEFAULT_FLOATING_MONITOR_BACKGROUND_OPACITY;
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
