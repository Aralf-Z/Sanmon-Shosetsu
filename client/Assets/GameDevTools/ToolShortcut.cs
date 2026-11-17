using System.Collections.Generic;
using UnityEngine;

namespace GameConsole
{
    /// <summary> 工具开关键可选的单个前置修饰键, 不区分左右侧. </summary>
    public enum ShortcutModifier
    {
        None = 0,
        Ctrl = 1,
        Alt = 2,
        Shift = 3
    }

    /// <summary> 统一约束和匹配工具快捷键, 不依赖项目业务或 UI 控件. </summary>
    public static class ToolShortcut
    {
        private static readonly IReadOnlyList<KeyCode> _keys = CreateKeys();

        /// <summary> F1 到 F15, 字母, 顶部数字及标点键, 不包含小键盘和导航键. </summary>
        public static IReadOnlyList<KeyCode> Keys => _keys;

        public static string DisplayName => Config.KeyModifier == ShortcutModifier.None
            ? GetKeyName(Config.KeyCode)
            : $"{Config.KeyModifier} + {GetKeyName(Config.KeyCode)}";

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

        public static KeyCode NormalizeKey(KeyCode key)
        {
            key = PhysicalKey(key);
            return IsSupportedKey(key)
                ? key
                : Config.DEFAULT_SHORTCUT_KEY;
        }

        public static ShortcutModifier NormalizeModifier(ShortcutModifier value) => value is ShortcutModifier.None or ShortcutModifier.Ctrl or ShortcutModifier.Alt or ShortcutModifier.Shift
            ? value
            : ShortcutModifier.None;

        public static bool IsSupportedKey(KeyCode key) => key >= KeyCode.F1 && key <= KeyCode.F15 || key >= KeyCode.A && key <= KeyCode.Z || key >= KeyCode.Alpha0 && key <= KeyCode.Alpha9 || key is KeyCode.BackQuote or KeyCode.Minus or KeyCode.Equals or KeyCode.LeftBracket or KeyCode.RightBracket or KeyCode.Backslash or KeyCode.Semicolon or KeyCode.Quote or KeyCode.Comma or KeyCode.Period or KeyCode.Slash;

        public static string GetKeyName(KeyCode key)
        {
            key = NormalizeKey(key);
            if (key >= KeyCode.Alpha0 && key <= KeyCode.Alpha9) return ((int)key - (int)KeyCode.Alpha0).ToString();

            return key switch
            {
                KeyCode.BackQuote => "`",
                KeyCode.Minus => "-",
                KeyCode.Equals => "=",
                KeyCode.LeftBracket => "[",
                KeyCode.RightBracket => "]",
                KeyCode.Backslash => "\\",
                KeyCode.Semicolon => ";",
                KeyCode.Quote => "'",
                KeyCode.Comma => ",",
                KeyCode.Period => ".",
                KeyCode.Slash => "/",
                _ => key.ToString()
            };
        }

        /// <summary> 选择器仅显示标准键帽下侧的基础字符, 字母和功能键保留键名. </summary>
        public static string GetKeyOptionName(KeyCode key) => GetKeyName(key);

        private static IReadOnlyList<KeyCode> CreateKeys()
        {
            var keys = new List<KeyCode>();

            for (var key = (int)KeyCode.F1; key <= (int)KeyCode.F15; key++)
            {
                keys.Add((KeyCode)key);
            }

            for (var key = (int)KeyCode.A; key <= (int)KeyCode.Z; key++)
            {
                keys.Add((KeyCode)key);
            }

            for (var key = (int)KeyCode.Alpha0; key <= (int)KeyCode.Alpha9; key++)
            {
                keys.Add((KeyCode)key);
            }

            keys.AddRange(new[] { KeyCode.BackQuote, KeyCode.Minus, KeyCode.Equals, KeyCode.LeftBracket, KeyCode.RightBracket, KeyCode.Backslash, KeyCode.Semicolon, KeyCode.Quote, KeyCode.Comma, KeyCode.Period, KeyCode.Slash });
            return keys.AsReadOnly();
        }

        private static KeyCode PhysicalKey(KeyCode key) => (int)key switch
        {
            33 => KeyCode.Alpha1,
            34 => KeyCode.Quote,
            35 => KeyCode.Alpha3,
            36 => KeyCode.Alpha4,
            37 => KeyCode.Alpha5,
            38 => KeyCode.Alpha7,
            40 => KeyCode.Alpha9,
            41 => KeyCode.Alpha0,
            42 => KeyCode.Alpha8,
            43 => KeyCode.Equals,
            58 => KeyCode.Semicolon,
            60 => KeyCode.Comma,
            62 => KeyCode.Period,
            63 => KeyCode.Slash,
            64 => KeyCode.Alpha2,
            94 => KeyCode.Alpha6,
            95 => KeyCode.Minus,
            123 => KeyCode.LeftBracket,
            124 => KeyCode.Backslash,
            125 => KeyCode.RightBracket,
            126 => KeyCode.BackQuote,
            _ => key
        };
    }
}
