using System;
using System.Collections.Generic;
using System.Linq;
using GameConsole.Extension;
using RedSaw.CommandLineInterface;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 复用命令缓存和分帧加载, 管理 P2 双栏中的命令选择与参数表单. </summary>
    public sealed class ToolkitCheatPanel : IDisposable
    {

        /// <summary> 帮助入口按当前右侧内容选择说明, 不修改选中命令. </summary>
        internal ToolkitHelpTopic HelpTopic => _showingMacros ? ToolkitHelpTopic.CheatMacros : _selected?.ParameterDefines?.Count > 0 ? ToolkitHelpTopic.CheatPresets : ToolkitHelpTopic.CheatParameters;

        private readonly ToolkitConsoleRenderer _renderer; // 沿用原命令输出与补全适配器.
        private readonly VisualTreeAsset _categoryTemplate; // 可在 UI Builder 编辑的分类按钮.
        private readonly VisualTreeAsset _parameterTemplate; // 可编辑的普通参数模板.
        private readonly VisualTreeAsset _commandTemplate; // 可编辑的命令按钮模板.
        private readonly VisualElement _tags; // 顶部横向滚动的命令分类.
        private readonly TextField _search; // 全局命令搜索, 不影响预设搜索.
        private readonly ToolkitSearchField _searchField; // 共用搜索历史交互.
        private readonly ToolkitSearchIndex<CommandWrapper> _searchIndex = new(wrapper => new[] { wrapper.Command.name, wrapper.Alias, wrapper.Command.tag, wrapper.Command.description }); // 仅索引命令允许的四个字段.
        private readonly ListView _commandsView; // 虚拟化的命令按钮列表.
        private readonly Label _count; // 加载进度和筛选数量.
        private readonly Label _description; // 当前命令标题, 完整说明位于外侧详情.
        private readonly Label _status; // 加载和提交状态.
        private string _lastSubmittedCommand; // 仅用于重新格式化最后一次提交提示, 不执行命令.
        private readonly VisualElement _parameters; // 普通参数字段容器.
        private readonly VisualElement _form; // 无预设时使用的表单与执行按钮.
        private readonly Button _execute; // 普通或零参数命令的执行入口.
        private readonly ToolkitCommandDetails _details; // 外侧共享的悬停详情.
        private readonly ToolkitPresetPanel _presets; // 分页按钮网格及独立搜索.
        private readonly ToolkitContextMenu _contextMenu; // 命令, 宏和预设共用的运行时右键菜单.
        private readonly ToolkitMacroGrid _macroGrid; // 固定宏入口的预设式按钮网格.
        private readonly ToolkitPinnedDrag _pinnedDrag; // 左侧用户置顶命令拖拽.
        private readonly Button _macroEntry; // 永久第一且不可取消置顶的入口.
        private MacroSystem _macroSystem; // 与编辑页共享命令包装缓存.
        private bool _showingMacros = true; // 默认选中固定宏入口.
        private string _pinsSnapshot; // 无关配置更新不重建命令列表.
        private readonly Dictionary<string, int> _pinRanks = new(StringComparer.Ordinal); // 保留命令名的大小写规则, 避免排序时重复扫描置顶列表.
        private readonly ToolkitCheatSplitter _splitter; // 实例内保留的双栏比例.
        private readonly ToolkitTextDisplay _text = new(); // 统一命令与状态的展示省略, 保留原始字符串.
        private readonly List<CommandWrapper> _commands = new(); // 仅首次收集的有效命令.
        private readonly List<CommandWrapper> _filtered = new(); // 分类和命令搜索后的缓存.
        private readonly List<CommandWrapper> _displayCommands = new(); // 按批次显示的结果.
        private readonly SortedSet<string> _commandTags = new(); // 命令收集后有序显示的分类.
        private readonly List<TextField> _fields = new(); // 顺序与 ParameterInfos 一致.
        private readonly List<Action> _unbind = new(); // 分类事件的显式注销操作.
        private readonly Dictionary<VisualElement, Action> _rowUnbind = new(); // 虚拟化按钮只注册一次事件.
        private ConsoleController<LogType> _console; // 总控创建的唯一命令控制器.
        private CommandWrapper _selected; // 点击后选中, 悬停不会改变此值.
        private Command[] _commandSource; // 有序的首次命令快照.
        private string[] _categorySource; // 待创建的分类快照.
        private string _tag; // 空表示全部分类.
        private int _commandIndex; // 共享命令快照中下一条待归类的命令, 不重新构造包装.
        private int _categoryIndex; // 下一批待显示的分类.
        private int _displayIndex; // 下一批待显示的命令.
        private int _failed; // 单条命令初始化失败数量.
        private int _parameterIndex; // 下一批待构建的字段.
        private int _visibleFrame; // 切页当帧不构造内容.
        private int _lastLoadFrame = -1; // 每帧仅推进一次.
        private bool _loaded; // 命令与分类是否全部构造完成.
        private bool _visible; // 隐藏页面暂停内容构造.
        private bool _restoreSelectionPending; // 列表恢复后默认选择首项.

        /// <summary> 连接 UXML 布局, 参数定义仍在用户打开页面后分帧收集. </summary>
        public ToolkitCheatPanel(VisualElement root, ToolkitConsoleRenderer renderer)
        {
            _renderer = renderer;
            _categoryTemplate = ToolkitFactory.RequireTemplate("ToolkitCommandCategory");
            _parameterTemplate = ToolkitFactory.RequireTemplate("ToolkitCommandParameter");
            _commandTemplate = ToolkitFactory.RequireTemplate("ToolkitCommandButton");
            _tags = ToolkitView.Require<VisualElement>(root, "commandTags");
            _search = ToolkitView.Require<TextField>(root, "commandSearch");
            _searchField = new ToolkitSearchField(_search, ToolkitView.Require<Button>(root, "commandSearchHistory"));
            _commandsView = ToolkitView.Require<ListView>(root, "commandList");
            _count = ToolkitView.Require<Label>(root, "commandCount");
            _description = ToolkitView.Require<Label>(root, "commandDescription");
            _status = ToolkitView.Require<Label>(root, "cheatStatus");
            _parameters = ToolkitView.Require<VisualElement>(root, "parameterFields");
            _form = ToolkitView.Require<VisualElement>(root, "parameterForm");
            _execute = ToolkitView.Require<Button>(root, "executeCommand");
            _details = new ToolkitCommandDetails(root);
            _contextMenu = new ToolkitContextMenu(root);
            _presets = new ToolkitPresetPanel(root, _details, Submit, _contextMenu);
            _macroGrid = new ToolkitMacroGrid(root, _details, _contextMenu);
            _pinnedDrag = new ToolkitPinnedDrag(_commandsView, false);
            _macroEntry = ToolkitView.Require<Button>(root, "showMacros");
            _macroEntry.clicked += ShowMacros;
            Config.e_onChanged += OnPinsChanged;
            _splitter = new ToolkitCheatSplitter(root);
            _text.Track(_description);
            _text.Track(_count);
            _text.Track(_status, maxHeight: 42f);
            _commandsView.selectionType = SelectionType.None;
            var commandScroll = _commandsView.Q<ScrollView>();
            if (commandScroll != null)
                commandScroll.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
            _commandsView.itemsSource = _displayCommands;
            _commandsView.makeItem = MakeCommandItem;
            _commandsView.bindItem = BindCommandItem;
            _commandsView.unbindItem = UnbindCommandItem;
            _commandsView.destroyItem = DestroyCommandItem;
            _search.RegisterValueChangedCallback(OnSearch);
            _execute.clicked += Execute;
            _execute.SetEnabled(false);
            ShowMacros();
        }

        /// <summary> 使用原控制器, 不引入第二套命令系统. </summary>
        public void BindConsole(ConsoleController<LogType> console, MacroSystem macros, Func<string, Sprite> iconLoader = null)
        {
            _console = console;
            _macroSystem = macros ?? throw new ArgumentNullException(nameof(macros));
            _details.SetIconLoader(iconLoader);
            _macroGrid.Bind(_macroSystem);
            var pins = Config.GetPins(false);
            _pinsSnapshot = string.Join("\n", pins);
            UpdatePinRanks(pins);
        }

        /// <summary> 共享缓存就绪后先准备最终显示顺序, 按钮在 Tick 中分批追加. </summary>
        public void EnsureCommands()
        {
            if (_loaded || _commandSource != null || _console == null)
                return;
            _macroSystem.RequestCommands();
            if (!_macroSystem.IsReady) return;
            _commandSource = _macroSystem.Commands.Select(wrapper => wrapper.Command).ToArray();
            AddTag(ToolkitLocalization.Get("ui.CheatPanel.EnsureCommands"), null);
            while (_commandIndex < _commandSource.Length) LoadCommand();
            _searchIndex.Rebuild(_commands);
            Filter(true);
        }

        /// <summary> 保留编辑内容, 隐藏时取消分隔线捕获及悬停详情. </summary>
        public void SetVisible(bool visible)
        {
            if (_visible == visible)
                return;
            _visible = visible;
            _details.SetVisible(visible);
            _macroGrid.SetVisible(visible && _showingMacros);
            if (!visible)
            {
                _searchField.Hide();
                _presets.HideSearch();
                _contextMenu.Close();
                _pinnedDrag.Cancel();
                _splitter.Cancel();
                return;
            }
            _visibleFrame = Time.frameCount;
            _displayIndex = 0;
            _restoreSelectionPending = true;
            _displayCommands.Clear();
            _commandsView.RefreshItems();
        }

        /// <summary> 可见时按批次构造命令, 表单和预设网格, 合并交互布局更新. </summary>
        public void Tick()
        {
            if (!_visible || Time.frameCount <= _visibleFrame || _lastLoadFrame == Time.frameCount)
                return;
            _lastLoadFrame = Time.frameCount;
            _splitter.Tick();
            _macroGrid.Tick();
            EnsureCommands();
            if (_commandSource == null)
                return;
            var loadedBefore = _loaded;
            if (!_loaded)
                LoadCategories();
            var previousCount = _displayCommands.Count;
            for (var added = 0; added < Config.CHEAT_COMMANDS_PER_FRAME && _displayIndex < _filtered.Count; added++)
                _displayCommands.Add(_filtered[_displayIndex++]);
            if (_displayCommands.Count != previousCount)
                _commandsView.RefreshItems();
            if (loadedBefore != _loaded || _displayCommands.Count != previousCount)
                _count.text = _displayIndex < _filtered.Count ? ToolkitLocalization.Format("ui.CheatPanel.Tick", ("p0", (object)(_displayIndex)), ("p1", (object)(_filtered.Count))) : ToolkitLocalization.Format("ui.CheatPanel.Tick.2", ("p0", (object)(_filtered.Count)));
            if (_restoreSelectionPending && _loaded && _displayIndex == _filtered.Count)
            {
                _restoreSelectionPending = false;
                if (!_showingMacros && _selected == null && _displayCommands.Count > 0)
                    ShowCommand(_displayCommands[0]);
            }
            LoadParameters();
            _presets.Tick();
            _details.Tick();
            _text.Tick();
        }

        /// <summary> 只更新固定显示和子面板, 不重筛选或重建参数输入. </summary>
        internal void RefreshLanguage()
        {
            if (_tags.childCount > 0)
            {
                var all = _tags[0].Q<Button>();
                if (all != null) all.text = ToolkitLocalization.Get("ui.CheatPanel.EnsureCommands");
            }
            _description.text = _showingMacros ? ToolkitLocalization.Get("ui.CheatPanel.ShowMacros") : _selected?.Alias ?? ToolkitLocalization.Get("ui.CheatPanel.ShowCommand");
            _count.text = ToolkitLocalization.Format("ui.CheatPanel.Filter", ("p0", (object)_filtered.Count));
            _details.RefreshLanguage();
            _presets.RefreshLanguage();
            _macroGrid.RefreshLanguage();
            _contextMenu.Close();
            _status.text = _lastSubmittedCommand != null ? ToolkitLocalization.Format("ui.CheatPanel.Submit", ("p0", (object)_lastSubmittedCommand)) : _failed == 0 ? ToolkitLocalization.Get("ui.CheatPanel.LoadCategories") : ToolkitLocalization.Format("ui.CheatPanel.LoadCategories.2", ("p0", (object)_failed));
        }

        /// <summary> 解除静态和虚拟化控件事件, 清除运行时缓存引用. </summary>
        public void Dispose()
        {
            SetVisible(false);
            _search.UnregisterValueChangedCallback(OnSearch);
            _searchField.Dispose();
            _execute.clicked -= Execute;
            _splitter.Dispose();
            _presets.Dispose();
            _macroGrid.Dispose();
            _pinnedDrag.Dispose();
            _contextMenu.Dispose();
            _macroEntry.clicked -= ShowMacros;
            Config.e_onChanged -= OnPinsChanged;
            _commandsView.itemsSource = null;
            foreach (var unbind in _rowUnbind.Values)
                unbind();
            _rowUnbind.Clear();
            foreach (var unbind in _unbind)
                unbind();
            _unbind.Clear();
            _details.Clear();
            _text.Dispose();
            _console = null;
            _commandSource = null;
            _categorySource = null;
        }

        // 一条命令出错后继续收集其余命令.
        private void LoadCommand()
        {
            var wrapper = _macroSystem.Commands[_commandIndex++];
            var command = wrapper.Command;
            try
            {
                if (wrapper.IsCheatIgnore)
                    return;
                _commands.Add(wrapper);
                if (!string.IsNullOrEmpty(command.tag))
                    _commandTags.Add(command.tag);
            }
            catch (Exception exception)
            {
                _failed++;
                _renderer.Output(ToolkitLocalization.Format("ui.CheatPanel.LoadCommand", ("p0", (object)(command.name)), ("p1", (object)(exception.GetBaseException().Message))), "#f27a5f");
            }
        }

        // 分类每帧最多创建两个, 不清空已有用户输入.
        private void LoadCategories()
        {
            _categorySource ??= _commandTags.ToArray();
            for (var added = 0; added < 2 && _categoryIndex < _categorySource.Length; added++)
            {
                var tag = _categorySource[_categoryIndex++];
                AddTag(tag, tag);
            }
            _loaded = _categoryIndex == _categorySource.Length;
            if (_loaded)
            {
                _status.text = _failed == 0 ? ToolkitLocalization.Get("ui.CheatPanel.LoadCategories") : ToolkitLocalization.Format("ui.CheatPanel.LoadCategories.2", ("p0", (object)(_failed)));
            }
        }

        // 虚拟化行读取当前 userData, 避免闭包保存过期索引.
        private VisualElement MakeCommandItem()
        {
            var item = ToolkitFactory.InstantiateTemplate(_commandTemplate);
            var button = ToolkitView.Require<Button>(item, "commandButton");
            var row = ToolkitView.Require<VisualElement>(item, "commandRow");
            var handle = ToolkitView.Require<Label>(item, "commandDragHandle");
            _pinnedDrag.Attach(row, handle, () => (button.userData as CommandWrapper)?.Command.name, () => (button.userData as CommandWrapper)?.Alias);
            _contextMenu.AttachCommand(row, () => button.userData as CommandWrapper, _pinnedDrag.Cancel);
            _text.Track(button);
            Action click = () => ShowCommand(button.userData as CommandWrapper);
            EventCallback<PointerEnterEvent> enter = evt => _details.Hover(button.userData as CommandWrapper);
            EventCallback<PointerLeaveEvent> leave = evt => _details.Restore(button.userData);
            button.clicked += click;
            button.RegisterCallback(enter);
            button.RegisterCallback(leave);
            _rowUnbind[item] = () => { button.clicked -= click; button.UnregisterCallback(enter); button.UnregisterCallback(leave); };
            return item;
        }

        // 绑定只更新名称及选中外观, 不收集参数或加载图标.
        private void BindCommandItem(VisualElement item, int index)
        {
            _contextMenu.Recycle(ToolkitView.Require<VisualElement>(item, "commandRow"));
            var button = ToolkitView.Require<Button>(item, "commandButton");
            var command = _displayCommands[index];
            button.userData = command;
            _text.SetText(button, command.Alias);
            button.EnableInClassList("selected", ReferenceEquals(command, _selected));
            var pinned = _pinRanks.ContainsKey(command.Command.name);
            ToolkitView.Require<Label>(item, "commandDragHandle").EnableInClassList("is-hidden", !pinned);
            ToolkitView.Require<Label>(item, "commandPinned").EnableInClassList("is-hidden", !pinned);
        }

        // 回收悬停行时立即恢复选中命令详情.
        private void UnbindCommandItem(VisualElement item, int index)
        {
            _pinnedDrag.Recycle(ToolkitView.Require<VisualElement>(item, "commandRow"));
            _contextMenu.Recycle(ToolkitView.Require<VisualElement>(item, "commandRow"));
            var button = ToolkitView.Require<Button>(item, "commandButton");
            _details.Restore(button.userData);
            button.userData = null;
        }

        // 销毁虚拟化行时解除仅注册一次的回调.
        private void DestroyCommandItem(VisualElement item)
        {
            _pinnedDrag.Detach(ToolkitView.Require<VisualElement>(item, "commandRow"));
            _contextMenu.Detach(ToolkitView.Require<VisualElement>(item, "commandRow"));
            var button = ToolkitView.Require<Button>(item, "commandButton");
            _details.Restore(button.userData);
            _text.Untrack(button);
            if (_rowUnbind.TryGetValue(item, out var unbind))
                unbind();
            _rowUnbind.Remove(item);
        }

        // 命令搜索复用统一索引, 参数预设和宏仍使用独立数据源.
        private void OnSearch(ChangeEvent<string> evt) => Filter();

        // 分类模板在内容区顶端横向排列.
        private void AddTag(string title, string tag)
        {
            var item = ToolkitFactory.InstantiateTemplate(_categoryTemplate);
            var button = ToolkitView.Require<Button>(item, "categoryButton");
            button.text = title;
            button.EnableInClassList("selected", tag == _tag);
            Action click = () =>
            {
                _tag = tag;
                foreach (var child in _tags.Children())
                    ToolkitView.Require<Button>(child, "categoryButton").EnableInClassList("selected", child == item);
                Filter();
            };
            button.clicked += click;
            _unbind.Add(() => button.clicked -= click);
            _tags.Add(item);
        }

        // 搜索和分类共用缓存, 不重新调用参数收集器.
        private void Filter(bool preserveSelection = false)
        {
            _contextMenu.Close();
            _pinnedDrag.Cancel();
            _searchIndex.Search(_search.value, _filtered, IncludeCategory);
            var ordered = _filtered.OrderBy(wrapper => _pinRanks.TryGetValue(wrapper.Command.name, out var rank) ? rank : int.MaxValue).ThenBy(wrapper => wrapper.Command.name, StringComparer.OrdinalIgnoreCase).ToArray();
            _filtered.Clear();
            _filtered.AddRange(ordered);
            _displayCommands.Clear();
            _displayIndex = 0;
            _restoreSelectionPending = true;
            _commandsView.RefreshItems();
            _count.text = ToolkitLocalization.Format("ui.CheatPanel.Filter", ("p0", (object)(_filtered.Count)));
            if (!preserveSelection && !_showingMacros && (_selected == null || !_filtered.Contains(_selected))) ShowCommand(null);
        }

        // 分类只限制命令源, 标签文本仍可通过统一文字搜索匹配.
        private bool IncludeCategory(CommandWrapper wrapper) => _tag == null || wrapper.Command.tag == _tag;

        // 点击只选择命令, 不执行或重复清空同一条命令的编辑内容.
        private void ShowCommand(CommandWrapper wrapper)
        {
            if (!_showingMacros && wrapper != null && ReferenceEquals(_selected, wrapper))
                return;
            _contextMenu.Close();
            _showingMacros = false;
            _macroEntry.RemoveFromClassList("selected");
            _macroGrid.SetVisible(false);
            _selected = wrapper;
            foreach (var field in _fields)
                _text.Untrack(field.labelElement);
            _fields.Clear();
            _parameters.Clear();
            _parameterIndex = 0;
            _execute.SetEnabled(false);
            _details.Select(wrapper);
            _presets.SetCommand(wrapper);
            var hasPresets = wrapper?.ParameterDefines?.Count > 0;
            _form.EnableInClassList("is-hidden", wrapper == null || hasPresets);
            _description.text = wrapper?.Alias ?? ToolkitLocalization.Get("ui.CheatPanel.ShowCommand");
            _commandsView.RefreshItems();
        }

        // 固定入口不属于可排序命令数据, 搜索和分类不会将其移除.
        private void ShowMacros()
        {
            _contextMenu.Close();
            _showingMacros = true;
            _selected = null;
            _details.Select(null);
            _presets.SetCommand(null);
            _form.AddToClassList("is-hidden");
            _description.text = ToolkitLocalization.Get("ui.CheatPanel.ShowMacros");
            _macroEntry.AddToClassList("selected");
            _macroGrid.SetVisible(_visible);
            _commandsView.RefreshItems();
        }

        // 仅置顶列表实际变化时重排, 保留右侧正在编辑的参数和当前选择.
        private void OnPinsChanged()
        {
            var pins = Config.GetPins(false);
            var snapshot = string.Join("\n", pins);
            if (snapshot == _pinsSnapshot) return;
            _pinsSnapshot = snapshot;
            UpdatePinRanks(pins);
            Filter(true);
        }

        // 首次出现的位序与 List.IndexOf 一致, 更新不触碰存档或拖拽移动逻辑.
        private void UpdatePinRanks(IReadOnlyList<string> pins)
        {
            _pinRanks.Clear();
            for (var i = 0; i < pins.Count; i++)
                if (!_pinRanks.ContainsKey(pins[i])) _pinRanks.Add(pins[i], i);
        }

        // 预设与手动参数表单互斥, 零参数命令可以直接执行.
        private void LoadParameters()
        {
            if (_selected == null || _selected.ParameterDefines?.Count > 0)
                return;
            for (var added = 0; added < Config.CHEAT_PARAMETERS_PER_FRAME && _parameterIndex < _selected.ParameterInfos.Count; added++)
            {
                var info = _selected.ParameterInfos[_parameterIndex++];
                var item = ToolkitFactory.InstantiateTemplate(_parameterTemplate);
                var field = ToolkitView.Require<TextField>(item, "parameterInput");
                var parameterName = string.IsNullOrWhiteSpace(info.alias) ? info.name : info.alias;
                field.label = parameterName;
                _text.Track(field.labelElement);
                // 动态参数名解除模板的本地化绑定, 避免首次刷新或切换语言时被占位文本覆盖.
                _text.SetText(field.labelElement, parameterName);
                field.SetValueWithoutNotify(info.defaultValue ?? string.Empty);
                _fields.Add(field);
                _parameters.Add(item);
            }
            _execute.SetEnabled(_parameterIndex == _selected.ParameterInfos.Count);
        }

        // 参数字符串沿用控制台语法, 不拆分或重组预设中的引号.
        private void Submit(string parameters)
        {
            if (!_visible || _selected == null || _console == null)
                return;
            var command = _selected.BuildCommandInput(parameters);
            _renderer.IsAlternativeOptionsActive = false;
            _console.OnSubmit(command);
            _renderer.IsAlternativeOptionsActive = false;
            _lastSubmittedCommand = command;
            _status.text = ToolkitLocalization.Format("ui.CheatPanel.Submit", ("p0", (object)(command)));
        }

        // 手动表单只在字段创建完成后允许执行.
        private void Execute()
        {
            if (_execute.enabledSelf)
                Submit(string.Join(" ", _fields.Select(field => field.value)));
        }
    }
}
