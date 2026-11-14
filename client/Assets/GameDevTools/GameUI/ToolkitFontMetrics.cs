using System;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 使用已解析字体样式识别测量变化, 不依赖配置通知与布局更新的先后顺序. </summary>
    internal static class ToolkitFontMetrics
    {
        /// <summary> 外观版本与已解析字体参数共同决定文本测量是否失效. </summary>
        internal static int GetKey(VisualElement element)
        {
            var style = element.resolvedStyle;
            return HashCode.Combine(ToolkitFactory.AppearanceVersion, ToolkitLocalization.Version, style.unityFontDefinition, style.unityFont, style.unityFontStyleAndWeight, style.fontSize, style.letterSpacing, style.wordSpacing);
        }
    }
}
