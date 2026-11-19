using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 设置页共用的搜索和输入历史窗口. </summary>
    internal sealed class ToolkitHistoryWindow : IDisposable
    {
        private readonly VisualElement _owner;

        private readonly Button _trigger;

        private readonly TemplateContainer _host;

        private readonly VisualElement _dialog;

        private readonly ScrollView _scroll;

        private readonly VisualElement _entries;

        private readonly Label _count;

        private readonly Label _title;

        private readonly Label _hint;

        private readonly Button _close;

        private readonly bool _showInputHistory;

        private readonly List<string> _shown = new();

        private VisualElement _root;

        private bool _disposed; // 销毁后不再创建窗口.

        internal ToolkitHistoryWindow(VisualElement owner, Button trigger, bool showInputHistory = false)
        {
            _owner = owner;
            _trigger = trigger;
            _showInputHistory = showInputHistory;
            _host = ToolkitFactory.InstantiateTemplate("ToolkitSearchHistoryWindow");
            _host.AddToClassList("search-history-window-layer");
            _host.pickingMode = PickingMode.Position;
            _host.focusable = true;
            _dialog = ToolkitView.Require<VisualElement>(_host, "searchHistoryDialog");
            _scroll = ToolkitView.Require<ScrollView>(_host, "searchHistoryWindowScroll");
            _entries = ToolkitView.Require<VisualElement>(_host, "searchHistoryEntries");
            _count = ToolkitView.Require<Label>(_host, "searchHistoryWindowCount");
            _title = ToolkitView.Require<Label>(_host, "historyWindowTitle");
            _hint = ToolkitView.Require<Label>(_host, "historyWindowHint");
            _close = ToolkitView.Require<Button>(_host, "closeSearchHistoryWindow");
            _close.clicked += CloseFromButton;
            _host.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            _host.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            _owner.RegisterCallback<DetachFromPanelEvent>(OnDetached);
            ToolkitLocalization.e_onChanged += Refresh;
            if (_showInputHistory)
            {
                Config.e_onInputHistoryChanged += Refresh;
            }
            else
            {
                Config.e_onChanged += Refresh;
            }
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

            var history = _showInputHistory ? Config.InputHistoryEntries : Config.CheatSearchHistory;
            var capacity = _showInputHistory ? Config.ActiveInputHistoryCapacity : Config.CheatSearchHistoryCapacity;
            var ordered = _showInputHistory ? history.Reverse() : history;
            var emptyText = ToolkitLocalization.Get(_showInputHistory ? "ui.InputHistoryWindow.empty" : "ui.SearchHistoryWindow.Refresh");
            var deleteText = ToolkitLocalization.Get("ui.SearchHistoryWindow.Refresh.2");
            var deleteTooltip = ToolkitLocalization.Get(_showInputHistory ? "ui.InputHistoryWindow.delete.tooltip" : "ui.SearchHistoryWindow.Refresh.3");

            _title.text = ToolkitLocalization.Get(_showInputHistory ? "ui.inputHistory" : "ui.SearchHistoryWindow.label1.text");
            _hint.text = ToolkitLocalization.Get(_showInputHistory ? "ui.InputHistoryWindow.hint" : "ui.SearchHistoryWindow.label5.text");
            _close.tooltip = ToolkitLocalization.Get(_showInputHistory ? "ui.InputHistoryWindow.close.tooltip" : "ui.SearchHistoryWindow.closeSearchHistoryWindow.tooltip");
            _count.text = ToolkitLocalization.Format("history.count", ("count", (object)history.Count), ("capacity", (object)capacity));
            if (_shown.SequenceEqual(ordered) && _entries.childCount > 0)
            {
                foreach (var button in _entries.Query<Button>()
                    .ToList())
                {
                    button.text = deleteText;
                    button.tooltip = deleteTooltip;
                }

                if (history.Count == 0)
                {
                    _entries.Q<Label>().text = emptyText;
                }

                return;
            }

            _shown.Clear();
            _shown.AddRange(ordered);
            _entries.Clear();
            if (history.Count == 0)
            {
                var empty = new Label(emptyText);

                empty.AddToClassList("muted");
                _entries.Add(empty);
                ToolkitTextOpacity.Track(empty);
                return;
            }

            for (var index = 0; index < _shown.Count; index++)
            {
                var displayedIndex = index;
                var row = new VisualElement();

                row.AddToClassList("search-history-record");
                var label = new Label(_shown[index])
                { enableRichText = false, parseEscapeSequences = false };

                label.AddToClassList("search-history-record-text");
                label.selection.isSelectable = true;
                var remove = new Button(() => Remove(row, displayedIndex))
                { text = deleteText, tooltip = deleteTooltip };

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
            ToolkitLocalization.e_onChanged -= Refresh;
            Config.e_onInputHistoryChanged -= Refresh;
            Config.e_onChanged -= Refresh;
            _close.clicked -= CloseFromButton;
            _host.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            _host.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            _owner.UnregisterCallback<DetachFromPanelEvent>(OnDetached);
        }

        private void Remove(VisualElement row, int displayedIndex)
        {
            if (row.parent != _entries) return;

            if (_showInputHistory)
            {
                Config.RemoveInputHistory(_shown.Count - 1 - displayedIndex);
            }
            else
            {
                Config.RemoveCheatSearch(_shown[displayedIndex]);
            }

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
