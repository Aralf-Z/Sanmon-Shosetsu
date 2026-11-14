using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 将页签和详情定位到主窗口左侧, 不参与主面板布局. </summary>
    internal sealed class ToolkitWindowRail : IDisposable
    {
        private readonly VisualElement _root; // 不阻挡空白区域拾取的侧边根节点.
        private readonly VisualElement _header; // 页签起点与标题行底部对齐.
        private readonly VisualElement _tabs; // 保留原贴边外观的页签容器.
        private readonly VisualElement _details; // 独立的自适应详情卡片, 最大高度不超过主框.
        private readonly Button[] _buttons; // 极小视口下同步压缩页签宽高.
        private Rect _bounds; // 最新主窗口几何.
        private Vector2 _viewport; // 最新视口尺寸.

        /// <summary> 绑定外侧布局, 标题高度改变后重新对齐. </summary>
        internal ToolkitWindowRail(VisualElement root)
        {
            _root = ToolkitView.Require<VisualElement>(root, "windowRail");
            _header = ToolkitView.Require<VisualElement>(root, "windowHeader");
            _tabs = ToolkitView.Require<VisualElement>(root, "windowSideTabs");
            _details = ToolkitView.Require<VisualElement>(root, "commandHoverDetails");
            _buttons = new[] { ToolkitView.Require<Button>(root, "consoleTab"), ToolkitView.Require<Button>(root, "cheatTab"), ToolkitView.Require<Button>(root, "macroTab"), ToolkitView.Require<Button>(root, "optionTab") };
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
                headerHeight = 44f;
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

        /// <summary> 最小化或关闭时整组附属控件随主窗口隐藏. </summary>
        internal void SetVisible(bool visible) => _root.EnableInClassList("is-hidden", !visible);

        /// <summary> 解除标题高度监听并隐藏附属控件. </summary>
        public void Dispose()
        {
            _header.UnregisterCallback<GeometryChangedEvent>(OnHeaderGeometry);
            SetVisible(false);
        }

        // 紧凑标题样式切换后保持页签贴边位置正确.
        private void OnHeaderGeometry(GeometryChangedEvent evt) => Apply(_bounds, _viewport);
    }
}
