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
        private readonly VisualElement _owner; // 所属设置页, 隐藏或销毁时由宿主关闭.
        private readonly Button _trigger; // 关闭时返回查看历史按钮.
        private readonly TemplateContainer _host; // 视口居中的独立覆盖层.
        private readonly VisualElement _dialog; // 实际窗口, 用于识别外部点击.
        private readonly ScrollView _scroll; // 历史较多时原生滚动.
        private readonly VisualElement _entries; // 完整搜索词和删除按钮列表.
        private readonly Label _count; // 当前历史条数及容量.
        private readonly Button _close; // 关闭窗口.
        private readonly List<string> _shown = new(); // 无关配置保存不重建列表或改变滚动位置.
        private VisualElement _root; // 打开时所在的面板根节点.
        private bool _disposed; // 销毁后不再创建窗口.

        /// <summary> 绑定独立模板, 不连接命令执行或搜索框的输入逻辑. </summary>
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

        /// <summary> 从最新记录开始展示, 不把查看动作记为一次搜索. </summary>
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

        /// <summary> 更新数量及变化的历史内容, 删除最后一项时显示空列表提示. </summary>
        internal void Refresh()
        {
            if (_root == null) return;
            var history = Config.CheatSearchHistory;
            _count.text = ToolkitLocalization.Format("history.count", ("count", (object)history.Count), ("capacity", (object)Config.CheatSearchHistoryCapacity));
            if (_shown.SequenceEqual(history) && _entries.childCount > 0)
            {
                foreach (var button in _entries.Query<Button>().ToList())
                {
                    button.text = ToolkitLocalization.Get("ui.SearchHistoryWindow.Refresh.2");
                    button.tooltip = ToolkitLocalization.Get("ui.SearchHistoryWindow.Refresh.3");
                }
                if (history.Count == 0) _entries.Q<Label>().text = ToolkitLocalization.Get("ui.SearchHistoryWindow.Refresh");
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
                var label = new Label(query) { enableRichText = false, parseEscapeSequences = false };
                label.AddToClassList("search-history-record-text");
                label.selection.isSelectable = true;
                var remove = new Button(() => Remove(query)) { text = ToolkitLocalization.Get("ui.SearchHistoryWindow.Refresh.2"), tooltip = ToolkitLocalization.Get("ui.SearchHistoryWindow.Refresh.3") };
                remove.AddToClassList("search-history-record-delete");
                row.Add(label);
                row.Add(remove);
                _entries.Add(row);
                ToolkitTextOpacity.Track(row);
            }
        }

        /// <summary> 关闭管理窗口, 不清空历史和各搜索框的输入. </summary>
        internal void Close(bool restoreFocus = false)
        {
            if (_root == null) return;
            _root = null;
            _host.RemoveFromHierarchy();
            _entries.Clear();
            _shown.Clear();
            if (restoreFocus && _trigger.panel != null) _trigger.Focus();
        }

        /// <summary> 解除全局配置订阅并释放覆盖层. </summary>
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

        // 普通关闭返回原入口.
        private void CloseFromButton() => Close(true);

        // 采用实际矩形判断, 不拦截窗口内的文字选择或滚动条操作.
        private void OnPointerDown(PointerDownEvent evt)
        {
            if (_dialog.worldBound.Contains(evt.position)) return;
            Close(true);
            evt.StopImmediatePropagation();
            evt.PreventDefault();
        }

        // Esc 只关闭历史窗口.
        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Escape) return;
            Close(true);
            evt.StopImmediatePropagation();
            evt.PreventDefault();
        }

        // 控件离开面板时不遗留独立覆盖层.
        private void OnDetached(DetachFromPanelEvent evt) => Close();
    }
}
