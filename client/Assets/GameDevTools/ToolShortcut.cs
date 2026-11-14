using System.Collections.Generic;
using UnityEngine;

namespace GameConsole
{
    /// <summary> 工具开关键可选的单个前置修饰键, 不区分左右侧. </summary>
    public enum ShortcutModifier
    {
        /// <summary> 不使用修饰键. </summary>
        None = 0,
        /// <summary> Control. </summary>
        Ctrl = 1,
        /// <summary> Alt. </summary>
        Alt = 2,
        /// <summary> Shift. </summary>
        Shift = 3
    }

    /// <summary> 统一约束和匹配工具快捷键, 不依赖项目业务或 UI 控件. </summary>
    public static class ToolShortcut
    {
        private static readonly IReadOnlyList<KeyCode> _keys = CreateKeys(); // 设置与 Inspector 共用的物理基础键列表.

        /// <summary> F1 到 F15, 字母, 顶部数字及标点键, 不包含小键盘和导航键. </summary>
        public static IReadOnlyList<KeyCode> Keys => _keys;
        /// <summary> 当前组合键的可读名称, 用于窗口与帮助提示. </summary>
        public static string DisplayName => Config.KeyModifier == ShortcutModifier.None ? GetKeyName(Config.KeyCode) : $"{Config.KeyModifier} + {GetKeyName(Config.KeyCode)}";

        /// <summary> 主线程逐帧检测, 修饰键必须与所选单个前置键一致. </summary>
        public static bool WasPressed()
        {
            var key = Config.KeyCode;
            return Input.GetKeyDown(key) && Matches(key, Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl), Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt), Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift), Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand));
        }

        /// <summary> UI 键盘事件与全局按键使用相同规则, 防止 Ctrl 组合吞掉普通字母输入. </summary>
        public static bool Matches(KeyCode key, bool ctrl, bool alt, bool shift, bool command = false)
        {
            key = PhysicalKey(key);
            var modifier = Config.KeyModifier;
            return !command && IsSupportedKey(key) && key == Config.KeyCode && ctrl == (modifier == ShortcutModifier.Ctrl) && alt == (modifier == ShortcutModifier.Alt) && shift == (modifier == ShortcutModifier.Shift);
        }

        /// <summary> 非法旧配置恢复反引号, 已保存的移位符号转换到对应物理键. </summary>
        public static KeyCode NormalizeKey(KeyCode key)
        {
            key = PhysicalKey(key);
            return IsSupportedKey(key) ? key : Config.DEFAULT_SHORTCUT_KEY;
        }

        /// <summary> 非法修饰键配置按无修饰键处理. </summary>
        public static ShortcutModifier NormalizeModifier(ShortcutModifier value) => value is ShortcutModifier.None or ShortcutModifier.Ctrl or ShortcutModifier.Alt or ShortcutModifier.Shift ? value : ShortcutModifier.None;

        /// <summary> 只接受所支持的基础物理键. </summary>
        public static bool IsSupportedKey(KeyCode key) => key >= KeyCode.F1 && key <= KeyCode.F15 || key >= KeyCode.A && key <= KeyCode.Z || key >= KeyCode.Alpha0 && key <= KeyCode.Alpha9 || key is KeyCode.BackQuote or KeyCode.Minus or KeyCode.Equals or KeyCode.LeftBracket or KeyCode.RightBracket or KeyCode.Backslash or KeyCode.Semicolon or KeyCode.Quote or KeyCode.Comma or KeyCode.Period or KeyCode.Slash;

        /// <summary> 组合键提示使用基础键名, 不把移位符号重复显示为额外按键. </summary>
        public static string GetKeyName(KeyCode key)
        {
            key = NormalizeKey(key);
            if (key >= KeyCode.Alpha0 && key <= KeyCode.Alpha9) return ((int)key - (int)KeyCode.Alpha0).ToString();
            return key switch
            {
                KeyCode.BackQuote => "`", KeyCode.Minus => "-", KeyCode.Equals => "=",
                KeyCode.LeftBracket => "[", KeyCode.RightBracket => "]", KeyCode.Backslash => "\\",
                KeyCode.Semicolon => ";", KeyCode.Quote => "'", KeyCode.Comma => ",", KeyCode.Period => ".", KeyCode.Slash => "/",
                _ => key.ToString()
            };
        }

        /// <summary> 选择器仅显示标准键帽下侧的基础字符, 字母和功能键保留键名. </summary>
        public static string GetKeyOptionName(KeyCode key) => GetKeyName(key);

        // 使用真实的物理基础键列表, 符号的 Shift 形式由修饰键选择表达.
        private static IReadOnlyList<KeyCode> CreateKeys()
        {
            var keys = new List<KeyCode>();
            for (var key = (int)KeyCode.F1; key <= (int)KeyCode.F15; key++) keys.Add((KeyCode)key);
            for (var key = (int)KeyCode.A; key <= (int)KeyCode.Z; key++) keys.Add((KeyCode)key);
            for (var key = (int)KeyCode.Alpha0; key <= (int)KeyCode.Alpha9; key++) keys.Add((KeyCode)key);
            keys.AddRange(new[] { KeyCode.BackQuote, KeyCode.Minus, KeyCode.Equals, KeyCode.LeftBracket, KeyCode.RightBracket, KeyCode.Backslash, KeyCode.Semicolon, KeyCode.Quote, KeyCode.Comma, KeyCode.Period, KeyCode.Slash });
            return keys.AsReadOnly();
        }

        // 旧配置或 UI 事件可能报告移位后的 ASCII 符号, 转回数字行或对应标点物理键.
        private static KeyCode PhysicalKey(KeyCode key) => (int)key switch
        {
            33 => KeyCode.Alpha1, 34 => KeyCode.Quote, 35 => KeyCode.Alpha3, 36 => KeyCode.Alpha4, 37 => KeyCode.Alpha5, 38 => KeyCode.Alpha7,
            40 => KeyCode.Alpha9, 41 => KeyCode.Alpha0, 42 => KeyCode.Alpha8, 43 => KeyCode.Equals, 58 => KeyCode.Semicolon, 60 => KeyCode.Comma,
            62 => KeyCode.Period, 63 => KeyCode.Slash, 64 => KeyCode.Alpha2, 94 => KeyCode.Alpha6, 95 => KeyCode.Minus,
            123 => KeyCode.LeftBracket, 124 => KeyCode.Backslash, 125 => KeyCode.RightBracket, 126 => KeyCode.BackQuote,
            _ => key
        };
    }
}
