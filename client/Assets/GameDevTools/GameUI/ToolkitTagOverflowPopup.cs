using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 跟随更多按钮的标签浮层, 固定高度, 加宽后仍溢出时分页. </summary>
    internal sealed partial class ToolkitTagOverflowPopup : IDisposable
    {
        private const float EDGE_MARGIN = 4f;

        private const float BUTTON_GAP = 4f;

        private const float DEFAULT_HEIGHT = 260f; // 面板单位, 不再随标签数量增高.

        internal bool IsOpen => _root != null;

        private readonly Button _btnTrigger;

        private readonly VisualElement _bar;

        private readonly Action<string> _select;

        private readonly TemplateContainer _host;

        private readonly VisualElement _popup;

        private readonly VisualElement _grid;

        private readonly VisualElement _pagerRoot;

        private readonly ToolkitPager _pager;

        private readonly List<Button> _buttons = new();

        private readonly List<Row> _rows = new();

        private readonly List<Page> _pages = new();

        private readonly IVisualElementScheduledItem _update;

        private VisualElement _root;

        private VisualElement _keyboardRoot;

        private IReadOnlyList<string> _tags;

        private Vector2[] _preferred = Array.Empty<Vector2>();

        private Rect[] _rects = Array.Empty<Rect>();

        private bool _disposed;

        private bool _measureDirty = true;

        private bool _layoutDirty = true;

        private float _width;

        private float _contentHeight;

        private float _barWidth = -1f;

        private Vector2 _available = new(-1f, -1f);

        private int _fontKey;

        private int _readyFrame;

        private int _page;

        private int _anchor;

        internal ToolkitTagOverflowPopup(Button trigger, VisualElement bar, Action<string> select)
        {
            _btnTrigger = trigger;
            _bar = bar;
            _select = select;
            _host = ToolkitFactory.InstantiateTemplate("ToolkitSearchHistoryPopup");
            _host.name = "tagOverflowLayer";
            _host.AddToClassList("search-history-layer");
            _host.pickingMode = PickingMode.Position;
            _host.focusable = true;
            _popup = ToolkitView.Require<VisualElement>(_host, "searchHistoryPopup");
            _popup.AddToClassList("tag-overflow-popup");
            ToolkitView.Require<ScrollView>(_host, "searchHistoryScroll").RemoveFromHierarchy();
            _grid = new VisualElement { name = "tagOverflowGrid" };
            _grid.AddToClassList("tag-overflow-grid");
            _pagerRoot = new VisualElement { name = "tagOverflowPager" };
            _pagerRoot.AddToClassList("tag-overflow-pager");
            _pager = new ToolkitPager(_pagerRoot, ChangePage);
            _popup.Add(_grid);
            _popup.Add(_pagerRoot);
            _update = _host.schedule.Execute(UpdateBounds).Every(16);
            _update.Pause();
            _host.RegisterCallback<PointerDownEvent>(PointerDown, TrickleDown.TrickleDown);
            _btnTrigger.RegisterCallback<DetachFromPanelEvent>(Detached);
        }

        internal void Show(IReadOnlyList<string> tags, string selected)
        {
            Close();
            if (_disposed || _btnTrigger.panel == null) return;

            _tags = tags;
            _anchor = 0;
            BuildButtons(selected);
            _root = ToolkitView.GetOverlayRoot(_btnTrigger);
            _host.style.visibility = Visibility.Hidden;
            _root.Add(_host);
            _keyboardRoot = _host.panel.visualTree;
            _keyboardRoot.RegisterCallback<KeyDownEvent>(KeyDown, TrickleDown.TrickleDown);
            ToolkitTextOpacity.Track(_host);
            Application.focusChanged += FocusChanged;
            _readyFrame = Time.frameCount + 1;
            _layoutDirty = true;
            _host.Focus();
            _update.Resume();
        }

        internal void Refresh(IReadOnlyList<string> tags, string selected)
        {
            _tags = tags;
            if (!IsOpen) return;

            UpdateLabels(selected);
            _readyFrame = Time.frameCount + 1;
        }

        internal void Close(bool restoreFocus = false)
        {
            if (!IsOpen) return;

            _update.Pause();
            Application.focusChanged -= FocusChanged;
            _keyboardRoot?.UnregisterCallback<KeyDownEvent>(KeyDown, TrickleDown.TrickleDown);
            _keyboardRoot = null;
            _root = null;
            _host.RemoveFromHierarchy();
            _grid.Clear();
            _buttons.Clear();
            _rows.Clear();
            _pages.Clear();
            if (restoreFocus && _btnTrigger.panel != null)
            {
                _btnTrigger.Focus();
            }
        }

        private void BuildButtons(string selected)
        {
            AddButton(null);
            foreach (var tag in _tags)
            {
                AddButton(tag);
                if (string.Equals(tag, selected, StringComparison.Ordinal))
                {
                    _anchor = _buttons.Count - 1;
                }
            }

            _preferred = new Vector2[_buttons.Count];
            _rects = new Rect[_buttons.Count];
            _pager.ReservePages(_buttons.Count);
            UpdateLabels(selected);
        }

        private void AddButton(string tag)
        {
            var btnTag = new Button(() => Choose(tag)) { userData = tag, enableRichText = false, parseEscapeSequences = false };

            btnTag.AddToClassList("category-button");
            btnTag.AddToClassList("tag-filter-button");
            btnTag.AddToClassList("tag-overflow-button");
            _buttons.Add(btnTag);
            _grid.Add(btnTag);
        }

        private void UpdateLabels(string selected)
        {
            _measureDirty = true;
            foreach (var btnTag in _buttons)
            {
                var tag = btnTag.userData as string;

                btnTag.text = tag ?? ToolkitLocalization.Get("ui.CheatPanel.EnsureCommands");
                btnTag.tooltip = btnTag.text;
                btnTag.EnableInClassList("selected", tag == selected);
            }
        }

        // 调度中更新锚点, 不在绘制回调中改变节点或分页结构.
        private void UpdateBounds()
        {
            if (!IsOpen || Application.isPlaying && Time.frameCount < _readyFrame) return;

            if (_btnTrigger.panel == null || _btnTrigger.resolvedStyle.display == DisplayStyle.None)
            {
                Close();
                return;
            }

            var key = ToolkitFontMetrics.GetKey(_buttons[0]);

            if (_measureDirty || _fontKey != key)
            {
                _fontKey = key;
                MeasureButtons();
            }

            var viewport = _root.contentRect;
            var anchor = _root.WorldToLocal(_btnTrigger.worldBound);
            var height = CalculateBounds(viewport, anchor, 0, DEFAULT_HEIGHT).height;
            var available = new Vector2(Mathf.Max(1f, viewport.width - EDGE_MARGIN * 2), height);
            var barWidth = Mathf.Max(1f, anchor.xMax - _root.WorldToLocal(_bar.worldBound).xMin);

            if (_layoutDirty || _available != available || _barWidth != barWidth)
            {
                _available = available;
                _barWidth = barWidth;
                Reflow(Mathf.Min(available.x, barWidth * .5f), Mathf.Min(available.x, barWidth), height);
            }

            var bounds = CalculateBounds(viewport, anchor, _width, DEFAULT_HEIGHT);

            _popup.style.left = bounds.x;
            _popup.style.top = bounds.y;
            _popup.style.width = bounds.width;
            _popup.style.height = bounds.height;
            _host.style.visibility = Visibility.Visible;
        }

        private void Choose(string tag)
        {
            if (!IsOpen) return;

            Close(true);
            _select(tag);
        }

        private void PointerDown(PointerDownEvent evt)
        {
            if (_popup.worldBound.Contains(evt.position)) return;

            Close(true);
            evt.StopImmediatePropagation();
            evt.PreventDefault();
        }

        private void KeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Escape) return;

            Close(true);
            evt.StopImmediatePropagation();
            evt.PreventDefault();
        }

        private void FocusChanged(bool focused)
        {
            if (!focused)
            {
                Close();
            }
        }

        private void Detached(DetachFromPanelEvent evt) => Close();

        public void Dispose()
        {
            Close();
            _disposed = true;
            _update.Pause();
            _host.UnregisterCallback<PointerDownEvent>(PointerDown, TrickleDown.TrickleDown);
            _btnTrigger.UnregisterCallback<DetachFromPanelEvent>(Detached);
            _pager.Dispose();
        }
    }
}
