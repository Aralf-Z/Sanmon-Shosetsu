using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 带树形导航的共用分页帮助窗口, 隔离底层交互并保留原区域焦点. </summary>
    internal sealed class ToolkitHelpWindow : IDisposable
    {
        /// <summary> 打开或关闭帮助时通知宿主更新工具内部的快捷键和焦点. </summary>
        internal event Action<bool> e_onVisibilityChanged;
        /// <summary> 当前是否显示模态帮助. </summary>
        internal bool IsOpen { get; private set; }
        /// <summary> 关闭当帧也不把同一次按键交给底层窗口. </summary>
        internal bool BlocksInputThisFrame => IsOpen || _closedFrame == Time.frameCount;

        private readonly VisualElement _root; // 文档根节点, 在最外层拦截帮助键盘事件.
        private readonly VisualElement _host; // 不随主窗口位置移动的覆盖层.
        private readonly VisualElement _backdrop; // 点击外部只关闭帮助, 不穿透.
        private readonly VisualElement _dialog; // 默认键盘焦点宿主.
        private readonly VisualElement _viewport; // 实际可用于正文的尺寸.
        private readonly Label _areaTitle; // 当前主题所属的区域.
        private readonly Label _featureTitle; // 自动附加多页编号的特性标题.
        private readonly Label _body; // 可选择的纯文本正文, 不使用滚动容器.
        private readonly Label _pageInfo; // 整个主题中的页码.
        private readonly Button _close; // 关闭帮助.
        private readonly Button _previous; // 前一页.
        private readonly Button _next; // 后一页.
        private readonly ToolkitHelpNavigation _navigation; // 全部主题与特性的树形入口.
        private readonly ToolkitHelpPlacement _placement; // 标题栏拖动, 每次打开重新居中.
        private readonly List<Action> _unbind = new(); // 各入口的显式事件清理.
        private readonly Dictionary<VisualElement, bool> _background = new(); // 保留底层根节点原有的启用状态.
        private readonly List<ToolkitHelpFeature> _features = new(); // 当前主题替换动态占位符后的文案.
        private List<ToolkitHelpPage> _pages = new(); // 当前可用空间下的分页结果.
        private FocusSnapshot _returnFocus; // 打开前的焦点和文本选区.
        private int _pageIndex; // 当前全局页索引.
        private int _closedFrame = -1; // 关闭帮助的帧号, 防止 Esc 继续关闭底层设置.
        private int _focusVersion; // 避免排队的旧焦点恢复覆盖新弹窗.
        private bool _dirty; // 几何变化合并到每帧一次分页.
        private bool _disposed; // 防止销毁后执行延迟回调.
        private Vector2 _fontSize; // 字体资源就绪或变化后重新测量页面.
        private int _fontKey; // 字体样式变化时重新分页, 即使样本文字尺寸相同.
        private ToolkitHelpTopic _topic; // 当前正文所属的稳定主题.
        private int _requestedFeature = -1; // 等待实际尺寸就绪后跳转的特性.

        /// <summary> 绑定静态模板和区域入口, Cheat 右侧主题在点击时读取. </summary>
        internal ToolkitHelpWindow(VisualElement root, Func<ToolkitHelpTopic> cheatTopic)
        {
            _root = root;
            _host = ToolkitView.Require<VisualElement>(root, "helpWindowHost");
            _backdrop = ToolkitView.Require<VisualElement>(_host, "helpBackdrop");
            _dialog = ToolkitView.Require<VisualElement>(_host, "helpDialog");
            _viewport = ToolkitView.Require<VisualElement>(_host, "helpBodyViewport");
            _areaTitle = ToolkitView.Require<Label>(_host, "helpAreaTitle");
            _featureTitle = ToolkitView.Require<Label>(_host, "helpFeatureTitle");
            _body = ToolkitView.Require<Label>(_host, "helpBody");
            _pageInfo = ToolkitView.Require<Label>(_host, "helpPageInfo");
            _close = ToolkitView.Require<Button>(_host, "closeHelp");
            _previous = ToolkitView.Require<Button>(_host, "previousHelp");
            _next = ToolkitView.Require<Button>(_host, "nextHelp");
            _navigation = new ToolkitHelpNavigation(ToolkitView.Require<ScrollView>(_host, "helpNavigation"), SelectTopic);
            _placement = new ToolkitHelpPlacement(_backdrop, _dialog, ToolkitView.Require<VisualElement>(_host, "helpHeader"));
            _body.enableRichText = false;
            _body.parseEscapeSequences = false;
            _body.selection.isSelectable = true;
            _close.clicked += CloseFromButton;
            _previous.clicked += Previous;
            _next.clicked += Next;
            _viewport.RegisterCallback<GeometryChangedEvent>(Geometry);
            _backdrop.RegisterCallback<PointerDownEvent>(Outside, TrickleDown.TrickleDown);
            _root.RegisterCallback<KeyDownEvent>(KeyDown, TrickleDown.TrickleDown);
            _root.RegisterCallback<FocusInEvent>(FocusIn, TrickleDown.TrickleDown);
            Bind("helpWindow", () => ToolkitHelpTopic.Window);
            Bind("helpConsole", () => ToolkitHelpTopic.Console);
            Bind("helpStack", () => ToolkitHelpTopic.Stack);
            Bind("helpCheatCommands", () => ToolkitHelpTopic.CheatCommands);
            Bind("helpCheatDetails", cheatTopic);
            Bind("helpMacroLibrary", () => ToolkitHelpTopic.MacroLibrary);
            Bind("helpMacroEditor", () => ToolkitHelpTopic.MacroEditor);
            Bind("helpMacroSyntax", () => ToolkitHelpTopic.MacroSyntax);
            Bind("helpConsoleSettings", () => ToolkitHelpTopic.ConsoleSettings);
            Bind("helpInterfaceSettings", () => ToolkitHelpTopic.InterfaceSettings);
            Bind("helpToolSettings", () => ToolkitHelpTopic.ToolSettings);
            Bind("helpCheatSettings", () => ToolkitHelpTopic.CheatSettings);
            Bind("helpFloatingOptions", () => ToolkitHelpTopic.FloatingSettings);
            Bind("helpFloatingSettings", () => ToolkitHelpTopic.FloatingSettings);
        }

        /// <summary> 仅显示时测量, 文本和几何不变时不重新分页. </summary>
        internal void Tick()
        {
            if (!IsOpen || _disposed) return;
            var fontSize = _body.MeasureTextSize(ToolkitLocalization.Get("ui.HelpWindow.Tick"), 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
            var fontKey = ToolkitFontMetrics.GetKey(_body);
            if (_fontSize != fontSize || _fontKey != fontKey) { _fontSize = fontSize; _fontKey = fontKey; _dirty = true; }
            if (!_dirty) return;
            var size = _viewport.contentRect.size;
            if (!Finite(size.x) || !Finite(size.y) || size.x <= 0 || size.y <= 0 || !Finite(fontSize.y) || fontSize.y <= 0) return;
            if (size.y < fontSize.y + 2f)
            {
                _body.text = ToolkitLocalization.Get("ui.HelpWindow.Tick.2");
                _previous.SetEnabled(false);
                _next.SetEnabled(false);
                return;
            }
            var anchor = _pages.Count == 0 ? null : _pages[_pageIndex];
            _pages = ToolkitHelpPagination.Build(_features, size.y - 2f, text => _body.MeasureTextSize(text, size.x, VisualElement.MeasureMode.Exactly, 0, VisualElement.MeasureMode.Undefined).y);
            _pageIndex = 0;
            if (_requestedFeature >= 0)
            {
                var index = _pages.FindIndex(page => page.FeatureIndex == _requestedFeature);
                if (index >= 0) _pageIndex = index;
                _requestedFeature = -1;
            }
            else if (anchor != null)
            {
                for (var i = 0; i < _pages.Count; i++)
                    if (_pages[i].FeatureIndex == anchor.FeatureIndex && _pages[i].Offset <= anchor.Offset) _pageIndex = i;
            }
            _dirty = false;
            ShowPage();
        }

        /// <summary> 生命周期关闭时不恢复已隐藏页面的焦点. </summary>
        internal void Close(bool restoreFocus = false)
        {
            if (!IsOpen) return;
            IsOpen = false;
            _closedFrame = Time.frameCount;
            var version = ++_focusVersion;
            var focus = _returnFocus;
            _returnFocus = null;
            _host.AddToClassList("is-hidden");
            _placement.Cancel();
            foreach (var pair in _background) pair.Key.SetEnabled(pair.Value);
            _background.Clear();
            e_onVisibilityChanged?.Invoke(false);
            if (restoreFocus && focus != null)
                _root.schedule.Execute(() => { if (!_disposed && !IsOpen && _focusVersion == version) focus.Restore(); });
        }

        /// <summary> 保留选中特性和窗口位置, 新语言重新分页到该特性第一页. </summary>
        internal void RefreshLanguage()
        {
            _navigation.RefreshLanguage();
            if (!IsOpen) return;
            var feature = _pages.Count > 0 ? _pages[_pageIndex].FeatureIndex : Math.Max(0, _requestedFeature);
            _pages.Clear();
            SelectTopic(_topic, feature);
        }

        /// <summary> 清理模态状态和所有区域按钮订阅. </summary>
        public void Dispose()
        {
            _disposed = true;
            Close();
            foreach (var unbind in _unbind) unbind();
            _unbind.Clear();
            _close.clicked -= CloseFromButton;
            _previous.clicked -= Previous;
            _next.clicked -= Next;
            _navigation.Dispose();
            _placement.Dispose();
            _viewport.UnregisterCallback<GeometryChangedEvent>(Geometry);
            _backdrop.UnregisterCallback<PointerDownEvent>(Outside, TrickleDown.TrickleDown);
            _root.UnregisterCallback<KeyDownEvent>(KeyDown, TrickleDown.TrickleDown);
            _root.UnregisterCallback<FocusInEvent>(FocusIn, TrickleDown.TrickleDown);
            e_onVisibilityChanged = null;
        }

        // 在按钮取得焦点之前保存输入框的光标, 点击后再读取动态主题.
        private void Bind(string name, Func<ToolkitHelpTopic> topic)
        {
            var button = ToolkitView.Require<Button>(_root, name);
            FocusSnapshot focus = null;
            EventCallback<PointerDownEvent> down = evt => { if (evt.button == 0) focus = new FocusSnapshot(_root); };
            EventCallback<PointerCancelEvent> cancel = evt => focus = null;
            EventCallback<KeyDownEvent> key = evt => focus = new FocusSnapshot(_root);
            Action click = () => { Open(topic(), focus ?? new FocusSnapshot(_root)); focus = null; };
            button.RegisterCallback(down, TrickleDown.TrickleDown);
            button.RegisterCallback(cancel);
            button.RegisterCallback(key, TrickleDown.TrickleDown);
            button.clicked += click;
            _unbind.Add(() => { button.UnregisterCallback(down, TrickleDown.TrickleDown); button.UnregisterCallback(cancel); button.UnregisterCallback(key, TrickleDown.TrickleDown); button.clicked -= click; });
        }

        private void Open(ToolkitHelpTopic topic, FocusSnapshot focus)
        {
            if (_disposed) return;
            if (!IsOpen)
            {
                _returnFocus = focus;
                _focusVersion++;
                IsOpen = true;
                foreach (var child in _host.parent.Children())
                {
                    if (child == _host) continue;
                    _background[child] = child.enabledSelf;
                    child.SetEnabled(false);
                }
                e_onVisibilityChanged?.Invoke(true);
            }
            _host.RemoveFromClassList("is-hidden");
            _host.BringToFront();
            _placement.Reset();
            // 入口始终定位到对应主题第一页, 不沿用上次阅读位置.
            _pages.Clear();
            SelectTopic(topic, 0);
            _close.Focus();
        }

        // 左侧叶页签跳转到该特性的第一页, 翻页仍可继续阅读同一主题的其他特性.
        private void SelectTopic(ToolkitHelpTopic topic, int featureIndex)
        {
            var section = ToolkitHelpContents.Get(topic);
            if (_topic != topic || _pages.Count == 0)
            {
                _topic = topic;
                _features.Clear();
                foreach (var feature in section.Features)
                    _features.Add(new ToolkitHelpFeature(feature.Title, (feature.Body ?? string.Empty).Replace("{ToolKey}", ToolShortcut.DisplayName)));
                _pages.Clear();
                _pageIndex = 0;
                _areaTitle.text = section.Title;
                _featureTitle.text = _features.Count > 0 ? _features[featureIndex].Title : section.Title;
                _body.text = string.Empty;
                _pageInfo.text = "1 / 1";
                _previous.SetEnabled(false);
                _next.SetEnabled(false);
            }
            _requestedFeature = featureIndex;
            _navigation.Select(topic, featureIndex);
            _dirty = true;
        }

        private void ShowPage()
        {
            if (_pages.Count == 0) { _body.text = ToolkitLocalization.Get("ui.HelpWindow.ShowPage"); return; }
            var page = _pages[_pageIndex];
            var title = _features[page.FeatureIndex].Title;
            _featureTitle.text = page.PartCount > 1 ? $"{title} {page.PartIndex}" : title;
            _body.text = page.Text;
            _body.selection.cursorIndex = 0;
            _body.selection.selectIndex = 0;
            _pageInfo.text = $"{_pageIndex + 1} / {_pages.Count}";
            _previous.SetEnabled(_pageIndex > 0);
            _next.SetEnabled(_pageIndex + 1 < _pages.Count);
            _navigation.Select(_topic, page.FeatureIndex);
            // 翻页后原按钮可能被禁用, 焦点回到弹窗而不是泄漏到底层.
            if (!(_root.panel?.focusController.focusedElement is VisualElement focus) || !focus.enabledInHierarchy) _dialog.Focus();
        }

        private void Previous() { if (IsOpen && _pageIndex > 0) { _pageIndex--; ShowPage(); } }
        private void Next() { if (IsOpen && _pageIndex + 1 < _pages.Count) { _pageIndex++; ShowPage(); } }
        private void CloseFromButton() => Close(true);
        private void Geometry(GeometryChangedEvent evt) { if (evt.oldRect.size != evt.newRect.size) _dirty = true; }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private void Outside(PointerDownEvent evt)
        {
            if (!IsOpen || evt.target != _backdrop || evt.button != 0) return;
            Close(true);
            evt.StopImmediatePropagation();
        }

        private void KeyDown(KeyDownEvent evt)
        {
            if (!IsOpen) return;
            if (evt.keyCode == KeyCode.Escape) { Close(true); ConsumeKey(evt); return; }
            if (evt.keyCode == KeyCode.Tab)
            {
                var targets = _dialog.Query<VisualElement>().ToList().FindAll(IsFocusTarget);
                var current = targets.IndexOf(_root.panel?.focusController.focusedElement as VisualElement);
                if (current < 0) current = evt.shiftKey ? 0 : -1;
                if (targets.Count > 0)
                {
                    current = (current + (evt.shiftKey ? -1 : 1) + targets.Count) % targets.Count;
                    targets[current].Focus();
                }
                ConsumeKey(evt);
                return;
            }
            if (evt.keyCode != KeyCode.LeftArrow && evt.keyCode != KeyCode.RightArrow) return;
            if (evt.target is VisualElement target && _navigation.Contains(target)) return;
            var selecting = _root.panel?.focusController.focusedElement == _body && (_body.selection.cursorIndex != _body.selection.selectIndex || evt.shiftKey || evt.ctrlKey || evt.commandKey);
            if (selecting) return;
            if (evt.keyCode == KeyCode.LeftArrow) Previous(); else Next();
            ConsumeKey(evt);
        }

        // 使用基线公开事件接口同时阻止传播和原生焦点导航.
        private void ConsumeKey(KeyDownEvent evt) { evt.PreventDefault(); evt.StopImmediatePropagation(); }

        // 收起的树节点不参与焦点循环, 保留导航控件自身的键盘行为.
        private bool IsFocusTarget(VisualElement element)
        {
            if (element == _dialog || !element.focusable || !element.enabledInHierarchy) return false;
            for (var parent = element; parent != null && parent != _dialog; parent = parent.parent)
                if (parent.resolvedStyle.display == DisplayStyle.None || parent.resolvedStyle.visibility == Visibility.Hidden) return false;
            return true;
        }

        // 底层的延迟聚焦也不能抢占帮助窗口的键盘输入.
        private void FocusIn(FocusInEvent evt)
        {
            if (!IsOpen || evt.target is VisualElement element && _host.Contains(element)) return;
            _root.schedule.Execute(() => { if (IsOpen && !_disposed) _close.Focus(); });
        }

        private sealed class FocusSnapshot
        {
            private readonly VisualElement _focus; // 原生实际焦点节点.
            private readonly TextField _field; // 焦点所属输入框, 用于恢复选区和滚动.
            private readonly int _cursor; // 打开前光标位置.
            private readonly int _selection; // 打开前选区末端.
            private readonly Vector2 _scroll; // 原生输入框滚动位置.

            /// <summary> 点击入口前保存焦点, 不读取或修改正文. </summary>
            internal FocusSnapshot(VisualElement root)
            {
                _focus = root.panel?.focusController.focusedElement as VisualElement;
                for (var element = _focus; element != null; element = element.parent)
                    if (element is TextField field) { _field = field; break; }
                if (_field == null) return;
                _cursor = _field.cursorIndex;
                _selection = _field.selectIndex;
                _scroll = _field.Q<ScrollView>()?.scrollOffset ?? Vector2.zero;
            }

            /// <summary> 恢复原光标和选区, 避免原生首次聚焦全选改变编辑状态. </summary>
            internal void Restore()
            {
                if (_focus?.panel == null || !_focus.enabledInHierarchy || _focus.resolvedStyle.display == DisplayStyle.None) return;
                if (_field == null) { _focus.Focus(); return; }
                _field.Focus();
                _field.SelectRange(_cursor, _selection);
                var scroll = _field.Q<ScrollView>();
                if (scroll != null) scroll.scrollOffset = _scroll;
            }
        }
    }
}
