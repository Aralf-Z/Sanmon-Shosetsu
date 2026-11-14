using System.Collections.Generic;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    public static partial class ToolkitFactory
    {
        private static readonly Dictionary<VisualElement, ToolkitTextOpacity> _menuOpacity = new(); // 原生菜单脱离 desktop, 单独登记同一套透明度行为.

        // 独立根只登记一次, 附加前准备的菜单也会在 Attach 时重新解析主题.
        private static void ApplyMenuOpacity(VisualElement root)
        {
            if (root == null || _menuOpacity.ContainsKey(root)) return;
            _menuOpacity.Add(root, new ToolkitTextOpacity(root));
        }

        // 菜单关闭后立即恢复原生样式并释放引用.
        private static void ReleaseMenuOpacity(VisualElement root)
        {
            if (!_menuOpacity.TryGetValue(root, out var opacity)) return;
            _menuOpacity.Remove(root);
            opacity.Dispose();
        }
    }
}
