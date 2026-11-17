using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> Options 中的共用搜索历史管理窗口, 查看完整搜索词并逐条删除. </summary>
    internal sealed class ToolkitSearchHistoryWindow : IDisposable
    {
        private readonly VisualElement _owner;

        private readonly Button _trigger;

        private readonly TemplateContainer _host;

        private readonly VisualElement _dialog;

        private readonly ScrollView _scroll;

        private readonly VisualElement _entries;

        private readonly Label _count;

        private readonly Button _close;

        private readonly List<string> _shown = new();

        private VisualElement _root;

        private bool _disposed; // 销毁后不再创建窗口.

        internal ToolkitSearchHistoryWindow(VisualElement owner, Button trigger)
        {
            _owner = owner;
            _trigger = trigger;
            _host = ToolkitFactory.InstantiateTemplate("ToolkitSearchHistoryWindow");
            _host.AddToClassList("search-history-window-layer");
            _host.pickingMode = PickingMode.Position;
            _host.focusable = true;
            _dialog = ToolkitView.Require<VisualElement>(_host, "searchHistoryDialog");
            _scroll = ToolkitView.Require<ScrollView>(_host, "searchHistoryWindowScroll");
            _entries = ToolkitView.Require<VisualElement>(_host, "searchHistoryEntries");
            _count = ToolkitView.Require<Label>(_host, "searchHistoryWindowCount");
            _close = ToolkitView.Require<Button>(_host, "closeSearchHistoryWindow");
            _close.clicked += CloseFromButton;
            _host.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            _host.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            _owner.RegisterCallback<DetachFromPanelEvent>(OnDetached);
            Config.e_onChanged += Refresh;
        }

        internal void Show()
        {
            if (_disposed || _owner.panel == null) return;

            if (_root == null)
            {
                _root = ToolkitView.GetOverlayRoot(_owner);
                _root.Add(_host);
            }

            Refresh();
            _scroll.scrollOffset = Vector2.zero;
            _host.Focus();
        }

        internal void Refresh()
        {
            if (_root == null) return;

            var history = Config.CheatSearchHistory;

            _count.text = ToolkitLocalization.Format("history.count", ("count", (object)history.Count), ("capacity", (object)Config.CheatSearchHistoryCapacity));
            if (_shown.SequenceEqual(history) && _entries.childCount > 0)
            {
                foreach (var button in _entries.Query<Button>()
                    .ToList())
                {
                    button.text = ToolkitLocalization.Get("ui.SearchHistoryWindow.Refresh.2");
                    button.tooltip = ToolkitLocalization.Get("ui.SearchHistoryWindow.Refresh.3");
                }

                if (history.Count == 0)
                {
                    _entries.Q<Label>().text = ToolkitLocalization.Get("ui.SearchHistoryWindow.Refresh");
                }

                return;
            }

            _shown.Clear();
            _shown.AddRange(history);
            _entries.Clear();
            if (history.Count == 0)
            {
                var empty = new Label(ToolkitLocalization.Get("ui.SearchHistoryWindow.Refresh"));

                empty.AddToClassList("muted");
                _entries.Add(empty);
                ToolkitTextOpacity.Track(empty);
                return;
            }

            foreach (var text in history)
            {
                var query = text;
                var row = new VisualElement();

                row.AddToClassList("search-history-record");
                var label = new Label(query)
                { enableRichText = false, parseEscapeSequences = false };

                label.AddToClassList("search-history-record-text");
                label.selection.isSelectable = true;
                var remove = new Button(() => Remove(query))
                { text = ToolkitLocalization.Get("ui.SearchHistoryWindow.Refresh.2"), tooltip = ToolkitLocalization.Get("ui.SearchHistoryWindow.Refresh.3") };

                remove.AddToClassList("search-history-record-delete");
                row.Add(label);
                row.Add(remove);
                _entries.Add(row);
                ToolkitTextOpacity.Track(row);
            }
        }

        internal void Close(bool restoreFocus = false)
        {
            if (_root == null) return;

            _root = null;
            _host.RemoveFromHierarchy();
            _entries.Clear();
            _shown.Clear();
            if (restoreFocus && _trigger.panel != null)
            {
                _trigger.Focus();
            }
        }

        public void Dispose()
        {
            Close();
            _disposed = true;
            Config.e_onChanged -= Refresh;
            _close.clicked -= CloseFromButton;
            _host.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            _host.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            _owner.UnregisterCallback<DetachFromPanelEvent>(OnDetached);
        }

        // 只删除对应文本, 其他历史的相对顺序和当前搜索输入均保留.
        private void Remove(string query)
        {
            Config.RemoveCheatSearch(query);
            Refresh();
        }

        private void CloseFromButton() => Close(true);

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (_dialog.worldBound.Contains(evt.position)) return;

            Close(true);
            evt.StopImmediatePropagation();
            evt.PreventDefault();
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Escape) return;

            Close(true);
            evt.StopImmediatePropagation();
            evt.PreventDefault();
        }

        private void OnDetached(DetachFromPanelEvent evt) => Close();
    }
}
