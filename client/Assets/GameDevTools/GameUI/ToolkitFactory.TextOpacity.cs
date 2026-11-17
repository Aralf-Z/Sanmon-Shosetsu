using System.Collections.Generic;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    public static partial class ToolkitFactory
    {
        private static readonly Dictionary<VisualElement, ToolkitTextOpacity> _menuOpacity = new();

        private static void ApplyMenuOpacity(VisualElement root)
        {
            if (root == null || _menuOpacity.ContainsKey(root)) return;

            _menuOpacity.Add(root, new ToolkitTextOpacity(root));
        }

        private static void ReleaseMenuOpacity(VisualElement root)
        {
            if (!_menuOpacity.TryGetValue(root, out var opacity)) return;

            _menuOpacity.Remove(root);
            opacity.Dispose();
        }
    }
}
