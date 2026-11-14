using System;
using System.Linq;

namespace GameConsole.Extension
{
    /// <summary> 宏编辑器共享的有限配色, 配置只接受预选颜色. </summary>
    internal static class MacroSyntaxColors
    {
        /// <summary> 等待语句默认淡蓝色. </summary>
        internal const string defaultWait = "#9CDCFE";
        /// <summary> 循环语句默认淡绿色. </summary>
        internal const string defaultRepeat = "#B5CEA8";
        /// <summary> 固定错误颜色. </summary>
        internal const string error = "#F27A5F";
        /// <summary> 正文默认颜色. </summary>
        internal const string text = "#E2E8F0";
        /// <summary> 预选颜色值, 与名称按索引对应. </summary>
        internal static readonly string[] palette = { defaultWait, defaultRepeat, "#DCDCAA", "#CE9178", "#C586C0", text };
        /// <summary> 预选颜色名称. </summary>
        internal static string[] names => new[] { ToolkitLocalization.Get("macro.color.lightBlue"), ToolkitLocalization.Get("macro.color.lightGreen"), ToolkitLocalization.Get("macro.color.lightYellow"), ToolkitLocalization.Get("macro.color.orange"), ToolkitLocalization.Get("macro.color.purple"), ToolkitLocalization.Get("macro.color.white") };

        /// <summary> 规范化旧配置或手动修改的配置. </summary>
        internal static string Normalize(string value, string fallback) => palette.FirstOrDefault(color => string.Equals(color, value, StringComparison.OrdinalIgnoreCase)) ?? fallback;
    }
}
