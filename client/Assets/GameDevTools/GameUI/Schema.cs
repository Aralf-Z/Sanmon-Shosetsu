using System;
using UnityEngine;

namespace GameConsole.UI
{
    /// <summary> Schema 资源中可编辑的名称与资源路径映射, Player 无需枚举文件系统. </summary>
    public sealed class Schema : ScriptableObject
    {
        public ToolkitStyleEntry[] styles = Array.Empty<ToolkitStyleEntry>();

        [InspectorName("字体")]
        public ToolkitFontEntry[] fonts = Array.Empty<ToolkitFontEntry>();

        /// <summary> 工具独立语言包, 首个有效项作为默认和缺词回退, 路径相对于 Resources. </summary>
        [InspectorName("语言")]
        public ToolkitLanguageEntry[] languages = Array.Empty<ToolkitLanguageEntry>();
    }

    /// <summary> 一个风格的显示名称和 Resources 相对目录. </summary>
    [Serializable]
    public sealed class ToolkitStyleEntry
    {
        [InspectorName("名称")]
        public string name = string.Empty;

        [InspectorName("路径")]
        public string path = string.Empty;
    }

    /// <summary> 共用字体的显示名称和 Resources/Font 下的加载路径. </summary>
    [Serializable]
    public sealed class ToolkitFontEntry
    {
        [InspectorName("名称")]
        public string name = string.Empty;

        [InspectorName("路径")]
        public string path = string.Empty;
    }

    /// <summary> 语言代码, 原生名称和语言包目录. </summary>
    [Serializable]
    public sealed class ToolkitLanguageEntry
    {
        [InspectorName("代码")]
        public string code = string.Empty;

        [InspectorName("名称")]
        public string name = string.Empty;

        [InspectorName("路径")]
        public string path = string.Empty;
    }
}
