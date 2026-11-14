using System;
using System.Collections.Generic;
using RedSaw.CommandLineInterface;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 用 UI Toolkit 实现现有命令控制台的显示接口. </summary>
    public sealed class ToolkitConsoleRenderer : IConsoleRenderer, IStructuredConsoleRenderer, IDisposable
    {
        /// <summary> 最新十条带颜色标记的单行日志预览, 不依赖 Console 页签可见状态. </summary>
        public IReadOnlyList<string> LatestLines => _buffer.Store.LatestLines;

        /// <summary> 输出或清空后递增, 供浮标合并刷新. </summary>
        public int LogVersion => _buffer.Store.Version;

        /// <summary> 使用结构化日志通路. </summary>
        public bool AcceptsStructuredLogs => true;

        /// <summary> 整个工具是否显示. </summary>
        public bool WindowVisible
        {
            get => _windowVisible;
            set { var wasVisible = IsVisible; _windowVisible = value; UpdateVisibility(wasVisible); }
        }

        /// <summary> 当前是否选中 Console 页. </summary>
        public bool ConsoleSelected
        {
            get => _consoleSelected;
            set { var wasVisible = IsVisible; _consoleSelected = value; UpdateVisibility(wasVisible); }
        }

        /// <summary> 控制器只处理可见的 Console 页. </summary>
        public bool IsVisible { get => WindowVisible && ConsoleSelected; set => ConsoleSelected = value; }

        /// <summary> 包括 TextField 内部文本编辑节点的焦点. </summary>
        public bool IsInputFieldFocus => IsVisible && !InputSuppressed && _input.panel?.focusController.focusedElement is VisualElement focus && (focus == _input || _input.Contains(focus));
        /// <summary> 模态帮助期间暂停输入聚焦, 日志渲染继续. </summary>
        internal bool InputSuppressed { get; set; }
        /// <summary> 详情文本中的复制操作应保留原生行为. </summary>
        public bool IsLogDetailFocused => IsVisible && _details.IsTextFocused;

        /// <summary> 通过普通 ChangeEvent 保留控制器的补全和历史处理. </summary>
        public string InputText
        {
            get => _input.value ?? string.Empty;
            set
            {
                value ??= string.Empty;
                if (_input.value == value)
                    _textChanged?.Invoke(value);
                else
                    _input.value = value;
            }
        }

        /// <summary> 光标左侧文本, 补全时保留右侧文本. </summary>
        public string InputTextToCursor
        {
            get => InputText[..Mathf.Clamp(_requestedCursor >= 0 ? _requestedCursor : _input.cursorIndex, 0, InputText.Length)];
            set
            {
                var cursor = InputTextToCursor.Length;
                InputText = value + InputText[cursor..];
            }
        }

        /// <summary> 候选列表是否参与本次提交. </summary>
        public bool IsAlternativeOptionsActive
        {
            get => _suggestionsVisible;
            set
            {
                _suggestionsVisible = value;
                _suggestions.EnableInClassList("is-hidden", !value);
            }
        }

        /// <summary> 替换控制器产生的补全说明. </summary>
        public List<string> AlternativeOptions
        {
            set
            {
                _alternatives.Clear();
                if (value != null)
                    _alternatives.AddRange(value);
                _suggestions.Rebuild();
            }
        }

        /// <summary> 控制器维护候选选择, UI 只显示高亮. </summary>
        public int AlternativeOptionsIndex
        {
            set
            {
                _alternativeIndex = value;
                _suggestions.RefreshItems();
                if (value >= 0 && value < _alternatives.Count)
                    _suggestions.ScrollToItem(value);
            }
        }

        private readonly TextField _input; // 单行命令输入框.
        private readonly ScrollView _output; // 直接布局日志行, 首屏不经过动态高度虚拟化的延迟填充.
        private readonly ListView _suggestions; // 补全候选列表.
        private readonly Label _title; // 清空以来的原始日志接收总数.
        private readonly Toggle _collapse; // 与 Options 同步的持久化折叠开关.
        private readonly Toggle _icons; // 仅控制日志类型图标的展示.
        private readonly Toggle _timestamps; // 接收时间由显示层生成, 不改变缓存正文.
        private readonly Button _newLogs; // 阅读历史时的新日志提示.
        private readonly Button[] _filters = new Button[4]; // Unity 日志, 警告, 错误与 Dev 分别开关, 支持任意组合.
        private readonly List<Action> _unbind = new(); // 工具栏事件的显式注销.
        private readonly ToolkitConsoleDetails _details; // 右边界箭头和固定宽度的堆栈侧栏.
        private readonly ToolkitTextDisplay _text = new(); // 仅省略补全与固定提示, 完整日志保持自动换行.
        private readonly ToolkitConsoleLogBuffer _buffer = new(); // 完整缓存与已加载范围独立, 重复日志仍按编号区分.
        private readonly ToolkitConsoleViewport _viewport; // 最新日志贴底, 向上准备旧行, 手动滚动时停止跟随.
        private readonly List<string> _alternatives = new(); // 由原控制器生成的候选.
        private Action<string> _textChanged; // 控制器注册的输入变更回调.
        private Action<string> _submit; // 控制器注册的提交回调.
        private int _alternativeIndex; // 当前高亮候选.
        private int _requestedCursor = -1; // TextCore 布局前暂存控制器请求的逻辑光标.
        private bool _suggestionsVisible; // 独立记录候选显示状态.
        private int _visibleFrame; // 输入聚焦延迟至进入后的下一帧.
        private int _lastLoadFrame = -1; // 一帧合并一次逻辑范围变更.
        private bool _windowVisible; // 整个工具窗口是否可见.
        private bool _consoleSelected; // 当前是否选择 Console.
        private bool _focusRequested; // 延后聚焦, 避免切页同时初始化文本布局.
        private bool _disposed; // 释放后拒绝所有回调.
        private ConsoleLogFilter _filter = ConsoleLogFilter.All; // 默认四种类型全部选中.
        private int _seenVersion = -1; // 数据增量更新标识.
        private int _generation; // 清空会使所有历史编号失效.
        private bool _configuredCollapse; // 折叠方式变化才重建显示范围.
        private long _selectedId; // 不依赖回收节点的选中状态.
        private int _reportedTotal = -1; // 纯滚动不重复拼接标题文本.
        private int _reportedPending = -1; // 已展示的新日志提示数量.

        /// <summary> 查找控件并配置日志和候选列表. </summary>
        public ToolkitConsoleRenderer(VisualElement root)
        {
            _input = ToolkitView.Require<TextField>(root, "commandInput");
            _output = ToolkitView.Require<ScrollView>(root, "outputList");
            _suggestions = ToolkitView.Require<ListView>(root, "suggestionList");
            _title = ToolkitView.Require<Label>(root, "consoleTitle");
            _collapse = ToolkitView.Require<Toggle>(root, "consoleCollapseToggle");
            _icons = ToolkitView.Require<Toggle>(root, "consoleIconsToggle");
            _timestamps = ToolkitView.Require<Toggle>(root, "consoleTimestampsToggle");
            _newLogs = ToolkitView.Require<Button>(root, "consoleNewLogs");
            _text.Track(_title);
            _text.Track(ToolkitView.Require<Label>(root, "consoleInputHint"));
            var itemTemplate = ToolkitFactory.RequireTemplate("ToolkitListItem");
            _viewport = new ToolkitConsoleViewport(_output, _buffer, itemTemplate, SelectLog);
            _details = new ToolkitConsoleDetails(root);
            var names = new[] { "Log", "Warning", "Error", "Dev" };
            for (var index = 0; index < names.Length; index++)
            {
                var filter = (ConsoleLogFilter)(1 << index);
                var button = _filters[index] = ToolkitView.Require<Button>(root, "consoleFilter" + names[index]);
                Action click = () => { _filter ^= filter; ResetRange(); };
                button.clicked += click;
                _unbind.Add(() => button.clicked -= click);
            }
            _configuredCollapse = Config.ConsoleCollapseLogs;
            _collapse.SetValueWithoutNotify(_configuredCollapse);
            _collapse.RegisterValueChangedCallback(OnCollapse);
            _icons.RegisterValueChangedCallback(OnIcons);
            _timestamps.RegisterValueChangedCallback(OnTimestamps);
            SyncPresentation();
            _newLogs.clicked += ResetRange;
            Config.e_onChanged += OnSettingsChanged;
            _suggestions.itemsSource = _alternatives;
            _suggestions.makeItem = () =>
            {
                var item = ToolkitFactory.InstantiateTemplate(itemTemplate);
                _text.Track(ToolkitView.Require<Label>(item, "itemText"));
                return item;
            };
            _suggestions.destroyItem = element => _text.Untrack(ToolkitView.Require<Label>(element, "itemText"));
            _suggestions.bindItem = (element, index) =>
            {
                _text.SetText(ToolkitView.Require<Label>(element, "itemText"), _alternatives[index]);
                element.EnableInClassList("suggestion-selected", index == _alternativeIndex);
            };
            _input.RegisterValueChangedCallback(OnTextChanged);
            _input.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            _input.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            _input.RegisterCallback<NavigationMoveEvent>(OnNavigationMove, TrickleDown.TrickleDown);
            _input.RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit, TrickleDown.TrickleDown);
            _input.RegisterCallback<NavigationCancelEvent>(OnNavigationCancel, TrickleDown.TrickleDown);
            IsAlternativeOptionsActive = false;
        }

        /// <summary> 每帧最多刷新一次输出列表. </summary>
        public void Tick()
        {
            if (_disposed || !IsVisible)
                return;
            if (!InputSuppressed && _focusRequested && Time.frameCount > _visibleFrame)
            {
                _focusRequested = false;
                _input.Focus();
                SetInputCursorPosition(InputText.Length);
            }
            if (_requestedCursor >= 0)
            {
                _input.SelectRange(_requestedCursor, _requestedCursor);
                if (_input.cursorIndex == _requestedCursor)
                    _requestedCursor = -1;
            }
            LoadOutputBatch();
            _details.Tick();
            _text.Tick();
        }

        /// <summary> 仅 Console 可见时允许命令提交重新聚焦. </summary>
        public void ActivateInput()
        {
            if (!IsVisible || _disposed || InputSuppressed)
                return;
            _focusRequested = true;
        }

        /// <summary> 聚焦单行命令输入. </summary>
        public void Focus() => ActivateInput();

        /// <summary> 释放整个面板的焦点, 包括 Cheat 参数输入框. </summary>
        public void QuitFocus()
        {
            _requestedCursor = -1;
            _focusRequested = false;
            _input.panel?.focusController.focusedElement?.Blur();
        }

        /// <summary> 同步光标和选择末端, 不保留历史文本选区. </summary>
        public void SetInputCursorPosition(int pos)
        {
            _requestedCursor = Mathf.Clamp(pos, 0, InputText.Length);
            _input.SelectRange(_requestedCursor, _requestedCursor);
        }

        /// <summary> 下次可见刷新后滚动, 避免隐藏页的布局访问. </summary>
        public void MoveScrollBarToEnd() { if (IsVisible) ResetRange(); }

        /// <summary> 绑定初始化前就已接收的同一份缓存, 不回放历史正文. </summary>
        public void BindLogs(ConsoleLogStore store)
        {
            _buffer.Bind(store);
            ResetRange();
        }

        /// <summary> 绑定现有控制器的提交入口. </summary>
        public void BindOnSubmit(Action<string> callback) => _submit += callback;

        /// <summary> 绑定现有控制器的补全入口. </summary>
        public void BindOnTextChanged(Action<string> callback) => _textChanged += callback;

        /// <summary> 清除完整日志缓存, 计数, 连续分组和选中项. </summary>
        public void Clear()
        {
            _buffer.Store.Clear();
            _selectedId = 0;
            ResetRange();
        }

        /// <summary> 追加一条日志, 每帧批量刷新显示. </summary>
        public void Output(string msg)
        {
            if (_disposed)
                return;
            OutputLog(new ConsoleLogRecord(msg, msg, null));
        }

        /// <summary> 按条追加多行输出. </summary>
        public void Output(string[] msgs)
        {
            foreach (var msg in msgs)
                Output(msg);
        }

        /// <summary> 保留控制器传入的富文本颜色. </summary>
        public void Output(string msg, string color = "#ffffff") => OutputLog(new ConsoleLogRecord(msg, msg, color));

        /// <summary> 写入完整缓存, 可见时在 Tick 中合并刷新, 不强制打断历史阅读. </summary>
        public void OutputLog(ConsoleLogRecord record) { if (!_disposed) _buffer.Store.Append(record); }

        /// <summary> 按条应用多行输出的颜色. </summary>
        public void Output(string[] msgs, string color = "#ffffff")
        {
            foreach (var msg in msgs)
                Output(msg, color);
        }

        /// <summary> 不清空历史范围或筛选, 仅刷新标题, 图标和选中详情. </summary>
        internal void RefreshLanguage()
        {
            _reportedTotal = -1;
            _reportedPending = -1;
            UpdateLoadStatus();
            _details.RefreshLanguage();
        }

        /// <summary> 释放所有输入事件和控制器引用. </summary>
        public void Dispose()
        {
            _disposed = true;
            _viewport.Dispose();
            _details.Dispose();
            _collapse.UnregisterValueChangedCallback(OnCollapse);
            _icons.UnregisterValueChangedCallback(OnIcons);
            _timestamps.UnregisterValueChangedCallback(OnTimestamps);
            _newLogs.clicked -= ResetRange;
            Config.e_onChanged -= OnSettingsChanged;
            foreach (var unbind in _unbind) unbind();
            _unbind.Clear();
            QuitFocus();
            _input.UnregisterValueChangedCallback(OnTextChanged);
            _input.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            _input.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            _input.UnregisterCallback<NavigationMoveEvent>(OnNavigationMove, TrickleDown.TrickleDown);
            _input.UnregisterCallback<NavigationSubmitEvent>(OnNavigationSubmit, TrickleDown.TrickleDown);
            _input.UnregisterCallback<NavigationCancelEvent>(OnNavigationCancel, TrickleDown.TrickleDown);
            _textChanged = null;
            _submit = null;
            _suggestions.itemsSource = null;
            _text.Dispose();
            _alternatives.Clear();
        }

        // 每次重新显示最近范围, 复用相同宽度下已有的节点和行高.
        private void UpdateVisibility(bool wasVisible)
        {
            if (_disposed || wasVisible == IsVisible)
                return;
            _focusRequested = false;
            _viewport.SetVisible(IsVisible);
            _details.SetVisible(IsVisible);
            if (!IsVisible)
                return;
            _visibleFrame = Time.frameCount;
            ResetRange();
        }

        // 先提交逻辑范围, 再由视口按预算一次准备并定位可见行.
        private void LoadOutputBatch()
        {
            OnSettingsChanged();
            if (_generation != _buffer.Store.Generation) { _selectedId = 0; ResetRange(); }
            if (_lastLoadFrame != Time.frameCount)
            {
                _lastLoadFrame = Time.frameCount;
                var anchor = _viewport.CaptureAnchor();
                var first = _buffer.First;
                var end = _buffer.End;
                var changed = _seenVersion != _buffer.Store.Version;
                if (!_viewport.IsUserScrolling)
                {
                    if (changed || _viewport.Following && (_buffer.Pending > 0 || _buffer.End != _buffer.Store.GetCount(_buffer.Filter, _buffer.Collapsed))) _buffer.Observe(_viewport.Following);
                    if (_viewport.TakeHistoryRequest()) _buffer.LoadOlder();
                }
                var rangeChanged = first != _buffer.First || end != _buffer.End;
                if (changed || rangeChanged)
                {
                    _viewport.Refresh(rangeChanged, anchor);
                    UpdateSelection();
                }
                _seenVersion = _buffer.Store.Version;
                UpdateLoadStatus();
            }
            _viewport.Tick();
        }

        // 一次确定最近 100 条逻辑范围, 保留现有的渐进准备和按需历史加载.
        private void ResetRange()
        {
            _configuredCollapse = Config.ConsoleCollapseLogs;
            _collapse.SetValueWithoutNotify(_configuredCollapse);
            _buffer.Reset(_filter, _configuredCollapse);
            _generation = _buffer.Store.Generation;
            _seenVersion = _buffer.Store.Version;
            _lastLoadFrame = Time.frameCount;
            for (var index = 0; index < _filters.Length; index++) _filters[index].EnableInClassList("selected", ((int)_filter & (1 << index)) != 0);
            UpdateSelection();
            if (IsVisible) _viewport.Reset();
            UpdateLoadStatus();
        }

        // 标题只显示接收总数, 新日志提示仍保留历史阅读时的累计数量.
        private void UpdateLoadStatus()
        {
            var total = _buffer.Store.Count;
            var pending = _buffer.Pending;
            if (_reportedTotal != total)
                _text.SetText(_title, ToolkitLocalization.Format("console.received", ("count", (object)(total))));
            if (_reportedPending != pending)
            {
                _newLogs.EnableInClassList("is-hidden", pending == 0);
                _newLogs.text = ToolkitLocalization.Format("console.newLogs", ("count", (object)(pending)));
            }
            _reportedTotal = total;
            _reportedPending = pending;
        }

        private void OnSettingsChanged()
        {
            SyncPresentation();
            if (_configuredCollapse == Config.ConsoleCollapseLogs) return;
            ResetRange();
        }

        private void OnCollapse(ChangeEvent<bool> evt) { Config.ConsoleCollapseLogs = evt.newValue; Config.Save(); } // 工具栏与 Options 共用唯一配置.
        private void OnIcons(ChangeEvent<bool> evt) { Config.ConsoleShowLogIcons = evt.newValue; Config.Save(); } // 不改变筛选或历史范围.
        private void OnTimestamps(ChangeEvent<bool> evt) { Config.ConsoleShowTimestamps = evt.newValue; Config.Save(); } // 对历史和新增日志统一生效.

        // 仅更新显示条件, 保留阅读位置, 选中项和当前筛选范围.
        private void SyncPresentation()
        {
            var icons = Config.ConsoleShowLogIcons;
            var timestamps = Config.ConsoleShowTimestamps;
            if (_icons.value != icons) _icons.SetValueWithoutNotify(icons);
            if (_timestamps.value != timestamps) _timestamps.SetValueWithoutNotify(timestamps);
            _viewport.SetPresentation(icons, timestamps);
        }

        private void SelectLog(long id)
        {
            _selectedId = id;
            UpdateSelection();
            _viewport.Refresh(false, _viewport.CaptureAnchor());
        }

        private void UpdateSelection()
        {
            var index = _selectedId == 0 ? -1 : _buffer.Store.Find(_filter, _configuredCollapse, _selectedId);
            if (index < 0) _selectedId = 0;
            _viewport.SelectedId = _selectedId;
            _details.Show(index < 0 ? null : _buffer.Store.GetItem(_filter, _configuredCollapse, index));
        }

        // 只在 Console 页允许用户输入触发补全, 控制器设置文本时仍需通知.
        private void OnTextChanged(ChangeEvent<string> evt)
        {
            _requestedCursor = -1;
            _textChanged?.Invoke(evt.newValue);
        }

        // 用户重新定位光标后取消尚未完成的程序化定位.
        private void OnPointerDown(PointerDownEvent evt)
        {
            _requestedCursor = -1;
            _focusRequested = false;
        }

        // 在 TextField 内部默认处理之前截获控制台快捷键.
        private void OnKeyDown(KeyDownEvent evt)
        {
            if (InputSuppressed) { evt.StopImmediatePropagation(); return; }
            if (!IsVisible)
                return;
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                evt.StopImmediatePropagation();
                if (string.IsNullOrEmpty(UnityEngine.Input.compositionString))
                    _submit?.Invoke(InputText);
            }
            else if (evt.keyCode == KeyCode.UpArrow || evt.keyCode == KeyCode.DownArrow || evt.keyCode == KeyCode.Escape || ToolShortcut.Matches(evt.keyCode, evt.ctrlKey, evt.altKey, evt.shiftKey, evt.commandKey))
                evt.StopImmediatePropagation();
            else if (evt.keyCode == KeyCode.LeftArrow || evt.keyCode == KeyCode.RightArrow || evt.keyCode == KeyCode.Home || evt.keyCode == KeyCode.End)
                _requestedCursor = -1;
        }

        // 阻止同一上下键额外触发 TextField 导航, 历史由控制器统一处理.
        private void OnNavigationMove(NavigationMoveEvent evt) => evt.StopImmediatePropagation();

        // 回车提交由 KeyDownEvent 处理, 不重复消费导航提交事件.
        private void OnNavigationSubmit(NavigationSubmitEvent evt) => evt.StopImmediatePropagation();

        // Escape 的焦点释放由控制器统一处理.
        private void OnNavigationCancel(NavigationCancelEvent evt) => evt.StopImmediatePropagation();
    }
}
