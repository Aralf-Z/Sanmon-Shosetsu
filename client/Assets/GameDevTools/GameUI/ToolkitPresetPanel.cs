using System;
using System.Collections.Generic;
using GameConsole.Extension;
using RedSaw.CommandLineInterface;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 按可视区域自适应分页, 分帧复用预设按钮并保持原始参数引用. </summary>
    internal sealed class ToolkitPresetPanel : IDisposable
    {
        private const float PAGER_INLINE_WIDTH = 660f; // 中间导航宽 360, 右侧跳转宽 150, 两侧对称预留空间.

        private readonly VisualElement _root; // 有预设时显示的右侧区域.
        private readonly VisualElement _grid; // 与实际单元格尺寸一致的换行容器.
        private readonly ScrollView _scroll; // 极矮窗口可在局部滚动, 正常每页完整显示.
        private readonly TextField _search; // 独立预设搜索.
        private readonly ToolkitSearchField _searchField; // 与命令和宏共用历史入口.
        private readonly ToolkitSearchIndex<ParamDefine> _searchIndex = new(define => new[] { define.name, define.param, define.tag }); // 描述只用于详情, 不参与预设搜索.
        private readonly SliderInt _size; // 零为纯文字模式.
        private readonly Label _empty; // 空结果提示.
        private readonly Label _pageInfo; // 显示页码与当前每页容量.
        private readonly Button _previous; // 前一页.
        private readonly Button _next; // 后一页.
        private readonly VisualElement _pager; // 宽窗口同排, 窄窗口把跳转移到下一行右侧.
        private readonly IntegerField _jumpPage; // 用户可编辑的目标页码, 从一开始.
        private readonly Button _jump; // 只切换页面, 不执行预设命令.
        private readonly VisualTreeAsset _template; // 可编辑的图标按钮模板.
        private readonly ToolkitCommandDetails _details; // 共用详情与图标缓存.
        private readonly Action<string> _submit; // 原始控制器提交入口.
        private readonly ToolkitContextMenu _contextMenu; // 与命令和宏共用的右键菜单.
        private readonly ToolkitTextDisplay _text = new(); // 缓存完整名称, 展示时追加英文省略号.
        private readonly List<ParamDefine> _matches = new(); // 当前筛选结果.
        private readonly List<Item> _pool = new(); // 模板仅在容量不足时创建, 隐藏项可复用.
        private readonly Dictionary<Command, int> _anchors = new(); // 记住各命令的首项索引, 不保存过期页码.
        private CommandWrapper _command; // 当前预设所属命令.
        private ParamDefine _hoveredPreset; // 网格间隙保留最后悬停项, 离开整个视口才恢复命令详情.
        private int _capacity = 1; // 有效可视尺寸确定前不使用零尺寸覆盖容量.
        private int _start; // 当前页首项索引.
        private int _anchor; // 重排后必须仍在当前页内的旧首项.
        private int _activeCount; // 当前页已经绑定的按钮数量.
        private bool _layoutDirty = true; // 合并宽高与图标尺寸变化.
        private bool _pageDirty = true; // 搜索或命令切换后重新绑定.
        private Vector2 _cell; // 包含按钮边距的完整单元格尺寸.

        /// <summary> 连接工具栏, 页码和可视区域尺寸监听. </summary>
        internal ToolkitPresetPanel(VisualElement root, ToolkitCommandDetails details, Action<string> submit, ToolkitContextMenu contextMenu)
        {
            _root = ToolkitView.Require<VisualElement>(root, "presetsBox");
            _grid = ToolkitView.Require<VisualElement>(root, "presetGrid");
            _scroll = ToolkitView.Require<ScrollView>(root, "presetScroll");
            _search = ToolkitView.Require<TextField>(root, "presetSearch");
            _searchField = new ToolkitSearchField(_search, ToolkitView.Require<Button>(root, "presetSearchHistory"));
            _size = ToolkitView.Require<SliderInt>(root, "presetButtonSize");
            _size.lowValue = Config.MIN_PRESET_BUTTON_SIZE;
            _size.highValue = Config.MAX_PRESET_BUTTON_SIZE;
            _size.SetValueWithoutNotify(Config.DEFAULT_PRESET_BUTTON_SIZE);
            _empty = ToolkitView.Require<Label>(root, "presetEmpty");
            _pageInfo = ToolkitView.Require<Label>(root, "presetPageInfo");
            _previous = ToolkitView.Require<Button>(root, "previousPresets");
            _next = ToolkitView.Require<Button>(root, "nextPresets");
            _pager = ToolkitView.Require<VisualElement>(root, "presetPager");
            _jumpPage = ToolkitView.Require<IntegerField>(root, "presetJumpPage");
            _jump = ToolkitView.Require<Button>(root, "jumpPresets");
            _template = ToolkitFactory.RequireTemplate("ToolkitPresetButton");
            _details = details;
            _submit = submit;
            _contextMenu = contextMenu;
            _text.Track(_pageInfo);
            _search.RegisterValueChangedCallback(OnSearch);
            _size.RegisterValueChangedCallback(OnSize);
            _scroll.contentViewport.RegisterCallback<GeometryChangedEvent>(OnViewportGeometry);
            _scroll.contentViewport.RegisterCallback<PointerLeaveEvent>(OnViewportLeave);
            _previous.clicked += PreviousPage;
            _next.clicked += NextPage;
            _jump.clicked += JumpPage;
            _jumpPage.RegisterValueChangedCallback(OnJumpPageChanged);
            _pager.RegisterCallback<GeometryChangedEvent>(OnPagerGeometry);
        }

        /// <summary> 切换命令时清空预设搜索, 恢复该命令的首项位置. </summary>
        internal void SetCommand(CommandWrapper command)
        {
            _searchField.Hide();
            _details.Restore(_hoveredPreset);
            _hoveredPreset = null;
            _command = command;
            _search.SetValueWithoutNotify(string.Empty);
            _searchIndex.Rebuild(command?.ParameterDefines);
            _searchIndex.Search(string.Empty, _matches);
            _root.EnableInClassList("is-hidden", command?.ParameterDefines == null || command.ParameterDefines.Count == 0);
            RequestPage(command != null && _anchors.TryGetValue(command.Command, out var anchor) ? anchor : 0);
        }

        /// <summary> 按最终可用宽高合并分页更新, 新增和图标解析均保留批次预算. </summary>
        internal void Tick()
        {
            var available = _scroll.contentViewport.contentRect.size;
            if (!ValidSize(available) || _command == null || _command.ParameterDefines?.Count == 0)
                return;
            if (_layoutDirty || _pageDirty)
            {
                _cell = CalculateCellSize(available.x, _size.value);
                var columns = Math.Max(1, Mathf.FloorToInt((available.x + 0.001f) / _cell.x));
                _cell.x = available.x / columns;
                var capacity = CalculateCapacity(available, _size.value);
                var total = Math.Max(1, (_matches.Count + (long)capacity - 1) / capacity);
                var page = (int)Math.Min(Math.Max(0, _anchor) / capacity, total - 1);
                var start = page * capacity;
                if (_pageDirty || _start != start)
                    DeactivateFrom(0);
                _capacity = capacity;
                _start = start;
                var count = Math.Min(_capacity, _matches.Count - _start);
                DeactivateFrom(Math.Max(0, count));
                foreach (var item in _pool)
                    if (item.define != null)
                        ApplySize(item);
                _pageInfo.text = ToolkitLocalization.Format("ui.PresetPanel.Tick", ("p0", (object)(page + 1)), ("p1", (object)(total)), ("p2", (object)(_capacity)), ("p3", (object)(_matches.Count)));
                _pageInfo.tooltip = _pageInfo.text;
                _previous.SetEnabled(total > 1);
                _next.SetEnabled(total > 1);
                _jumpPage.SetValueWithoutNotify(page + 1 < total ? page + 2 : 1);
                _jumpPage.SetEnabled(total > 1);
                _jump.SetEnabled(total > 1);
                _empty.EnableInClassList("is-hidden", _matches.Count > 0);
                if (_command != null && string.IsNullOrWhiteSpace(_search.value))
                    _anchors[_command.Command] = Math.Min(_anchor, Math.Max(0, _matches.Count - 1));
                _scroll.scrollOffset = Vector2.zero;
                _layoutDirty = false;
                _pageDirty = false;
            }
            var target = Math.Min(_capacity, _matches.Count - _start);
            for (var added = 0; added < Config.PRESET_ITEMS_PER_FRAME && _activeCount < target; added++)
            {
                if (_activeCount == _pool.Count)
                    CreateItem();
                Bind(_pool[_activeCount], _matches[_start + _activeCount]);
                _activeCount++;
            }
            var iconBudget = Config.PRESET_ITEMS_PER_FRAME;
            for (var index = 0; index < _activeCount; index++)
            {
                var item = _pool[index];
                if (_size.value > 0 && !item.iconResolved && iconBudget > 0)
                {
                    item.icon.sprite = _details.ResolveIcon(item.define);
                    item.iconResolved = true;
                    iconBudget--;
                }
                item.button.EnableInClassList("text-only", _size.value == 0 || item.icon.sprite == null);
                item.icon.EnableInClassList("is-hidden", _size.value == 0 || item.icon.sprite == null);
            }
            _text.Tick();
        }

        /// <summary> 以完整行列计算容量, 小于一个单元格时仍保留一项. </summary>
        internal static int CalculateCapacity(Vector2 available, int size)
        {
            if (!ValidSize(available))
                return 1;
            var cell = CalculateCellSize(available.x, size);
            var columns = Math.Max(1, Mathf.FloorToInt((available.x + 0.001f) / cell.x));
            var rows = Math.Max(1, Mathf.FloorToInt((available.y + 0.001f) / cell.y));
            return (int)Math.Min(int.MaxValue, (long)columns * rows);
        }

        /// <summary> 页面隐藏时关闭独立的历史菜单并提交完整搜索词. </summary>
        internal void HideSearch() => _searchField.Hide();

        /// <summary> 只更新分页文案, 不翻页或重置用户滚动与跳转输入. </summary>
        internal void RefreshLanguage()
        {
            var page = _start / Math.Max(1, _capacity);
            var total = Math.Max(1, (_matches.Count + _capacity - 1) / Math.Max(1, _capacity));
            _text.SetText(_pageInfo, ToolkitLocalization.Format("ui.PresetPanel.Tick", ("p0", (object)(page + 1)), ("p1", (object)total), ("p2", (object)_capacity), ("p3", (object)_matches.Count)));
            _pageInfo.tooltip = ToolkitLocalization.Format("ui.PresetPanel.Tick", ("p0", (object)(page + 1)), ("p1", (object)total), ("p2", (object)_capacity), ("p3", (object)_matches.Count));
        }

        /// <summary> 解除池中控件回调及文本测量监听. </summary>
        public void Dispose()
        {
            DeactivateFrom(0);
            foreach (var item in _pool)
            {
                _contextMenu.Detach(item.root);
                item.unbind();
            }
            _pool.Clear();
            _grid.Clear();
            _text.Dispose();
            _search.UnregisterValueChangedCallback(OnSearch);
            _searchField.Dispose();
            _size.UnregisterValueChangedCallback(OnSize);
            _scroll.contentViewport.UnregisterCallback<GeometryChangedEvent>(OnViewportGeometry);
            _scroll.contentViewport.UnregisterCallback<PointerLeaveEvent>(OnViewportLeave);
            _previous.clicked -= PreviousPage;
            _next.clicked -= NextPage;
            _jump.clicked -= JumpPage;
            _jumpPage.UnregisterValueChangedCallback(OnJumpPageChanged);
            _pager.UnregisterCallback<GeometryChangedEvent>(OnPagerGeometry);
            _matches.Clear();
            _anchors.Clear();
            _command = null;
        }

        /// <summary> 预设和宏共用完整单元格尺寸, 包含外框留白, 与分页公式保持一致. </summary>
        internal static Vector2 CalculateCellSize(float width, int size)
        {
            size = Mathf.Clamp(size, Config.MIN_PRESET_BUTTON_SIZE, Config.MAX_PRESET_BUTTON_SIZE);
            return new Vector2(Mathf.Min(96f + size * .64f, width), (size == 0 ? 32f : 56f + size * .72f) + 6f);
        }

        // 隐藏或尚未布局时不以零宽高计算新页.
        private static bool ValidSize(Vector2 value) => value.x > 0 && value.y > 0 && !float.IsNaN(value.x) && !float.IsNaN(value.y) && !float.IsInfinity(value.x) && !float.IsInfinity(value.y);

        // 事件仅注册一次, 执行时读取此按钮当前绑定的数据.
        private void CreateItem()
        {
            var root = ToolkitFactory.InstantiateTemplate(_template);
            root.pickingMode = PickingMode.Position;
            var item = new Item { root = root, button = ToolkitView.Require<Button>(root, "presetButton"), icon = ToolkitView.Require<Image>(root, "presetIcon"), name = ToolkitView.Require<Label>(root, "presetName") };
            _text.Track(item.name, 6);
            Action click = () => { if (item.define != null) _submit(item.define.param); };
            EventCallback<PointerEnterEvent> enter = evt => { _hoveredPreset = item.define; _details.Hover(item.define); };
            item.button.clicked += click;
            root.RegisterCallback(enter);
            _contextMenu.AttachPreset(root, () => _command, () => item.define);
            item.unbind = () => { item.button.clicked -= click; root.UnregisterCallback(enter); };
            _pool.Add(item);
            _grid.Add(root);
        }

        // 先恢复旧悬停再替换参数, 不在重排后保留过期回调引用.
        private void Bind(Item item, ParamDefine define)
        {
            if (!ReferenceEquals(item.define, define))
            {
                _contextMenu.Recycle(item.root);
                _details.Restore(item.define);
                item.define = define;
                item.icon.sprite = null;
                item.iconResolved = false;
            }
            _text.SetText(item.name, ToolkitCommandDetails.DisplayName(define));
            item.root.RemoveFromClassList("is-hidden");
            item.button.SetEnabled(true);
            ApplySize(item);
        }

        // 更新几何不重新创建按钮或加载图标.
        private void ApplySize(Item item)
        {
            item.root.style.width = _cell.x;
            item.root.style.height = _cell.y;
            var iconSize = 24f + Mathf.Clamp(_size.value, Config.MIN_PRESET_BUTTON_SIZE, Config.MAX_PRESET_BUTTON_SIZE) * .48f;
            item.icon.style.width = Mathf.Min(iconSize, Mathf.Max(0, _cell.x - 16f));
            item.icon.style.height = iconSize;
        }

        // 隐藏的池项不可点击, 后续页面可复用同一个模板.
        private void DeactivateFrom(int index)
        {
            for (var i = index; i < _activeCount; i++)
            {
                var item = _pool[i];
                _contextMenu.Recycle(item.root);
                _details.Restore(item.define);
                item.define = null;
                item.root.AddToClassList("is-hidden");
                item.button.SetEnabled(false);
            }
            _activeCount = Math.Min(index, _activeCount);
        }

        // 搜索变化重回第一页, 保留按钮大小.
        private void OnSearch(ChangeEvent<string> evt)
        {
            _searchIndex.Search(evt.newValue, _matches);
            RequestPage(0);
        }

        // 大小变化保留原页首项在新页面中, 同帧多次变化只处理一次.
        private void OnSize(ChangeEvent<int> evt) => _layoutDirty = true;

        // 实际可视宽高变化后重算行列, 不受内容总高度影响.
        private void OnViewportGeometry(GeometryChangedEvent evt)
        {
            if (evt.oldRect.size != evt.newRect.size) _layoutDirty = true;
        }

        // 完整单元格包含按钮留白, 网格间移动不会反复恢复默认详情.
        private void OnViewportLeave(PointerLeaveEvent evt)
        {
            _details.Restore(_hoveredPreset);
            _hoveredPreset = null;
        }

        // 命令和分页操作立即禁用旧数据, 新数据下一帧分批绑定.
        private void RequestPage(int anchor)
        {
            DeactivateFrom(0);
            _anchor = anchor;
            _pageDirty = true;
        }

        // 第一页向前循环到末页, 末页首项按当前容量对齐.
        private void PreviousPage()
        {
            if (_matches.Count <= _capacity) return;
            RequestPage(_start <= 0 ? (_matches.Count - 1) / _capacity * _capacity : _start - _capacity);
        }

        // 末页向后循环回第一页, 避免超出当前匹配结果.
        private void NextPage()
        {
            if (_matches.Count <= _capacity) return;
            RequestPage(_matches.Count - _start > _capacity ? _start + _capacity : 0);
        }

        // 显示的页码从一开始, 越界目标约束到有效范围内.
        private void JumpPage()
        {
            var total = (int)Math.Max(1, (_matches.Count + (long)_capacity - 1) / _capacity);
            var page = Mathf.Clamp(_jumpPage.value, 1, total);
            _jumpPage.SetValueWithoutNotify(page);
            RequestPage((page - 1) * _capacity);
        }

        private void OnJumpPageChanged(ChangeEvent<int> evt)
        {
            var total = (int)Math.Max(1, (_matches.Count + (long)_capacity - 1) / _capacity);
            _jumpPage.SetValueWithoutNotify(Mathf.Clamp(evt.newValue, 1, total));
        }

        // 只根据宽度切换排列, 不因分页栏高度变化反复切换布局.
        private void OnPagerGeometry(GeometryChangedEvent evt)
        {
            if (evt.oldRect.width != evt.newRect.width)
                _pager.EnableInClassList("preset-pager-stacked", evt.newRect.width < PAGER_INLINE_WIDTH);
        }

        // 池项保存当前引用, 不捕获旧的预设字符串.
        private sealed class Item
        {
            /// <summary> 模板根元素. </summary>
            internal TemplateContainer root;
            /// <summary> 执行按钮. </summary>
            internal Button button;
            /// <summary> 可隐藏图标. </summary>
            internal Image icon;
            /// <summary> 单独保留原文的短名称. </summary>
            internal Label name;
            /// <summary> 当前绑定预设, 隐藏后置空. </summary>
            internal ParamDefine define;
            /// <summary> 避免重排时重复加载. </summary>
            internal bool iconResolved;
            /// <summary> 销毁时注销回调. </summary>
            internal Action unbind;
        }
    }
}
