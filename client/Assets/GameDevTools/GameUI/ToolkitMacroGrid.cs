using System;
using System.Collections.Generic;
using System.Linq;
using GameConsole.Extension;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 在 Cheat 的固定宏入口下以预设式网格展示宏, 支持独立搜索与置顶拖拽. </summary>
    internal sealed class ToolkitMacroGrid : IDisposable
    {
        private readonly VisualElement _root; // 宏网格的有界容器.
        private readonly VisualElement _grid; // 自动换行布局.
        private readonly ScrollView _scroll; // 分页可用区域.
        private readonly TextField _search; // 宏名称, 描述和正文搜索.
        private readonly ToolkitSearchField _searchField; // 统一的手动历史入口.
        private readonly ToolkitSearchIndex<Macro> _searchIndex = new(macro => new[] { macro.name, macro.desc, macro.macro }); // 只在宏库变化时提取字段.
        private readonly SliderInt _size; // 沿用预设的按钮尺寸.
        private readonly Label _pageInfo; // 当前页和总项数.
        private readonly Label _status; // 运行状态.
        private readonly Button _previous; // 上一页.
        private readonly Button _next; // 下一页.
        private readonly Button _stop; // 停止后续执行.
        private readonly ToolkitCommandDetails _details; // 外侧共享详情.
        private readonly ToolkitPinnedDrag _drag; // 仅置顶网格项可拖动.
        private readonly ToolkitContextMenu _contextMenu; // Cheat 页共享的菜单服务.
        private readonly VisualTreeAsset _template; // 可在 UI Builder 调整的宏按钮.
        private readonly List<Item> _pool = new(); // 当前页按钮池.
        private readonly List<Macro> _matches = new(); // 排序并筛选后的宏快照.
        private readonly ToolkitTextDisplay _text = new(); // 与预设按钮共用名称省略, 完整宏数据不参与截断.
        private MacroSystem _system; // 总控的宏服务.
        private int _page; // 零起始页号.
        private int _capacity = 1; // 按可用宽高计算的分页容量.
        private int _active; // 已绑定的按钮数量.
        private bool _dirty = true; // 合并搜索, 配置和库变更.
        private bool _resultsDirty = true; // 翻页和尺寸变化复用筛选结果.
        private string _pinsSnapshot; // 无关配置或搜索历史保存不重排宏网格.
        private readonly Dictionary<string, int> _pinRanks = new(StringComparer.Ordinal); // 置顶变化时生成位序, 搜索和池项绑定只查询.
        private bool _visible; // 隐藏时暂停构造并取消拖拽.
        private bool _resetScroll = true; // 仅搜索和翻页主动回到顶部.
        private bool _restoreScroll; // 重绑期间保留原始偏移, 不读取中途收缩后的滚动值.
        private float _scrollAnchor; // 重排前的纵向滚动位置.
        private int _boundFrame = -1; // 全部按钮绑定完毕后的首帧, 下一帧使用完成布局的范围.

        /// <summary> 连接 UXML 中独立的宏网格区域. </summary>
        internal ToolkitMacroGrid(VisualElement root, ToolkitCommandDetails details, ToolkitContextMenu contextMenu)
        {
            _root = ToolkitView.Require<VisualElement>(root, "macroGridBox");
            _grid = ToolkitView.Require<VisualElement>(root, "macroGrid");
            _scroll = ToolkitView.Require<ScrollView>(root, "macroGridScroll");
            _search = ToolkitView.Require<TextField>(root, "macroGridSearch");
            _searchField = new ToolkitSearchField(_search, ToolkitView.Require<Button>(root, "macroGridSearchHistory"));
            _size = ToolkitView.Require<SliderInt>(root, "macroButtonSize");
            _size.lowValue = Config.MIN_PRESET_BUTTON_SIZE;
            _size.highValue = Config.MAX_PRESET_BUTTON_SIZE;
            _size.SetValueWithoutNotify(Config.DEFAULT_MACRO_BUTTON_SIZE);
            _pageInfo = ToolkitView.Require<Label>(root, "macroPageInfo");
            _status = ToolkitView.Require<Label>(root, "macroGridStatus");
            _previous = ToolkitView.Require<Button>(root, "previousMacros");
            _next = ToolkitView.Require<Button>(root, "nextMacros");
            _stop = ToolkitView.Require<Button>(root, "stopMacroGrid");
            _details = details;
            _contextMenu = contextMenu;
            _template = ToolkitFactory.RequireTemplate("ToolkitMacroButton");
            _drag = new ToolkitPinnedDrag(_scroll, true);
            _search.RegisterValueChangedCallback(OnSearch);
            _size.RegisterValueChangedCallback(OnSize);
            _scroll.contentViewport.RegisterCallback<GeometryChangedEvent>(OnGeometry);
            _previous.clicked += Previous;
            _next.clicked += Next;
            _stop.clicked += Stop;
            var pins = Config.GetPins(true);
            _pinsSnapshot = string.Join("\n", pins);
            UpdatePinRanks(pins);
            Config.e_onChanged += OnConfigChanged;
        }

        /// <summary> 所有宏来自同一个服务, 不复制注册逻辑. </summary>
        internal void Bind(MacroSystem system)
        {
            _system = system;
            system.e_onChanged += OnLibraryChanged;
            OnLibraryChanged();
        }

        /// <summary> 切换命令时不丢失宏搜索及当前页. </summary>
        internal void SetVisible(bool visible)
        {
            _visible = visible;
            _root.EnableInClassList("is-hidden", !visible);
            _root.parent.EnableInClassList("is-hidden", !visible);
            if (!visible)
            {
                _searchField.Hide();
                _contextMenu.Close();
                _drag.Cancel();
            }
            else if (_restoreScroll) _boundFrame = -1;
        }

        /// <summary> 可见时分页并按统一批次逐帧创建宏按钮. </summary>
        internal void Tick()
        {
            if (!_visible || _system == null) return;
            var available = _scroll.contentViewport.contentRect.size;
            if (available.x <= 0 || available.y <= 0 || float.IsNaN(available.x) || float.IsNaN(available.y)) return;
            if (_dirty)
            {
                if (_resetScroll) _scrollAnchor = 0;
                else if (!_restoreScroll) _scrollAnchor = _scroll.scrollOffset.y;
                _restoreScroll = true;
                _boundFrame = -1;
                if (_resetScroll) _scroll.scrollOffset = Vector2.zero;
                _resetScroll = false;
                _drag.Cancel();
                ClearItems();
                if (_resultsDirty)
                {
                    _searchIndex.Search(_search.value, _matches);
                    var ordered = _matches.OrderBy(macro => _pinRanks.TryGetValue(macro.Id, out var rank) ? rank : int.MaxValue).ThenBy(macro => macro.name, StringComparer.OrdinalIgnoreCase).ToArray();
                    _matches.Clear();
                    _matches.AddRange(ordered);
                    _resultsDirty = false;
                }
                _capacity = ToolkitPresetPanel.CalculateCapacity(available, _size.value);
                var pages = Math.Max(1, (_matches.Count + _capacity - 1) / _capacity);
                _page = Math.Max(0, Math.Min(_page, pages - 1));
                _pageInfo.text = ToolkitLocalization.Format("ui.MacroGrid.Tick", ("p0", (object)(_page + 1)), ("p1", (object)(pages)), ("p2", (object)(_matches.Count)));
                _previous.SetEnabled(_page > 0);
                _next.SetEnabled(_page + 1 < pages);
                _dirty = false;
            }
            var cell = ToolkitPresetPanel.CalculateCellSize(available.x, _size.value);
            var target = Math.Max(0, Math.Min(_capacity, _matches.Count - _page * _capacity));
            for (var count = 0; count < Config.PRESET_ITEMS_PER_FRAME && _active < target; count++)
            {
                if (_active == _pool.Count) CreateItem();
                var item = _pool[_active];
                item.macro = _matches[_page * _capacity + _active];
                _text.SetText(item.name, item.macro.name);
                var pinned = _pinRanks.ContainsKey(item.macro.Id);
                item.handle.EnableInClassList("is-hidden", !pinned);
                item.pin.EnableInClassList("is-hidden", !pinned);
                item.root.style.width = cell.x;
                item.root.style.height = cell.y;
                item.root.RemoveFromClassList("is-hidden");
                _active++;
            }
            if (_restoreScroll && _active == target)
            {
                if (_boundFrame < 0) _boundFrame = Time.frameCount;
                else if (_boundFrame != Time.frameCount) RestoreScroll();
            }
            _status.text = _system.Status;
            _stop.SetEnabled(_system.RunningId != null);
            foreach (var item in _pool)
                if (item.macro != null) item.button.SetEnabled(_system.RunningId == null);
            _text.Tick();
        }

        /// <summary> 保留网格及分页位置, 只刷新当前计数和运行状态. </summary>
        internal void RefreshLanguage()
        {
            var pages = Math.Max(1, (_matches.Count + _capacity - 1) / Math.Max(1, _capacity));
            _pageInfo.text = ToolkitLocalization.Format("ui.MacroGrid.Tick", ("p0", (object)(_page + 1)), ("p1", (object)pages), ("p2", (object)_matches.Count));
            if (_system != null) _status.text = _system.Status;
        }

        /// <summary> 解除服务订阅, 行事件和拖动捕获. </summary>
        public void Dispose()
        {
            ClearItems();
            _drag.Dispose();
            foreach (var item in _pool)
            {
                _contextMenu.Detach(item.root);
                item.unbind();
            }
            _grid.Clear();
            _text.Dispose();
            if (_system != null) _system.e_onChanged -= OnLibraryChanged;
            Config.e_onChanged -= OnConfigChanged;
            _search.UnregisterValueChangedCallback(OnSearch);
            _searchField.Dispose();
            _size.UnregisterValueChangedCallback(OnSize);
            _scroll.contentViewport.UnregisterCallback<GeometryChangedEvent>(OnGeometry);
            _previous.clicked -= Previous;
            _next.clicked -= Next;
            _stop.clicked -= Stop;
        }

        // 控件只注册一次, 执行与悬停读取当前池项数据.
        private void CreateItem()
        {
            var root = ToolkitFactory.InstantiateTemplate(_template);
            root.AddToClassList("macro-grid-item");
            var item = new Item { root = root, button = ToolkitView.Require<Button>(root, "macroButton"), name = ToolkitView.Require<Label>(root, "macroButtonName"), handle = ToolkitView.Require<Label>(root, "macroDragHandle"), pin = ToolkitView.Require<Label>(root, "macroPinned") };
            _text.Track(item.name, 6, truncatedClass: "macro-name-truncated");
            Action click = () => { if (item.macro != null) _system.Run(item.macro); };
            EventCallback<PointerEnterEvent> enter = evt => _details.Hover(item.macro);
            EventCallback<PointerLeaveEvent> leave = evt => _details.Restore(item.macro);
            item.button.clicked += click;
            root.RegisterCallback(enter);
            root.RegisterCallback(leave);
            _drag.Attach(root, item.handle, () => item.macro?.Id, () => item.macro?.name);
            _contextMenu.AttachMacro(root, () => item.macro, _drag.Cancel);
            item.unbind = () => { item.button.clicked -= click; root.UnregisterCallback(enter); root.UnregisterCallback(leave); };
            _pool.Add(item);
            _grid.Add(root);
        }

        // 清空当前引用后隐藏, 悬停详情不能保留旧的池项.
        private void ClearItems()
        {
            foreach (var item in _pool)
            {
                _drag.Recycle(item.root);
                _contextMenu.Recycle(item.root);
                _details.Restore(item.macro);
                item.macro = null;
                item.root.AddToClassList("is-hidden");
            }
            _active = 0;
        }

        private void Changed() { _dirty = true; _drag.Cancel(); if (_visible) _contextMenu.Close(); } // 库和置顶列表变化合并到下一帧.
        private void OnSearch(ChangeEvent<string> evt) { _page = 0; _resetScroll = true; _resultsDirty = true; Changed(); } // 搜索从第一页开始.
        private void OnSize(ChangeEvent<int> evt) => Changed(); // 尺寸变化重新计算行列.
        private void OnGeometry(GeometryChangedEvent evt) { if (evt.oldRect.size != evt.newRect.size) Changed(); } // 位置变化不重建网格.
        private void Previous() { _page--; _resetScroll = true; Changed(); } // 翻页保留搜索并回到顶部.
        private void Next() { _page++; _resetScroll = true; Changed(); } // 页号在 Tick 中夹取.
        private void Stop() => _system?.Stop(); // 与编辑页共用取消入口.

        // 保存, 删除或注册宏后刷新完整字段, 改名和正文修改立即参与搜索.
        private void OnLibraryChanged()
        {
            _searchIndex.Rebuild(_system?.Macros);
            _resultsDirty = true;
            Changed();
        }

        // 只对宏置顶变化重排, 避免记录历史时关闭悬停或打断拖拽.
        private void OnConfigChanged()
        {
            var pins = Config.GetPins(true);
            var snapshot = string.Join("\n", pins);
            if (snapshot == _pinsSnapshot) return;
            _pinsSnapshot = snapshot;
            UpdatePinRanks(pins);
            _resultsDirty = true;
            Changed();
        }

        // 首次出现的位序与 List.IndexOf 一致, 不改变配置中的标识或排列.
        private void UpdatePinRanks(IReadOnlyList<string> pins)
        {
            _pinRanks.Clear();
            for (var i = 0; i < pins.Count; i++)
                if (!_pinRanks.ContainsKey(pins[i])) _pinRanks.Add(pins[i], i);
        }

        // 所有批次完成且经过一次布局后恢复位置, 不使用隐藏池项时的临时范围.
        private void RestoreScroll()
        {
            var maximum = _scroll.verticalScroller.highValue;
            if (float.IsNaN(maximum) || float.IsInfinity(maximum)) return;
            var anchor = float.IsNaN(_scrollAnchor) || float.IsInfinity(_scrollAnchor) ? 0 : _scrollAnchor;
            _scroll.scrollOffset = new Vector2(0, Mathf.Clamp(anchor, 0, Mathf.Max(0, maximum)));
            _restoreScroll = false;
            _boundFrame = -1;
        }

        private sealed class Item
        {
            /// <summary> 池项外框. </summary>
            internal TemplateContainer root;
            /// <summary> 点击执行. </summary>
            internal Button button;
            /// <summary> 宏名称. </summary>
            internal Label name;
            /// <summary> 拖动柄. </summary>
            internal Label handle;
            /// <summary> 置顶标记. </summary>
            internal Label pin;
            /// <summary> 当前绑定宏. </summary>
            internal Macro macro;
            /// <summary> 回调注销. </summary>
            internal Action unbind;
        }
    }
}
