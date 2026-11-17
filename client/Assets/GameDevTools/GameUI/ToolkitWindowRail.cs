using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 将页签和详情定位到主窗口左侧, 不参与主面板布局. </summary>
    internal sealed class ToolkitWindowRail : IDisposable
    {
        private readonly VisualElement _root;

        private readonly VisualElement _header;

        private readonly VisualElement _tabs; // 保留原贴边外观的页签容器.

        private readonly VisualElement _details;

        private readonly Button[] _buttons;

        private Rect _bounds;

        private Vector2 _viewport;

        internal ToolkitWindowRail(VisualElement root)
        {
            _root = ToolkitView.Require<VisualElement>(root, "windowRail");
            _header = ToolkitView.Require<VisualElement>(root, "windowHeader");
            _tabs = ToolkitView.Require<VisualElement>(root, "windowSideTabs");
            _details = ToolkitView.Require<VisualElement>(root, "commandHoverDetails");
            _buttons = new[]
            { ToolkitView.Require<Button>(root, "consoleTab"), ToolkitView.Require<Button>(root, "cheatTab"), ToolkitView.Require<Button>(root, "macroTab"), ToolkitView.Require<Button>(root, "optionTab") };
            _header.RegisterCallback<GeometryChangedEvent>(OnHeaderGeometry);
        }

        /// <summary> 预留附属区, 最多使用视口宽度的 1/6, 保证默认主窗口完整显示. </summary>
        internal static float ReservedWidth(float viewportWidth) => Mathf.Min(220f, Mathf.Max(0, viewportWidth) / 6f);

        /// <summary> 跟随主窗口移动, 不改变其内容区宽度. </summary>
        internal void Apply(Rect bounds, Vector2 viewport)
        {
            _bounds = bounds;
            _viewport = viewport;
            var width = ReservedWidth(viewport.x);
            var headerHeight = _header.layout.height;

            if (float.IsNaN(headerHeight) || headerHeight <= 0)
            {
                headerHeight = 44f;
            }

            var top = bounds.y + headerHeight;
            var height = Mathf.Max(0, bounds.height - headerHeight);
            var tabHeight = Mathf.Min(34f, height / _buttons.Length);

            _root.style.width = width;
            _root.style.height = height;
            _root.style.translate = new Translate(bounds.x - width, top, 0);
            foreach (var button in _buttons)
            {
                button.style.width = Mathf.Min(96f, width);
                button.style.height = tabHeight;
            }

            _details.style.maxHeight = Mathf.Max(0, height - tabHeight * _buttons.Length - 8f);
            _root.EnableInClassList("rail-no-details", height - tabHeight * _buttons.Length < 88f);
            _tabs.style.flexShrink = 0;
        }

        internal void SetVisible(bool visible) => _root.EnableInClassList("is-hidden", !visible);

        public void Dispose()
        {
            _header.UnregisterCallback<GeometryChangedEvent>(OnHeaderGeometry);
            SetVisible(false);
        }

        private void OnHeaderGeometry(GeometryChangedEvent evt) => Apply(_bounds, _viewport);
    }
}
