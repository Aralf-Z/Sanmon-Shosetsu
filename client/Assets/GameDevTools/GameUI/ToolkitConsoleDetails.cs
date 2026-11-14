using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 固定宽度的日志堆栈侧栏, 仅运行时保存开关, 窄窗口覆盖日志区. </summary>
    internal sealed class ToolkitConsoleDetails : IDisposable
    {
        private const float PANEL_WIDTH = 360f; // 原最小宽度 240 的一点五倍.

        /// <summary> 日志详情正在进行原生文本选择, 不使用控制台聚焦快捷键. </summary>
        internal bool IsTextFocused => _panel.panel?.focusController.focusedElement is VisualElement focus && _panel.Contains(focus);
        /// <summary> 单击堆栈条目时通知原始行文本, 当前仅预留扩展入口. </summary>
        internal event Action<string> e_onStackEntryClicked;
        private readonly VisualElement _columns; // 日志与详情的可用区域.
        private readonly VisualElement _panel; // 宽屏并排, 窄屏覆盖.
        private readonly Button _toggle; // 右边界中央的独立箭头.
        private readonly Label _metadata; // 类型, 时间和出现次数.
        private readonly VisualElement _stack; // 独立堆栈条目的宿主, 不使用输入框的光标滚动.
        private readonly ScrollView _stackScroll; // 独立的滚动宿主, 可视高度与文本内容高度分离.
        private readonly Button _copyStack; // 复制实际堆栈, 不包含占位提示或元数据.
        private string _stackText = string.Empty; // 当前选中日志提供的原始堆栈.
        private string _renderedStackText; // 避免相同堆栈重复创建节点.
        private bool _renderedPlaceholder; // 区分无堆栈提示与实际堆栈文本.
        private Label _selectedEntry; // 当前高亮的堆栈条目.
        private Label _pressedEntry; // 原生文字选择不被条目点击处理接管.
        private int _pressedPointer = -1; // 当前候选点击的指针编号.
        private Vector2 _pressPosition; // 拖选开始的位置, 单位为面板单位.
        private Vector2 _pressScrollOffset; // 滚动过程中不触发条目点击.
        private bool _dragged; // 指针移动超过容差后取消候选点击.
        private bool _open; // 初次运行关闭, 切页和关闭窗口不重置.
        private bool _dirty = true; // 每帧最多应用一次几何变化.
        private bool _hasShown; // 首次显示需要初始化按钮状态.
        private ConsoleLogItem? _shownItem; // 无关日志更新不重建当前详情文本.

        /// <summary> 绑定静态侧栏和边界箭头. </summary>
        internal ToolkitConsoleDetails(VisualElement root)
        {
            _columns = ToolkitView.Require<VisualElement>(root, "consoleColumns");
            _panel = ToolkitView.Require<VisualElement>(root, "consoleLogDetails");
            _toggle = ToolkitView.Require<Button>(root, "toggleConsoleLogDetails");
            _metadata = ToolkitView.Require<Label>(root, "consoleLogMetadata");
            _stack = ToolkitView.Require<VisualElement>(root, "consoleLogStack");
            _stackScroll = ToolkitView.Require<ScrollView>(root, "consoleStackScroll");
            _copyStack = ToolkitView.Require<Button>(root, "copyConsoleLogStack");
            _stack.RegisterCallback<PointerDownEvent>(StackPointerDown, TrickleDown.TrickleDown);
            _stack.RegisterCallback<PointerMoveEvent>(StackPointerMove, TrickleDown.TrickleDown);
            _stack.RegisterCallback<PointerCancelEvent>(StackPointerCancel, TrickleDown.TrickleDown);
            _stack.RegisterCallback<ClickEvent>(StackClick, TrickleDown.TrickleDown);
            _stackScroll.mode = ScrollViewMode.Vertical;
            _stackScroll.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
            _stackScroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _stackScroll.touchScrollBehavior = ScrollView.TouchScrollBehavior.Clamped;
            _copyStack.clicked += CopyStack;
            _toggle.clicked += Toggle;
            _columns.RegisterCallback<GeometryChangedEvent>(Geometry);
            Show(null);
        }

        /// <summary> 更新选中日志, 不自动展开侧栏. </summary>
        internal void Show(ConsoleLogItem? item)
        {
            if (_hasShown && (!item.HasValue && !_shownItem.HasValue || item.HasValue && _shownItem.HasValue && item.Value.Id == _shownItem.Value.Id && ReferenceEquals(item.Value.Record, _shownItem.Value.Record) && item.Value.Count == _shownItem.Value.Count)) return;
            var selectionChanged = _shownItem?.Id != item?.Id;
            _hasShown = true;
            _shownItem = item;
            if (!item.HasValue)
            {
                _metadata.text = ToolkitLocalization.Get("ui.ConsoleDetails.Show");
                _stackText = string.Empty;
                SetStackText(string.Empty, selectionChanged);
                _copyStack.SetEnabled(false);
                return;
            }
            var value = item.Value;
            var record = value.Record;
            var kind = ConsoleLogStore.Category(record.Type) switch { 1 => ToolkitLocalization.Get("ui.ConsoleDetails.Show.2"), 2 => ToolkitLocalization.Get("icon.warning"), 4 => ToolkitLocalization.Get("icon.dev"), _ => ToolkitLocalization.Get("icon.error") };
            _metadata.text = ToolkitLocalization.Format("ui.ConsoleDetails.Show.6", ("p0", (object)(kind)), ("p1", (object)ToolkitLocalization.Get("log.type." + record.Type)), ("p2", (object)(ToolkitLocalization.Value((object)(record.Time), "HH:mm:ss.fff", 0))), ("p3", (object)(value.Count)));
            _stackText = record.IsUserInput ? ToolkitLocalization.Get("console.stack.userInput") : record.StackTrace;
            _copyStack.SetEnabled(!string.IsNullOrEmpty(_stackText));
            var stack = string.IsNullOrEmpty(_stackText) ? ToolkitLocalization.Get("ui.ConsoleDetails.Show.7") : _stackText;
            SetStackText(stack, selectionChanged, string.IsNullOrEmpty(_stackText));
        }

        /// <summary> 固定宽 360, 不能同时保留日志区 280 时使用覆盖布局. </summary>
        internal void Tick()
        {
            if (!_dirty) return;
            var available = _columns.contentRect.width;
            if (available <= 0 || float.IsNaN(available) || float.IsInfinity(available)) return;
            var narrow = available < 280f + PANEL_WIDTH + 16f;
            _panel.style.width = Mathf.Min(PANEL_WIDTH, available);
            _panel.style.position = narrow ? Position.Absolute : Position.Relative;
            _panel.EnableInClassList("console-details-overlay", narrow);
            _panel.EnableInClassList("is-hidden", !_open);
            _toggle.text = _open ? ToolkitLocalization.Get("icon.collapsed") : ToolkitLocalization.Get("icon.expandLeft");
            _toggle.tooltip = _open ? ToolkitLocalization.Get("ui.ConsoleDetails.Tick.3") : ToolkitLocalization.Get("ui.ConsoleDetails.Tick.4");
            _toggle.BringToFront();
            _dirty = false;
        }

        /// <summary> 切页和隐藏时保留开关, 重新显示时按可用宽度更新布局. </summary>
        internal void SetVisible(bool visible) { ClearStackPress(); if (visible) _dirty = true; }

        /// <summary> 更新元数据与占位提示, 实际堆栈和滚动位置保持原样. </summary>
        internal void RefreshLanguage()
        {
            var offset = _stackScroll.scrollOffset;
            var selected = _selectedEntry;
            _hasShown = false;
            Show(_shownItem);
            _stackScroll.scrollOffset = offset;
            if (selected != null && selected.parent != null) SelectStackEntry(selected);
            _dirty = true;
        }

        /// <summary> 清理按钮和几何事件. </summary>
        public void Dispose()
        {
            _toggle.clicked -= Toggle;
            _copyStack.clicked -= CopyStack;
            _columns.UnregisterCallback<GeometryChangedEvent>(Geometry);
            _stack.UnregisterCallback<PointerDownEvent>(StackPointerDown, TrickleDown.TrickleDown);
            _stack.UnregisterCallback<PointerMoveEvent>(StackPointerMove, TrickleDown.TrickleDown);
            _stack.UnregisterCallback<PointerCancelEvent>(StackPointerCancel, TrickleDown.TrickleDown);
            _stack.UnregisterCallback<ClickEvent>(StackClick, TrickleDown.TrickleDown);
            ClearStackPress();
            e_onStackEntryClicked = null;
        }

        private void Toggle() { ClearStackPress(); _open = !_open; _dirty = true; Tick(); } // 箭头始终固定在内容右边界.
        // 只复制日志原始堆栈, 没有堆栈时按钮禁用.
        private void CopyStack() { if (!string.IsNullOrEmpty(_stackText)) GUIUtility.systemCopyBuffer = _stackText; }
        // 仅切换日志或实际堆栈变化时回到顶部, 普通日志更新不干预用户拖动.
        private void SetStackText(string text, bool resetPosition, bool placeholder = false)
        {
            if (_renderedStackText != text || _renderedPlaceholder != placeholder)
            {
                SelectStackEntry(null);
                ClearStackPress();
                _stack.Clear();
                _renderedStackText = text;
                _renderedPlaceholder = placeholder;
                if (placeholder) _stack.Add(CreateStackLabel(text, false));
                else if (!string.IsNullOrEmpty(text))
                {
                    var index = 0;
                    foreach (var line in text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None))
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            var gap = new VisualElement { pickingMode = PickingMode.Ignore };
                            gap.AddToClassList("console-stack-gap");
                            _stack.Add(gap);
                            continue;
                        }
                        var entry = CreateStackLabel(line, true);
                        entry.AddToClassList(index++ % 2 == 0 ? "console-stack-entry-dark" : "console-stack-entry-gray");
                        _stack.Add(entry);
                    }
                }
                ToolkitTextOpacity.Track(_stack);
                resetPosition = true;
            }
            if (resetPosition) { SelectStackEntry(null); ClearStackPress(); }
            if (resetPosition) _stackScroll.scrollOffset = Vector2.zero;
        }

        // 原生 Label 负责文字选择, 条目处理只增加高亮且不捕获指针.
        private static Label CreateStackLabel(string text, bool selectable)
        {
            var label = new Label(text) { enableRichText = false, parseEscapeSequences = false, focusable = selectable };
            label.selection.isSelectable = selectable;
            label.AddToClassList("console-stack-text");
            if (selectable) label.AddToClassList("console-stack-entry");
            return label;
        }

        // 仅接受宿主下的实际条目, 空行和占位提示不参与点击.
        private Label FindStackEntry(object target)
        {
            var element = target as VisualElement;
            while (element != null && element != _stack)
            {
                if (element.parent == _stack && element is Label label && label.ClassListContains("console-stack-entry")) return label;
                element = element.parent;
            }
            return null;
        }

        private void StackPointerDown(PointerDownEvent evt)
        {
            ClearStackPress();
            if (evt.button != 0 || !evt.isPrimary) return;
            _pressedEntry = FindStackEntry(evt.target);
            if (_pressedEntry == null) return;
            _pressedPointer = evt.pointerId;
            _pressPosition = evt.position;
            _pressScrollOffset = _stackScroll.scrollOffset;
        }

        private void StackPointerMove(PointerMoveEvent evt)
        {
            if (evt.pointerId == _pressedPointer && ((Vector2)evt.position - _pressPosition).sqrMagnitude > 16f) _dragged = true;
        }

        private void StackPointerCancel(PointerCancelEvent evt) { if (evt.pointerId == _pressedPointer) ClearStackPress(); }

        // ClickEvent 在原生文字选择之后处理, 拖选和多击选择文字不通知业务接口.
        private void StackClick(ClickEvent evt)
        {
            var entry = FindStackEntry(evt.target);
            var select = entry != null && entry == _pressedEntry && evt.pointerId == _pressedPointer && evt.button == 0 && evt.clickCount == 1 && !_dragged && ((Vector2)evt.position - _pressPosition).sqrMagnitude <= 16f && _stackScroll.scrollOffset == _pressScrollOffset && entry.selection.cursorIndex == entry.selection.selectIndex;
            ClearStackPress();
            if (!select) return;
            SelectStackEntry(entry);
            e_onStackEntryClicked?.Invoke(entry.text);
        }

        private void ClearStackPress() { _pressedEntry = null; _pressedPointer = -1; _dragged = false; }

        private void SelectStackEntry(Label entry)
        {
            if (_selectedEntry == entry) return;
            _selectedEntry?.RemoveFromClassList("console-stack-entry-selected");
            _selectedEntry = entry;
            _selectedEntry?.AddToClassList("console-stack-entry-selected");
        }

        private void Geometry(GeometryChangedEvent evt) { if (evt.oldRect.size != evt.newRect.size) _dirty = true; }
    }
}
