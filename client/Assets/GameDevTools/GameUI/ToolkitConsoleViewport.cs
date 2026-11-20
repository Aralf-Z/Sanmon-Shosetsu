using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 原生滚动条控制逻辑位置, 独立可视层从最新日志向上准备可变高度行. </summary>
    internal sealed partial class ToolkitConsoleViewport : IDisposable
    {
        internal bool Following { get; private set; } = true;

        /// <summary> 原生滑块拖动期间不扩充逻辑范围或程序化回写位置. </summary>
        internal bool IsScrollbarDragging => _scrollbarPointer >= 0;

        /// <summary> 原生滑块或触摸拖动期间不回写位置或扩充逻辑范围. </summary>
        internal bool IsUserScrolling => IsScrollbarDragging || _touchPointer >= 0;

        internal long SelectedId { get; set; }

        private readonly ScrollView _scroll; // 保留原生滚轮, 滑块和触摸操作.

        private readonly ToolkitConsoleLogBuffer _model; // 逻辑日志范围与实际节点分离.

        private readonly VisualTreeAsset _template;

        private readonly Action<long> _select;

        private readonly VisualElement _extent = new();

        private readonly VisualElement _host = new(); // 位于原生 contentContainer 之外, 不受其延迟滚动变换影响.

        private readonly ToolkitConsoleHeightIndex _heights = new(); // 当前范围内的估算和已测量高度.

        private Anchor _anchor; // 改变行高或前插历史之前的阅读位置.

        private ConsoleLogStore _indexedStore;

        private ConsoleLogFilter _indexedFilter;

        private bool _indexedCollapsed;

        private int _indexedFirst = -1; // 高度索引对应的逻辑范围.

        private int _indexedEnd = -1;

        private int _indexedGeneration = -1;

        private float _width; // 当前正文可视宽度, 不包含滚动条.

        private float _height;

        private float _offset;

        private float _extentHeight = -1; // 避免重复改变原生内容高度.

        private int _fontKey;

        private int _scrollbarPointer = -1;

        private int _touchPointer = -1;

        private bool _visible;

        private bool _dirty;

        private bool _extentPending;

        private bool _pointerDown;

        private bool _pointerMoved; // 点击本身不触发历史加载.

        private bool _userPending;

        private bool _upwardPending;

        private bool _historyRequested;

        private bool _applying; // 程序化调整滑块不改变阅读状态.

        private bool _disposed;

        /// <summary> 稳定日志编号和相对于行顶部的偏移. </summary>
        internal readonly struct Anchor
        {
            /// <summary> 零表示没有阅读锚点. </summary>
            internal readonly long id;

            /// <summary> 包含多行日志内部的偏移, 单位为面板单位. </summary>
            internal readonly float offset;

            internal Anchor(long id, float offset)
            {
                this.id = id;
                this.offset = offset;
            }
        }

        internal ToolkitConsoleViewport(ScrollView scroll, ToolkitConsoleLogBuffer model, VisualTreeAsset template, Action<long> select)
        {
            _scroll = scroll;
            _model = model;
            _template = template;
            _select = select;
            _extent.AddToClassList("console-log-spacer");
            _extent.pickingMode = PickingMode.Ignore;
            _host.AddToClassList("console-log-host");
            _host.pickingMode = PickingMode.Ignore;
            _scroll.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
            _scroll.Add(_extent);
            _scroll.contentViewport.Add(_host);
            _measureRow = MakeRow(false);
            _scroll.verticalScroller.valueChanged += OnScroll;
            _scroll.contentViewport.RegisterCallback<GeometryChangedEvent>(Geometry);
            _extent.RegisterCallback<GeometryChangedEvent>(ExtentGeometry);
            RegisterInput();
        }

        /// <summary> 切页保留节点和测量缓存, 只清除尚未完成的用户手势. </summary>
        internal void SetVisible(bool visible)
        {
            _visible = visible;
            _pointerDown = _pointerMoved = _userPending = _upwardPending = _historyRequested = false;
            _scrollbarPointer = -1;
            _touchPointer = -1;
            if (visible)
            {
                _dirty = true;
            }
        }

        /// <summary> 筛选和最近范围一次提交, 同一范围的节点和高度不清空. </summary>
        internal void Reset()
        {
            Following = true;
            _scrollbarPointer = -1;
            _touchPointer = -1;
            _anchor = default;
            _pointerDown = _pointerMoved = _userPending = _upwardPending = _historyRequested = false;
            if (!ReferenceEquals(_indexedStore, _model.Store) || _indexedGeneration != _model.Store.Generation)
            {
                _layouts.Clear();
            }

            UpdateSize();
            if (!SameRange())
            {
                RebuildHeights();
            }

            _dirty = true;
            Render();
        }

        internal Anchor CaptureAnchor()
        {
            if (IsUserScrolling || Following || _model.Count == 0 || _heights.Count != _model.Count) return default;

            if (_anchor.id != 0) return _anchor;

            var index = _heights.Find(Mathf.Max(0, _offset - _heights.TopPadding(_height)));

            return new Anchor(_model.GetItem(index).Id, _offset - _heights.TopPadding(_height) - _heights.Prefix(index));
        }

        internal void Refresh(bool rangeChanged, Anchor anchor)
        {
            if (!Following && !IsUserScrolling && anchor.id != 0)
            {
                _anchor = anchor;
            }

            if (rangeChanged)
            {
                RebuildHeights();
            }

            _dirty = true;
        }

        internal void Tick()
        {
            if (!_visible || _disposed) return;

            if (IsScrollbarDragging && !Application.isFocused)
            {
                EndScrollbarDrag();
            }

            if (!Application.isFocused)
            {
                _touchPointer = -1;
            }

            UpdateSize();
            if (_model.Count > 0 && _fontKey != ToolkitFontMetrics.GetKey(_measureRow.text))
            {
                _dirty = true;
            }

            if (_userPending)
            {
                _userPending = false;
                Following = !IsScrollbarDragging && !_upwardPending && AtBottom();
                _upwardPending = false;
            }

            if (_dirty)
            {
                Render();
            }

            if (_extentPending && Mathf.Abs(_extent.layout.height - _extentHeight) <= 1f)
            {
                _extentPending = false;
                ApplyNativeOffset();
            }

            _pointerMoved = false;
        }

        /// <summary> 普通布局和高度校正不会请求更多历史. </summary>
        internal bool TakeHistoryRequest()
        {
            var requested = _historyRequested;

            _historyRequested = false;
            return requested;
        }

        public void Dispose()
        {
            _disposed = true;
            SetVisible(false);
            UnregisterInput();
            _scroll.verticalScroller.valueChanged -= OnScroll;
            _scroll.contentViewport.UnregisterCallback<GeometryChangedEvent>(Geometry);
            _extent.UnregisterCallback<GeometryChangedEvent>(ExtentGeometry);
            _host.RemoveFromHierarchy();
            _extent.RemoveFromHierarchy();
            ReleaseRows();
        }

        internal void ReleaseRows()
        {
            _host.Clear();
            _pool.Clear();
            _activeRows.Clear();
            _boundRows.Clear();
            _freeRows.Clear();
            _layouts.Clear();
        }

        private bool SameRange() => ReferenceEquals(_indexedStore, _model.Store) && _indexedGeneration == _model.Store.Generation && _indexedFirst == _model.First && _indexedEnd == _model.End && _indexedFilter == _model.Filter && _indexedCollapsed == _model.Collapsed;

        private void RebuildHeights()
        {
            _heights.Reset(_model.Count, CachedHeight);
            _indexedStore = _model.Store;
            _indexedGeneration = _model.Store.Generation;
            _indexedFilter = _model.Filter;
            _indexedCollapsed = _model.Collapsed;
            _indexedFirst = _model.First;
            _indexedEnd = _model.End;
        }

        private float CurrentOffset()
        {
            if (Following) return _heights.MaxOffset(_height);

            var index = _anchor.id == 0
                ? -1
                : _model.Find(_anchor.id);
            var value = index < 0
                ? _offset
                : _heights.TopPadding(_height) + _heights.Prefix(index) + _anchor.offset;

            return Mathf.Clamp(value, 0, _heights.MaxOffset(_height));
        }

        private bool AtBottom() => _heights.MaxOffset(_height) - _offset <= 1f;

        private static bool Valid(float value) => value > 0 && !float.IsNaN(value) && !float.IsInfinity(value);

        // 行高与位置已经提交, 原生内容即使晚一次布局也不会带动可视文字跳动.
        private void ApplyNativeOffset()
        {
            if (!_visible || _disposed || IsUserScrolling || !Valid(_height)) return;

            _applying = true;
            _scroll.verticalScroller.highValue = _heights.MaxOffset(_height);
            _scroll.verticalScroller.Adjust(Mathf.Min(1f, _height / Mathf.Max(_height, _heights.Total)));
            _scroll.scrollOffset = new Vector2(0, _offset);
            _applying = false;
        }

        private void UpdateSize()
        {
            var size = _scroll.contentViewport.contentRect.size;

            if (!Valid(size.x) || !Valid(size.y) || size == new Vector2(_width, _height)) return;

            var anchor = CaptureAnchor();
            var widthChanged = !Mathf.Approximately(_width, size.x);

            _width = size.x;
            _height = size.y;
            if (!Following && anchor.id != 0)
            {
                _anchor = anchor;
            }

            if (widthChanged)
            {
                RebuildHeights();
            }

            _dirty = true;
        }

        private void Geometry(GeometryChangedEvent evt)
        {
            if (evt.oldRect.size == evt.newRect.size || !_visible) return;

            UpdateSize();
            Render();
        }

        private void ExtentGeometry(GeometryChangedEvent evt)
        {
            if (evt.oldRect.height != evt.newRect.height && _visible)
            {
                ApplyNativeOffset();
            }
        }
    }
}
