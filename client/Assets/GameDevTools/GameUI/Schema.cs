using System;
using UnityEngine;

namespace GameConsole.UI
{
    /// <summary> Schema 资源中可编辑的名称与资源路径映射, Player 无需枚举文件系统. </summary>
    public sealed class Schema : ScriptableObject
    {
        /// <summary> 风格列表, 首个有效项作为默认, 显示名称可独立于资源目录修改. </summary>
        public ToolkitStyleEntry[] styles = Array.Empty<ToolkitStyleEntry>();
        /// <summary> 共用字体列表, 首个有效项作为默认, 路径相对于 Resources, 不含扩展名. </summary>
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
        /// <summary> 选项菜单直接显示的名称, 不随语言变化, 不作为加载或保存标识. </summary>
        [InspectorName("名称")]
        public string name = string.Empty;
        /// <summary> Resources 下的相对目录, 例如 Style/YourStyle. </summary>
        [InspectorName("路径")]
        public string path = string.Empty;
    }

    /// <summary> 共用字体的显示名称和 Resources/Font 下的加载路径. </summary>
    [Serializable]
    public sealed class ToolkitFontEntry
    {
        /// <summary> 字体选择菜单直接显示的名称, 不随语言变化. </summary>
        [InspectorName("名称")]
        public string name = string.Empty;
        /// <summary> Resources 加载键, 例如 Font/YourFont, 不含扩展名. </summary>
        [InspectorName("路径")]
        public string path = string.Empty;
    }

    /// <summary> 语言代码, 原生名称和语言包目录. </summary>
    [Serializable]
    public sealed class ToolkitLanguageEntry
    {
        /// <summary> 保存到配置中的稳定语言代码. </summary>
        [InspectorName("代码")]
        public string code = string.Empty;
        /// <summary> 语言菜单显示的原生名称. </summary>
        [InspectorName("名称")]
        public string name = string.Empty;
        /// <summary> Resources 相对目录, 例如 Localization/语言目录. </summary>
        [InspectorName("路径")]
        public string path = string.Empty;
    }
}
