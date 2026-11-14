using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> Macro 固定侧栏与窄窗口说明抽屉, 中央编辑器占据剩余空间. </summary>
    internal sealed class ToolkitMacroLayout : IDisposable
    {
        private const float LEFT_WIDTH = 180f; // 宏列表固定宽度, 单位为面板单位.
        private const float RIGHT_WIDTH = 240f; // 语法栏固定宽度, 抽屉仅在极窄窗口内受限.
        private const float EDITOR_MIN = 280f; // 中央编辑器最小可用宽度.
        private const float GAP_WIDTH = 24f; // 两侧保留原分隔条占用的空白.
        private readonly VisualElement _root; // 页面内稳定的抽屉与键盘事件宿主.
        private readonly ScrollView _scroll; // 极窄窗口允许宏列表和编辑区横向滚动.
        private readonly VisualElement _columns; // 三栏主布局.
        private readonly VisualElement _right; // 正常布局的说明宿主.
        private readonly VisualElement _panel; // 共用的说明内容, 只重新挂载不重建.
        private readonly VisualElement _drawer; // 窄布局覆盖抽屉.
        private readonly VisualElement _backdrop; // 抽屉外点击只用于关闭.
        private readonly VisualElement _rightGap; // 说明栏收起时同时移除其前方空白.
        private readonly Button _toggle; // 窄布局显示的说明入口.
        private bool _narrow; // 当前使用抽屉布局.
        private bool _open; // 抽屉当前是否可见.
        private bool _dirty = true; // 一帧最多应用一次几何变化.

        /// <summary> 连接固定侧栏和窄窗口抽屉, 不注册拉伸操纵器. </summary>
        internal ToolkitMacroLayout(VisualElement root)
        {
            _root = ToolkitView.Require<VisualElement>(root, "macroContent");
            _scroll = ToolkitView.Require<ScrollView>(root, "macroColumnsScroll");
            _columns = ToolkitView.Require<VisualElement>(root, "macroColumns");
            _right = ToolkitView.Require<VisualElement>(root, "macroHelpHost");
            _panel = ToolkitView.Require<VisualElement>(root, "macroHelpPanel");
            _drawer = ToolkitView.Require<VisualElement>(root, "macroHelpDrawer");
            _backdrop = ToolkitView.Require<VisualElement>(root, "macroHelpBackdrop");
            _rightGap = ToolkitView.Require<VisualElement>(root, "macroRightGap");
            _toggle = ToolkitView.Require<Button>(root, "toggleMacroHelp");
            _toggle.clicked += Toggle;
            _backdrop.RegisterCallback<PointerDownEvent>(Outside);
            _root.RegisterCallback<KeyDownEvent>(KeyDown, TrickleDown.TrickleDown);
            _scroll.contentViewport.RegisterCallback<GeometryChangedEvent>(Geometry);
        }

        /// <summary> 仅根据可用尺寸切换三栏或抽屉, 侧栏不参与伸缩. </summary>
        internal void Tick()
        {
            if (!_dirty) return;
            var width = _scroll.contentViewport.contentRect.width;
            if (width <= 0 || float.IsNaN(width) || float.IsInfinity(width)) return;
            var narrow = width < LEFT_WIDTH + RIGHT_WIDTH + EDITOR_MIN + GAP_WIDTH * 2;
            if (_narrow != narrow)
            {
                _narrow = narrow;
                _open = false;
                if (narrow) _drawer.Add(_panel); else _right.Add(_panel);
                _scroll.scrollOffset = Vector2.zero;
            }
            _columns.style.width = _narrow ? Mathf.Max(width, LEFT_WIDTH + GAP_WIDTH + EDITOR_MIN) : width;
            _columns.style.height = _scroll.contentViewport.contentRect.height;
            _drawer.style.width = Mathf.Min(width, RIGHT_WIDTH);
            _right.EnableInClassList("is-hidden", _narrow);
            _rightGap.EnableInClassList("is-hidden", _narrow);
            _toggle.EnableInClassList("is-hidden", !_narrow);
            ApplyDrawer();
            _dirty = false;
        }

        /// <summary> 隐藏时关闭说明抽屉, 保留编辑内容. </summary>
        internal void SetVisible(bool visible)
        {
            if (!visible) Close();
            else _dirty = true;
        }

        /// <summary> 解除页面事件. </summary>
        public void Dispose()
        {
            Close();
            _toggle.clicked -= Toggle;
            _backdrop.UnregisterCallback<PointerDownEvent>(Outside);
            _root.UnregisterCallback<KeyDownEvent>(KeyDown, TrickleDown.TrickleDown);
            _scroll.contentViewport.UnregisterCallback<GeometryChangedEvent>(Geometry);
        }

        private void Toggle() { if (_narrow) { _open = !_open; ApplyDrawer(); } } // 抽屉不挤压编辑区.
        private void Close() { _open = false; ApplyDrawer(); } // 离开页面时移除遮罩.

        // 遮罩和抽屉使用正常层级覆盖编辑区.
        private void ApplyDrawer()
        {
            _drawer.EnableInClassList("is-hidden", !_narrow || !_open);
            _backdrop.EnableInClassList("is-hidden", !_narrow || !_open);
        }

        private void Outside(PointerDownEvent evt) { if (evt.button == 0) { Close(); evt.StopImmediatePropagation(); } } // 关闭点击不穿透.
        private void KeyDown(KeyDownEvent evt) { if (_open && evt.keyCode == KeyCode.Escape) { Close(); evt.StopImmediatePropagation(); } } // 优先关闭抽屉.

        // 单纯位置变化不需要重新布局.
        private void Geometry(GeometryChangedEvent evt)
        {
            if (evt.oldRect.size != evt.newRect.size) _dirty = true;
        }
    }
}
