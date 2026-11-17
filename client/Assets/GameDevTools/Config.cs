using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using GameConsole.Extension;

namespace GameConsole
{
    /// <summary> 开发工具唯一的配置读取和持久化入口. </summary>
    public static partial class Config
    {
        public static event Action e_onChanged;

        public static string FilePath => Path.Combine(Application.persistentDataPath, "development_config.json");

        public static string MacroWaitForColor => Data.macroWaitForColor;

        public static string MacroRepeatColor => Data.macroRepeatColor;

        public static IReadOnlyList<string> GetPins(bool macros) => (macros
            ? Data.cheatPinnedMacros
            : Data.cheatPinnedCommands).AsReadOnly();

        public static void SetPinned(string id, bool macros, bool pinned)
        {
            if (string.IsNullOrEmpty(id)) return;

            var pins = macros
                ? Data.cheatPinnedMacros
                : Data.cheatPinnedCommands;

            if (pinned == pins.Contains(id)) return;

            if (pinned)
            {
                pins.Add(id);
            }
            else
            {
                pins.Remove(id);
            }

            Save();
        }

        /// <summary> 相对于目标项移动, 不改变其他置顶项的相对顺序. </summary>
        public static void MovePinned(string id, string target, bool macros, bool after)
        {
            var pins = macros
                ? Data.cheatPinnedMacros
                : Data.cheatPinnedCommands;

            if (id == target || !pins.Contains(id) || !pins.Contains(target)) return;

            pins.Remove(id);
            pins.Insert(pins.IndexOf(target) + (after ? 1 : 0), id);
            Save();
        }

        public static string MainTagger
        {
            get => Data.mainTagger;
            set
            {
                value = NormalizeMainTagger(value);
                if (Data.mainTagger == value) return;

                Data.mainTagger = value;
                Save();
            }
        }

        /// <summary> 工具开关键的基础键, 仅允许功能键, 字母, 数字和标点. </summary>
        public static KeyCode KeyCode
        {
            get => Data.keyCode; set => Data.keyCode = ToolShortcut.NormalizeKey(value);
        }

        public static ShortcutModifier KeyModifier
        {
            get => Data.keyModifier; set => Data.keyModifier = ToolShortcut.NormalizeModifier(value);
        }

        /// <summary> 主窗口打开时按开关键最小化; 为 false 时关闭工具. </summary>
        public static bool MinimizeOnShortcut
        {
            get => Data.minimizeOnShortcut; set => Data.minimizeOnShortcut = value;
        }

        /// <summary> 整体界面不透明度, 非法浮点值回退为全显示. </summary>
        public static float PanelOpacity
        {
            get => Data.panelOpacity; set => Data.panelOpacity = NormalizePanelOpacity(value);
        }

        public static string UIStyle
        {
            get => Data.uiStyle; set => Data.uiStyle = NormalizeResourcePath(value);
        }

        public static string UIFont
        {
            get => Data.uiFont; set => Data.uiFont = NormalizeResourcePath(value);
        }

        /// <summary> 仅保存完成提交的语言代码. </summary>
        public static string UILanguage
        {
            get => Data.uiLanguage; set => Data.uiLanguage = string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim();
        }

        public static int InputHistoryCapacity
        {
            get => Data.inputHistoryCapacity; set => Data.inputHistoryCapacity = Mathf.Clamp(value, MIN_COMMAND_CAPACITY, MAX_COMMAND_CAPACITY);
        }

        public static int CommandQueryCacheCapacity
        {
            get => Data.commandQueryCacheCapacity; set => Data.commandQueryCacheCapacity = Mathf.Clamp(value, MIN_COMMAND_CAPACITY, MAX_COMMAND_CAPACITY);
        }

        public static int AlternativeCommandCount
        {
            get => Data.alternativeCommandCount; set => Data.alternativeCommandCount = Mathf.Clamp(value, MIN_COMMAND_CAPACITY, MAX_COMMAND_CAPACITY);
        }

        public static bool ConsoleShowLogIcons
        {
            get => Data.consoleShowLogIcons; set => Data.consoleShowLogIcons = value;
        }

        public static bool ConsoleShowTimestamps
        {
            get => Data.consoleShowTimestamps; set => Data.consoleShowTimestamps = value;
        }

        /// <summary> 记录失败命令. </summary>
        public static bool RecordFailedCommand
        {
            get => Data.recordFailedCommand; set => Data.recordFailedCommand = value;
        }

        /// <summary> 输出异常堆栈. </summary>
        public static bool OutputVmExceptionStack
        {
            get => Data.outputVmExceptionStack; set => Data.outputVmExceptionStack = value;
        }

        public static bool ConsoleCollapseLogs
        {
            get => Data.consoleCollapseLogs; set => Data.consoleCollapseLogs = value;
        }

        /// <summary> Cheat 页底部最新日志行数, 零隐藏预览. </summary>
        public static int CheatLogLineCount
        {
            get => Data.cheatLogLineCount; set => Data.cheatLogLineCount = Mathf.Clamp(value, MIN_CHEAT_LOG_COUNT, MAX_CHEAT_LOG_COUNT);
        }

        public static int CheatSearchHistoryCapacity
        {
            get => Data.cheatSearchHistoryCapacity;
            set
            {
                Data.cheatSearchHistoryCapacity = Mathf.Clamp(value, MIN_CHEAT_SEARCH_HISTORY_CAPACITY, MAX_CHEAT_SEARCH_HISTORY_CAPACITY);
                NormalizeSearchHistory(Data);
            }
        }

        public static IReadOnlyList<string> CheatSearchHistory => Data.cheatSearchHistory.AsReadOnly();

        public static bool FloatingShowLatestLogs
        {
            get => Data.floatingShowLatestLogs; set => Data.floatingShowLatestLogs = value;
        }

        public static int FloatingLogLineCount
        {
            get => Data.floatingLogLineCount; set => Data.floatingLogLineCount = Mathf.Clamp(value, MIN_FLOATING_LOG_COUNT, MAX_FLOATING_LOG_COUNT);
        }

        public static float FloatingLogBackgroundOpacity
        {
            get => Data.floatingLogBackgroundOpacity; set => Data.floatingLogBackgroundOpacity = NormalizeOpacity(value, DEFAULT_FLOATING_BACKGROUND_OPACITY);
        }

        public static float FloatingLogTextOpacity
        {
            get => Data.floatingLogTextOpacity; set => Data.floatingLogTextOpacity = NormalizeOpacity(value, DEFAULT_FLOATING_TEXT_OPACITY);
        }

        private static GameDevToolsConfig _data;

        private static GameDevToolsConfig Data
        {
            get
            {
                if (_data == null)
                {
                    Reload();
                }

                return _data;
            }
        }

        public static void RecordCheatSearch(string query)
        {
            if (RecordSearchHistory(Data, query))
            {
                Save();
            }
        }

        public static void ClearCheatSearchHistory()
        {
            if (Data.cheatSearchHistory.Count == 0) return;

            Data.cheatSearchHistory.Clear();
            Save();
        }

        public static void RemoveCheatSearch(string query)
        {
            query = (query ?? string.Empty).Trim();
            if (query.Length == 0) return;

            if (Data.cheatSearchHistory.RemoveAll(value => string.Equals(value, query, StringComparison.OrdinalIgnoreCase)) > 0)
            {
                Save();
            }
        }

        /// <summary> 每次启动显式重读, 兼容关闭 Domain Reload 的 Play 模式. </summary>
        public static void Reload()
        {
            var rewrite = !File.Exists(FilePath);

            _data = new GameDevToolsConfig();
            try
            {
                if (!rewrite)
                {
                    _data = JsonConvert.DeserializeObject<GameDevToolsConfig>(File.ReadAllText(FilePath)) ?? throw new JsonSerializationException("配置根节点不能为空.");
                }

                Normalize(_data);
            }
            catch (Exception exception)
            {
                _data = new GameDevToolsConfig();
                rewrite = true;
                Debug.LogWarning($"[GameDevTools] 读取配置失败, 重建默认配置. {exception.Message}");
            }

            if (rewrite)
            {
                Write();
            }

            e_onChanged?.Invoke();
        }

        /// <summary> 返回独立副本, 防止 Inspector 保留过期的可变引用. </summary>
        public static GameDevToolsConfig Snapshot() => JsonConvert.DeserializeObject<GameDevToolsConfig>(JsonConvert.SerializeObject(Data));

        public static void Edit(Action<GameDevToolsConfig> edit)
        {
            edit(Data);
            Save();
        }

        public static void Reset()
        {
            _data = new GameDevToolsConfig();
            Save();
        }

        /// <summary> 保存设置, 失败时返回 false 并保留内存设置. </summary>
        public static bool Save()
        {
            Normalize(Data);
            if (!Write()) return false;

            e_onChanged?.Invoke();
            return true;
        }

        // 仅写入当前数据, 由调用者统一发出变更通知.
        private static bool Write()
        {
            try
            {
                Directory.CreateDirectory(Application.persistentDataPath);
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(_data, Formatting.Indented));
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[GameDevTools] 保存配置失败. {exception.Message}");
                return false;
            }
        }

        // 统一磁盘数据, Inspector 和运行时的边界.
        private static void Normalize(GameDevToolsConfig data)
        {
            data.mainTagger = NormalizeMainTagger(data.mainTagger);
            data.keyCode = ToolShortcut.NormalizeKey(data.keyCode);
            data.keyModifier = ToolShortcut.NormalizeModifier(data.keyModifier);
            data.panelOpacity = NormalizePanelOpacity(data.panelOpacity);
            data.uiStyle = NormalizeResourcePath(data.uiStyle);
            data.uiFont = NormalizeResourcePath(data.uiFont);
            data.uiLanguage = string.IsNullOrWhiteSpace(data.uiLanguage)
                ? string.Empty
                : data.uiLanguage.Trim();
            data.macroWaitForColor = MacroSyntaxColors.Normalize(data.macroWaitForColor, MacroSyntaxColors.defaultWait);
            data.macroRepeatColor = MacroSyntaxColors.Normalize(data.macroRepeatColor, MacroSyntaxColors.defaultRepeat);
            data.cheatPinnedCommands = (data.cheatPinnedCommands ?? new List<string>()).Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();
            data.cheatPinnedMacros = (data.cheatPinnedMacros ?? new List<string>()).Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();
            data.inputHistoryCapacity = Mathf.Clamp(data.inputHistoryCapacity, MIN_COMMAND_CAPACITY, MAX_COMMAND_CAPACITY);
            data.commandQueryCacheCapacity = Mathf.Clamp(data.commandQueryCacheCapacity, MIN_COMMAND_CAPACITY, MAX_COMMAND_CAPACITY);
            data.alternativeCommandCount = Mathf.Clamp(data.alternativeCommandCount, MIN_COMMAND_CAPACITY, MAX_COMMAND_CAPACITY);
            data.cheatLogLineCount = Mathf.Clamp(data.cheatLogLineCount, MIN_CHEAT_LOG_COUNT, MAX_CHEAT_LOG_COUNT);
            data.cheatSearchHistoryCapacity = Mathf.Clamp(data.cheatSearchHistoryCapacity, MIN_CHEAT_SEARCH_HISTORY_CAPACITY, MAX_CHEAT_SEARCH_HISTORY_CAPACITY);
            NormalizeSearchHistory(data);
            data.floatingLogLineCount = Mathf.Clamp(data.floatingLogLineCount, MIN_FLOATING_LOG_COUNT, MAX_FLOATING_LOG_COUNT);
            data.floatingLogBackgroundOpacity = NormalizeOpacity(data.floatingLogBackgroundOpacity, DEFAULT_FLOATING_BACKGROUND_OPACITY);
            data.floatingLogTextOpacity = NormalizeOpacity(data.floatingLogTextOpacity, DEFAULT_FLOATING_TEXT_OPACITY);
        }

        // 兼容缺失或非法历史字段, 保留最近项和词内原始文本.
        private static void NormalizeSearchHistory(GameDevToolsConfig data)
        {
            data.cheatSearchHistory = (data.cheatSearchHistory ?? new List<string>()).Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(data.cheatSearchHistoryCapacity)
                .ToList();
        }

        // 更新最近使用顺序并返回是否变化, 相同首项不重复写入配置.
        private static bool RecordSearchHistory(GameDevToolsConfig data, string query)
        {
            query = (query ?? string.Empty).Trim();
            if (query.Length == 0) return false;

            var history = data.cheatSearchHistory;

            if (history.Count > 0 && history[0] == query) return false;

            history.RemoveAll(value => string.Equals(value, query, StringComparison.OrdinalIgnoreCase));
            history.Insert(0, query);
            NormalizeSearchHistory(data);
            return true;
        }

        // 防止非法浮点值传入颜色和布局.
        private static float NormalizeOpacity(float value, float fallback)
        {
            return float.IsNaN(value) || float.IsInfinity(value)
                ? fallback
                : Mathf.Clamp(value, MIN_OPACITY, MAX_OPACITY);
        }

        // 整体界面保留最低可见度, 确保设置仍能操作.
        private static float NormalizePanelOpacity(float value) => float.IsNaN(value) || float.IsInfinity(value)
            ? DEFAULT_PANEL_OPACITY
            : Mathf.Clamp(value, MIN_PANEL_OPACITY, MAX_OPACITY);

        // 未知主页签回退到默认页, 不读取旧的独立存档.
        private static string NormalizeMainTagger(string value) => value == "console" || value == "cheat" || value == "macro" || value == "option"
            ? value
            : defaultMainTagger;

        // 资源选择和失效回退由 Schema 处理, 此处仅规范路径格式.
        private static string NormalizeResourcePath(string value) => string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().Replace('\\', '/');
    }
}
