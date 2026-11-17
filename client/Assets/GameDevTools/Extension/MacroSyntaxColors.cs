using System;
using System.Linq;

namespace GameConsole.Extension
{
    /// <summary> 宏编辑器共享的有限配色, 配置只接受预选颜色. </summary>
    internal static class MacroSyntaxColors
    {
        internal const string defaultWait = "#9CDCFE";

        internal const string defaultRepeat = "#B5CEA8";

        internal const string error = "#F27A5F";

        internal const string text = "#E2E8F0";

        internal static readonly string[] palette =
        { defaultWait, defaultRepeat, "#DCDCAA", "#CE9178", "#C586C0", text };

        internal static string[] names => new[]
        { ToolkitLocalization.Get("macro.color.lightBlue"), ToolkitLocalization.Get("macro.color.lightGreen"), ToolkitLocalization.Get("macro.color.lightYellow"), ToolkitLocalization.Get("macro.color.orange"), ToolkitLocalization.Get("macro.color.purple"), ToolkitLocalization.Get("macro.color.white") };

        internal static string Normalize(string value, string fallback) => palette.FirstOrDefault(color => string.Equals(color, value, StringComparison.OrdinalIgnoreCase)) ?? fallback;
    }
}
