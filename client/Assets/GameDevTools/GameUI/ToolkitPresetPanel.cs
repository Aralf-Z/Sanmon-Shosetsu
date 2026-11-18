using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using GameConsole.Extension;
using RedSaw.CommandLineInterface;

namespace GameConsole.UI
{
    /// <summary> 按可视区域自适应分页, 分帧复用预设按钮并保持原始参数引用. </summary>
    internal sealed class ToolkitPresetPanel : IDisposable
    {
        private readonly VisualElement _root;

        private readonly VisualElement _grid;

        private readonly ScrollView _scroll;

        private readonly TextField _search;

        private readonly ToolkitSearchField _searchField;

        private readonly ToolkitTagFilterBar _tagFilter;

        private readonly ToolkitSearchIndex<ParamDefine> _searchIndex = new(define => new[] { define.name, define.param, define.tag });

        private readonly SliderInt _size; // 零为纯文字模式.

        private readonly Label _empty;

        private readonly ToolkitPager _pager;

        private readonly VisualTreeAsset _template;

        private readonly ToolkitCommandDetails _details;

        private readonly Action<string> _submit;

        private readonly ToolkitContextMenu _contextMenu;

        private readonly ToolkitTextDisplay _text = new();

        private readonly List<ParamDefine> _matches = new();

        private readonly List<Item> _pool = new(); // 模板仅在容量不足时创建, 隐藏项可复用.

        private readonly Dictionary<Command, int> _anchors = new();

        private CommandWrapper _command;

        private ParamDefine _hoveredPreset; // 网格间隙保留最后悬停项, 离开整个视口才恢复命令详情.

        private int _capacity = 1; // 有效可视尺寸确定前不使用零尺寸覆盖容量.

        private int _start;

        private int _anchor; // 重排后必须仍在当前页内的旧首项.

        private int _activeCount;

        private bool _layoutDirty = true;

        private bool _pageDirty = true;

        private Vector2 _cell;

        internal ToolkitPresetPanel(VisualElement root, ToolkitCommandDetails details, Action<string> submit, ToolkitContextMenu contextMenu)
        {
            _root = ToolkitView.Require<VisualElement>(root, "presetsBox");
            _grid = ToolkitView.Require<VisualElement>(root, "presetGrid");
            _scroll = ToolkitView.Require<ScrollView>(root, "presetScroll");
            _search = ToolkitView.Require<TextField>(root, "presetSearch");
            _searchField = new ToolkitSearchField(_search, ToolkitView.Require<Button>(root, "presetSearchHistory"));
            _tagFilter = new ToolkitTagFilterBar(ToolkitView.Require<VisualElement>(root, "presetTags"), ToolkitView.Require<VisualElement>(root, "presetToolbar"), ToolkitView.Require<VisualElement>(root, "presetSearchGroup"));
            _tagFilter.e_onChanged += OnTag;
            _size = ToolkitView.Require<SliderInt>(root, "presetButtonSize");
            _size.lowValue = Config.MIN_PRESET_BUTTON_SIZE;
            _size.highValue = Config.MAX_PRESET_BUTTON_SIZE;
            _size.SetValueWithoutNotify(Config.DEFAULT_PRESET_BUTTON_SIZE);
            _empty = ToolkitView.Require<Label>(root, "presetEmpty");
            _pager = new ToolkitPager(ToolkitView.Require<VisualElement>(root, "presetPager"), ChangePage);
            _template = ToolkitFactory.RequireTemplate("ToolkitPresetButton");
            _details = details;
            _submit = submit;
            _contextMenu = contextMenu;
            _search.RegisterValueChangedCallback(OnSearch);
            _size.RegisterValueChangedCallback(OnSize);
            _scroll.contentViewport.RegisterCallback<GeometryChangedEvent>(OnViewportGeometry);
            _scroll.contentViewport.RegisterCallback<PointerLeaveEvent>(OnViewportLeave);
        }

        internal void SetCommand(CommandWrapper command)
        {
            _searchField.Hide();
            _details.Restore(_hoveredPreset);
            _hoveredPreset = null;
            _command = command;
            _search.SetValueWithoutNotify(string.Empty);
            _tagFilter.SetTags(command?.ParameterDefines?.Select(define => define?.tag));
            _searchIndex.Rebuild(command?.ParameterDefines);
            _searchIndex.Search(string.Empty, _matches, IncludeTag);
            _root.EnableInClassList("is-hidden", command?.ParameterDefines == null || command.ParameterDefines.Count == 0);
            RequestPage(command != null && _anchors.TryGetValue(command.Command, out var anchor) ? anchor : 0);
        }

        /// <summary> 按最终可用宽高合并分页更新, 新增和图标解析均保留批次预算. </summary>
        internal void Tick()
        {
            var available = _scroll.contentViewport.contentRect.size;

            if (!ValidSize(available) || _command == null || _command.ParameterDefines?.Count == 0) return;

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
                {
                    DeactivateFrom(0);
                }

                _capacity = capacity;
                _start = start;
                var count = Math.Min(_capacity, _matches.Count - _start);

                DeactivateFrom(Math.Max(0, count));
                foreach (var item in _pool)
                {
                    if (item.define != null)
                    {
                        ApplySize(item);
                    }
                }

                _pager.SetPage(page, (int)total, ToolkitLocalization.Format("ui.PresetPanel.Tick", ("p0", (object)(page + 1)), ("p1", (object)(total)), ("p2", (object)(_capacity)), ("p3", (object)(_matches.Count))));
                _empty.EnableInClassList("is-hidden", _matches.Count > 0);
                if (_command != null && _tagFilter.SelectedTag == null && string.IsNullOrWhiteSpace(_search.value))
                {
                    _anchors[_command.Command] = Math.Min(_anchor, Math.Max(0, _matches.Count - 1));
                }

                _scroll.scrollOffset = Vector2.zero;
                _layoutDirty = false;
                _pageDirty = false;
            }

            var target = Math.Min(_capacity, _matches.Count - _start);

            for (var added = 0; added < Config.PRESET_ITEMS_PER_FRAME && _activeCount < target; added++)
            {
                if (_activeCount == _pool.Count)
                {
                    CreateItem();
                }

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
            if (!ValidSize(available)) return 1;

            var cell = CalculateCellSize(available.x, size);
            var columns = Math.Max(1, Mathf.FloorToInt((available.x + 0.001f) / cell.x));
            var rows = Math.Max(1, Mathf.FloorToInt((available.y + 0.001f) / cell.y));

            return (int)Math.Min(int.MaxValue, (long)columns * rows);
        }

        internal void HideSearch()
        {
            _searchField.Hide();
            _tagFilter.Close();
        }

        internal void RefreshLanguage()
        {
            _tagFilter.RefreshLanguage();
            var page = _start / Math.Max(1, _capacity);
            var total = Math.Max(1, (_matches.Count + _capacity - 1) / Math.Max(1, _capacity));

            _pager.SetPage(page, total, ToolkitLocalization.Format("ui.PresetPanel.Tick", ("p0", (object)(page + 1)), ("p1", (object)total), ("p2", (object)_capacity), ("p3", (object)_matches.Count)));
        }

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
            _tagFilter.e_onChanged -= OnTag;
            _tagFilter.Dispose();
            _size.UnregisterValueChangedCallback(OnSize);
            _scroll.contentViewport.UnregisterCallback<GeometryChangedEvent>(OnViewportGeometry);
            _scroll.contentViewport.UnregisterCallback<PointerLeaveEvent>(OnViewportLeave);
            _pager.Dispose();
            _matches.Clear();
            _anchors.Clear();
            _command = null;
        }

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
            var item = new Item
            { root = root, button = ToolkitView.Require<Button>(root, "presetButton"), icon = ToolkitView.Require<Image>(root, "presetIcon"), name = ToolkitView.Require<Label>(root, "presetName") };

            _text.Track(item.name, 6);
            Action click = () =>
            {
                if (item.define != null)
                {
                    _submit(item.define.param);
                }
            };
            EventCallback<PointerEnterEvent> enter = evt =>
            {
                _hoveredPreset = item.define;
                _details.Hover(item.define);
            };

            item.button.clicked += click;
            root.RegisterCallback(enter);
            _contextMenu.AttachPreset(root, () => _command, () => item.define);
            item.unbind = () =>
            {
                item.button.clicked -= click;
                root.UnregisterCallback(enter);
            };
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

        private void ApplySize(Item item)
        {
            item.root.style.width = _cell.x;
            item.root.style.height = _cell.y;
            var iconSize = 24f + Mathf.Clamp(_size.value, Config.MIN_PRESET_BUTTON_SIZE, Config.MAX_PRESET_BUTTON_SIZE) * .48f;

            item.icon.style.width = Mathf.Min(iconSize, Mathf.Max(0, _cell.x - 16f));
            item.icon.style.height = iconSize;
        }

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
            Filter();
        }

        private void OnTag(string tag) => Filter();

        private bool IncludeTag(ParamDefine define) => _tagFilter.SelectedTag == null || string.Equals(define.tag, _tagFilter.SelectedTag, StringComparison.Ordinal);

        private void Filter()
        {
            _searchIndex.Search(_search.value, _matches, IncludeTag);
            RequestPage(0);
        }

        // 大小变化保留原页首项在新页面中, 同帧多次变化只处理一次.
        private void OnSize(ChangeEvent<int> evt) => _layoutDirty = true;

        private void OnViewportGeometry(GeometryChangedEvent evt)
        {
            if (evt.oldRect.size != evt.newRect.size)
            {
                _layoutDirty = true;
            }
        }

        // 完整单元格包含按钮留白, 网格间移动不会反复恢复默认详情.
        private void OnViewportLeave(PointerLeaveEvent evt)
        {
            _details.Restore(_hoveredPreset);
            _hoveredPreset = null;
        }

        private void RequestPage(int anchor)
        {
            DeactivateFrom(0);
            _anchor = anchor;
            _pageDirty = true;
        }

        private void ChangePage(int page) => RequestPage(page * _capacity);

        private sealed class Item
        {
            internal TemplateContainer root;

            internal Button button;

            internal Image icon;

            /// <summary> 单独保留原文的短名称. </summary>
            internal Label name;

            internal ParamDefine define;

            /// <summary> 避免重排时重复加载. </summary>
            internal bool iconResolved;

            internal Action unbind;
        }
    }
}
