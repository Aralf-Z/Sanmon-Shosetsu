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

        private const float EDITOR_MIN = 280f;

        private const float GAP_WIDTH = 24f; // 两侧保留原分隔条占用的空白.

        private static bool ValidSize(float value) => value > 0 && !float.IsNaN(value) && !float.IsInfinity(value);

        private readonly VisualElement _root;

        private readonly ScrollView _scroll;

        private readonly VisualElement _columns;

        private readonly VisualElement _right;

        private readonly VisualElement _panel;

        private readonly VisualElement _drawer;

        private readonly VisualElement _backdrop;

        private readonly VisualElement _rightGap;

        private readonly Button _toggle;

        private bool _narrow;

        private bool _open;

        private bool _dirty = true;

        private bool _visible;

        private int _appearanceVersion = -1;

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
            _root.RegisterCallback<AttachToPanelEvent>(Attached);
        }

        /// <summary> 仅根据可用尺寸切换三栏或抽屉, 侧栏不参与伸缩. </summary>
        internal void Tick()
        {
            if (!_visible || _root.panel == null) return;

            if (_appearanceVersion != ToolkitFactory.AppearanceVersion)
            {
                _appearanceVersion = ToolkitFactory.AppearanceVersion;
                _dirty = true;
            }

            if (!_dirty) return;

            var size = _scroll.contentViewport.contentRect.size;

            if (!ValidSize(size.x) || !ValidSize(size.y)) return;

            SetNarrow(size.x < LEFT_WIDTH + RIGHT_WIDTH + EDITOR_MIN + GAP_WIDTH * 2);
            ApplySize(size);
            _dirty = false;
        }

        private void SetNarrow(bool narrow)
        {
            if (_narrow == narrow) return;

            _narrow = narrow;
            _open = false;
            if (narrow)
            {
                _drawer.Add(_panel);
            }
            else
            {
                _right.Add(_panel);
            }

            _scroll.scrollOffset = Vector2.zero;
        }

        private void ApplySize(Vector2 size)
        {
            _columns.style.width = _narrow
                ? Mathf.Max(size.x, LEFT_WIDTH + GAP_WIDTH + EDITOR_MIN)
                : size.x;
            _columns.style.height = size.y;
            _drawer.style.width = Mathf.Min(size.x, RIGHT_WIDTH);
            _right.EnableInClassList("is-hidden", _narrow);
            _rightGap.EnableInClassList("is-hidden", _narrow);
            _toggle.EnableInClassList("is-hidden", !_narrow);
            ApplyDrawer();
        }

        /// <summary> 隐藏时关闭说明抽屉, 保留编辑内容. </summary>
        internal void SetVisible(bool visible)
        {
            _visible = visible;
            if (!visible)
            {
                Close();
            }
            else
            {
                _dirty = true;
            }
        }

        public void Dispose()
        {
            Close();
            _toggle.clicked -= Toggle;
            _backdrop.UnregisterCallback<PointerDownEvent>(Outside);
            _root.UnregisterCallback<KeyDownEvent>(KeyDown, TrickleDown.TrickleDown);
            _scroll.contentViewport.UnregisterCallback<GeometryChangedEvent>(Geometry);
            _root.UnregisterCallback<AttachToPanelEvent>(Attached);
        }

        private void Attached(AttachToPanelEvent evt) => _dirty = true;

        private void Toggle()
        {
            if (_narrow)
            {
                _open = !_open;
                ApplyDrawer();
            }
        }

        private void Close()
        {
            _open = false;
            ApplyDrawer();
        }

        private void ApplyDrawer()
        {
            _drawer.EnableInClassList("is-hidden", !_narrow || !_open);
            _backdrop.EnableInClassList("is-hidden", !_narrow || !_open);
        }

        private void Outside(PointerDownEvent evt)
        {
            if (evt.button == 0)
            {
                Close();
                evt.StopImmediatePropagation();
            }
        }

        private void KeyDown(KeyDownEvent evt)
        {
            if (_open && evt.keyCode == KeyCode.Escape)
            {
                Close();
                evt.StopImmediatePropagation();
            }
        }

        private void Geometry(GeometryChangedEvent evt)
        {
            if (evt.oldRect.size != evt.newRect.size)
            {
                _dirty = true;
            }
        }
    }
}
