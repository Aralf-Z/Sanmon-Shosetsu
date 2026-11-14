using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 风格选择与搜索历史共用的滚动弹层, 复用当前风格模板和悬停反馈. </summary>
    internal sealed class ToolkitDropdownPopup : IDisposable
    {
        private const float ROW_HEIGHT = 30f; // 与历史按钮 USS 高度一致, 单位为面板单位.
        private const float BORDER_WIDTH = 1f; // 弹层边框单边宽度.
        private const float EDGE_MARGIN = 4f; // 弹层与视口边缘的间距.

        /// <summary> 当前是否挂载到面板最上层. </summary>
        internal bool IsOpen => _root != null;

        private readonly VisualElement _focusTarget; // 选取或取消后返回的控件.
        private readonly VisualElement _trigger; // 用于定位的按钮或其他控件.
        private readonly int _visibleCount; // 当前用途允许同时显示的最大行数.
        private readonly bool _compact; // 搜索历史保留原三分之一宽度, 普通选择匹配按钮宽度.
        private Action<int> _select; // 使用快照索引返回选项, 不依赖显示名称唯一.
        private readonly TemplateContainer _host; // 拦截弹层外部点击的透明覆盖层.
        private readonly VisualElement _popup; // 由本类直接控制位置和大小.
        private readonly ScrollView _scroll; // 独立原生滚动控件, 支持滚轮及滑块.
        private readonly ToolkitTextDisplay _text = new(); // 只省略展示文字, tooltip 和选择回调保留完整历史词.
        private readonly IVisualElementScheduledItem _textUpdate; // 仅弹层打开时合并文字和字体的测量更新.
        private VisualElement _root; // 打开时所在的面板根节点.
        private int _count; // 本次打开的选项数量快照.
        private int _revision; // 旧行回调不能选中重开菜单中的新选项.
        private bool _disposed; // 销毁后禁止再次打开.

        /// <summary> 创建独立模板并只注册自己的事件. </summary>
        internal ToolkitDropdownPopup(VisualElement anchor, VisualElement focusTarget, int visibleCount, bool compact = false)
        {
            _focusTarget = focusTarget;
            _trigger = anchor;
            _visibleCount = Math.Max(1, visibleCount);
            _compact = compact;
            _host = ToolkitFactory.InstantiateTemplate("ToolkitSearchHistoryPopup");
            _host.AddToClassList("search-history-layer");
            _host.pickingMode = PickingMode.Position;
            _host.focusable = true;
            _popup = ToolkitView.Require<VisualElement>(_host, "searchHistoryPopup");
            _scroll = ToolkitView.Require<ScrollView>(_host, "searchHistoryScroll");
            _textUpdate = _host.schedule.Execute(_text.Tick).Every(16);
            _textUpdate.Pause();
            _host.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            _host.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            _focusTarget.RegisterCallback<DetachFromPanelEvent>(OnDetached);
            _trigger.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        /// <summary> 保留全部选项, 超出可见行数时通过原生滚动条和滚轮查看. </summary>
        internal void Show(IReadOnlyList<string> choices, Action<int> select, string emptyMessage = null)
        {
            emptyMessage ??= ToolkitLocalization.Get("ui.DropdownPopup.Show");
            Close();
            if (_disposed || _trigger.panel == null) return;
            _select = select;
            _count = choices.Count;
            var revision = ++_revision;
            _scroll.Clear();
            if (_count == 0)
            {
                var empty = new Label(emptyMessage) { tooltip = emptyMessage };
                empty.AddToClassList("search-history-empty");
                _text.Track(empty);
                _scroll.Add(empty);
            }
            for (var index = 0; index < choices.Count; index++)
            {
                var choice = index;
                var text = choices[index];
                var button = new Button(() => Select(choice, revision)) { text = text, tooltip = text };
                button.AddToClassList("search-history-entry");
                button.enableRichText = false;
                button.parseEscapeSequences = false;
                _text.Track(button);
                // 显式维护悬停类, 不依赖原生菜单对伪状态的内部处理.
                button.RegisterCallback<PointerEnterEvent>(evt => button.AddToClassList("search-history-hovered"));
                button.RegisterCallback<PointerLeaveEvent>(evt => button.RemoveFromClassList("search-history-hovered"));
                _scroll.Add(button);
            }
            _scroll.verticalScrollerVisibility = _count > _visibleCount ? ScrollerVisibility.AlwaysVisible : ScrollerVisibility.Auto;
            _root = ToolkitView.GetOverlayRoot(_trigger);
            _root.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            _root.Add(_host);
            Application.focusChanged += OnFocusChanged;
            UpdateBounds();
            _scroll.scrollOffset = Vector2.zero;
            _host.Focus();
            _textUpdate.Resume();
        }

        /// <summary> 取消或隐藏时移除弹层, 不修改历史或搜索词. </summary>
        internal void Close(bool restoreFocus = false)
        {
            if (!IsOpen) return;
            _root.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            _root = null;
            _revision++;
            Application.focusChanged -= OnFocusChanged;
            _select = null;
            _textUpdate.Pause();
            _text.Dispose();
            _host.RemoveFromHierarchy();
            _scroll.Clear();
            if (restoreFocus && _focusTarget.panel != null) _focusTarget.Focus();
        }

        /// <summary> 清理自己的覆盖层和事件, 不影响其他搜索实例. </summary>
        public void Dispose()
        {
            Close();
            _disposed = true;
            _host.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            _host.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            _focusTarget.UnregisterCallback<DetachFromPanelEvent>(OnDetached);
            _trigger.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        /// <summary> 按用途限制可见行数, 空间不足时上下选择可用空间并保留滚动. </summary>
        internal static Rect CalculateBounds(Rect viewport, Rect anchor, float preferredWidth, int count, int visibleCount, bool compact)
        {
            var availableWidth = Mathf.Max(0, viewport.width - EDGE_MARGIN * 2);
            var width = compact ? Mathf.Min(availableWidth, Mathf.Clamp(preferredWidth, 220f, 420f)) / 3f : Mathf.Min(availableWidth, Mathf.Max(0, preferredWidth));
            var desiredHeight = Math.Min(Math.Max(1, visibleCount), Math.Max(1, count)) * ROW_HEIGHT + BORDER_WIDTH * 2;
            var top = Mathf.Clamp(anchor.yMin, viewport.yMin + EDGE_MARGIN, Mathf.Max(viewport.yMin + EDGE_MARGIN, viewport.yMax - EDGE_MARGIN));
            var bottom = Mathf.Clamp(anchor.yMax, viewport.yMin + EDGE_MARGIN, Mathf.Max(viewport.yMin + EDGE_MARGIN, viewport.yMax - EDGE_MARGIN));
            var below = Mathf.Max(0, viewport.yMax - EDGE_MARGIN - bottom);
            var above = Mathf.Max(0, top - viewport.yMin - EDGE_MARGIN);
            var openBelow = desiredHeight <= below || below >= above;
            var height = Mathf.Min(desiredHeight, openBelow ? below : above);
            var x = Mathf.Clamp(anchor.xMax - width, viewport.xMin + EDGE_MARGIN, Mathf.Max(viewport.xMin + EDGE_MARGIN, viewport.xMax - EDGE_MARGIN - width));
            return new Rect(x, openBelow ? bottom : top - height, width, height);
        }

        // 弹层自身的明确边界决定滚动视口, 不依赖内部菜单或内容的自然高度.
        private void UpdateBounds()
        {
            if (!IsOpen) return;
            var anchor = new Rect(_root.WorldToLocal(_trigger.worldBound.min), _trigger.worldBound.size);
            var preferredWidth = _compact ? _trigger.worldBound.xMax - _focusTarget.worldBound.xMin : _trigger.worldBound.width;
            var bounds = CalculateBounds(_root.contentRect, anchor, preferredWidth, _count, _visibleCount, _compact);
            _popup.style.left = bounds.x;
            _popup.style.top = bounds.y;
            _popup.style.width = bounds.width;
            _popup.style.height = bounds.height;
        }

        // 点击后先卸下覆盖层, 再填入当前搜索框并恢复焦点.
        private void Select(int index, int revision)
        {
            if (!IsOpen || revision != _revision) return;
            var select = _select;
            Close();
            select?.Invoke(index);
            if (_focusTarget.panel != null) _focusTarget.Focus();
        }

        // 通过实际矩形判断, 滚动条及其内部控件均属于弹层, 不截断拖动.
        private void OnPointerDown(PointerDownEvent evt)
        {
            if (_popup.worldBound.Contains(evt.position)) return;
            Close(true);
            evt.StopImmediatePropagation();
            evt.PreventDefault();
        }

        // Esc 仅关闭当前弹层, 不继续传给下层窗口或说明抽屉.
        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Escape) return;
            Close(true);
            evt.StopImmediatePropagation();
            evt.PreventDefault();
        }

        // 锚点或视口变化时直接重新限制位置, 不影响当前滚动偏移.
        private void OnGeometryChanged(GeometryChangedEvent evt) => UpdateBounds();

        // 应用失焦时关闭弹层, 下次打开重新建立选项快照.
        private void OnFocusChanged(bool focused) { if (!focused) Close(); }

        // 文档销毁或控件离开面板时清理独立的覆盖层.
        private void OnDetached(DetachFromPanelEvent evt) => Close();
    }
}
