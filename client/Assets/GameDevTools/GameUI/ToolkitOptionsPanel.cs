using System;
using System.Collections.Generic;
using System.Globalization;

using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 编辑控制台, Cheat 与浮标配置, 保留各项设置的生效和保存时机. </summary>
    public sealed partial class ToolkitOptionsPanel : IDisposable
    {

        private readonly List<Action> _unbind = new(); // 释放设置控件的事件绑定.
        private readonly List<Action> _refresh = new(); // 配置保存后同步 Inspector 和 Options.
        private readonly List<VisualElement> _rows = new(); // UXML 中现有设置项, 分帧显示且保留编辑值.
        private readonly ToolkitFloatingSettingsControls _floatingSettings; // 与图标弹窗共用的即时设置.
        private readonly ToolkitAppearanceController _appearance; // 准备成功后才提交字体或风格并保存.
        private readonly ToolkitTextDisplay _text = new(); // 仅处理固定标签, 不省略可编辑数值.
        private readonly ToolkitOptionsLayout _layout; // 按每个设置分区的真实宽度划分四成和六成.
        private readonly VisualElement _root; // 历史管理窗口的所属设置页.
        private readonly Button _clearHistory; // 清空三处搜索共用的历史.
        private readonly Button _viewHistory; // 查看完整历史并逐条删除.
        private ToolkitSearchHistoryWindow _historyWindow; // 按需创建的独立历史管理窗口.
        private ToolkitDropdownPopup _styleMenu; // 与搜索历史共用弹层, 最多显示五项.
        private ToolkitDropdownPopup _fontMenu; // 全部 UI 风格共用的即时字体选择.
        private int _nextRow; // 下一项待显示的设置.
        private int _visibleFrame; // 切页当帧只呈现页面框架.
        private int _lastLoadFrame = -1; // 同一帧不重复推进显示.
        private bool _visible; // 隐藏时暂停显示剩余设置.
        private bool _dirty; // 仅修改配置后进行保存.

        /// <summary> 将现有配置绑定到 UXML 中的整数输入和开关. </summary>
        public ToolkitOptionsPanel(VisualElement root, ToolkitWindowSettings windowSettings, ToolkitAppearanceController appearance)
        {
            _root = root;
            _appearance = appearance;
            _layout = new ToolkitOptionsLayout(root);
            _clearHistory = ToolkitView.Require<Button>(root, "clearCheatSearchHistory");
            _viewHistory = ToolkitView.Require<Button>(root, "viewCheatSearchHistory");
            _clearHistory.clicked += ClearHistory;
            _viewHistory.clicked += ViewHistory;
            _refresh.Add(RefreshHistoryButtons);
            RefreshHistoryButtons();
            _rows.AddRange(root.Query<VisualElement>(className: "setting-row").ToList());
            _text.Track(ToolkitView.Require<Label>(root, "consoleSettingsTitle"));
            _text.Track(ToolkitView.Require<Label>(root, "interfaceSettingsTitle"));
            _text.Track(ToolkitView.Require<Label>(root, "toolSettingsTitle"));
            _text.Track(ToolkitView.Require<Label>(root, "cheatSettingsTitle"));
            _text.Track(ToolkitView.Require<Label>(root, "floatingSettingsTitle"));
            BindInteger(root, "inputHistoryCapacity", () => Config.InputHistoryCapacity, value => Config.InputHistoryCapacity = value);
            BindInteger(root, "commandQueryCacheCapacity", () => Config.CommandQueryCacheCapacity, value => Config.CommandQueryCacheCapacity = value);
            BindInteger(root, "alternativeCommandCount", () => Config.AlternativeCommandCount, value => Config.AlternativeCommandCount = value);
            BindInteger(root, "cheatSearchHistoryCapacity", () => Config.CheatSearchHistoryCapacity, value => Config.CheatSearchHistoryCapacity = value, Config.MIN_CHEAT_SEARCH_HISTORY_CAPACITY, Config.MAX_CHEAT_SEARCH_HISTORY_CAPACITY, true);
            BindPanelOpacity(root, windowSettings);
            BindStyle(root);
            BindFont(root);
            BindLanguage(root);
            BindShortcut(root);
            BindToggle(root, "minimizeOnShortcut", () => Config.MinimizeOnShortcut, value => Config.MinimizeOnShortcut = value, true);
            _text.Track(ToolkitView.Require<SliderInt>(root, "cheatPreviewLogLines").labelElement);
            BindToggle(root, "consoleShowLogIcons", () => Config.ConsoleShowLogIcons, value => Config.ConsoleShowLogIcons = value);
            BindToggle(root, "consoleShowTimestamps", () => Config.ConsoleShowTimestamps, value => Config.ConsoleShowTimestamps = value);
            BindToggle(root, "consoleCollapseLogs", () => Config.ConsoleCollapseLogs, value => Config.ConsoleCollapseLogs = value);
            BindToggle(root, "recordFailedCommand", () => Config.RecordFailedCommand, value => Config.RecordFailedCommand = value);
            BindToggle(root, "outputVmExceptionStack", () => Config.OutputVmExceptionStack, value => Config.OutputVmExceptionStack = value);
            _floatingSettings = new ToolkitFloatingSettingsControls(root, windowSettings, "floating");
            Action refresh = () => { foreach (var action in _refresh) action(); };
            windowSettings.e_onChanged += refresh;
            _unbind.Add(() => windowSettings.e_onChanged -= refresh);
            Config.e_onChanged += refresh;
            _unbind.Add(() => Config.e_onChanged -= refresh);
            _appearance.e_onChanging += OnAppearanceChanging;
            _unbind.Add(() => _appearance.e_onChanging -= OnAppearanceChanging);
        }

        /// <summary> 每次切入都逐帧显示已有控件, 不重建或重置设置值. </summary>
        public void SetVisible(bool visible)
        {
            if (_visible == visible)
                return;
            _visible = visible;
            if (!visible)
            {
                CloseSelectionMenus();
                _historyWindow?.Close();
                return;
            }
            _visibleFrame = Time.frameCount;
            _layout.Invalidate();
            _nextRow = 0;
            foreach (var row in _rows)
                row.AddToClassList("is-hidden");
        }

        /// <summary> 切页后一帧开始逐项呈现, 只处理当前可见设置页. </summary>
        public void Tick()
        {
            if (!_visible || Time.frameCount <= _visibleFrame || _lastLoadFrame == Time.frameCount)
                return;
            _lastLoadFrame = Time.frameCount;
            for (var added = 0; added < Config.OPTIONS_ROWS_PER_FRAME && _nextRow < _rows.Count; added++)
                _rows[_nextRow++].RemoveFromClassList("is-hidden");
            _layout.Tick();
            _floatingSettings.Tick();
            _text.Tick();
        }

        /// <summary> 切页和关闭时保存, 失败时保留待保存标记. </summary>
        public void Save()
        {
            if (!_dirty)
                return;
            if (Config.Save()) _dirty = false;
        }

        /// <summary> 解除所有设置回调. </summary>
        public void Dispose()
        {
            _visible = false;
            _layout.Dispose();
            _styleMenu?.Dispose();
            _styleMenu = null;
            _fontMenu?.Dispose();
            _shortcutModifierMenu?.Dispose();
            _shortcutKeyMenu?.Dispose();
            _languageMenu?.Dispose();
            _fontMenu = null;
            _historyWindow?.Dispose();
            _historyWindow = null;
            _clearHistory.clicked -= ClearHistory;
            _viewHistory.clicked -= ViewHistory;
            _floatingSettings.Dispose();
            _text.Dispose();
            foreach (var unbind in _unbind)
                unbind();
            _unbind.Clear();
        }

        /// <summary> 切页, 帮助或隐藏时关闭临时选择菜单, 不修改配置. </summary>
        internal void CloseSelectionMenus()
        {
            _styleMenu?.Close();
            _fontMenu?.Close();
            _languageMenu?.Close();
            _shortcutModifierMenu?.Close();
            _shortcutKeyMenu?.Close();
        }

        // 清空仅作用于搜索历史, 不改动 Console 的命令输入历史.
        private void ClearHistory()
        {
            Config.ClearCheatSearchHistory();
            RefreshHistoryButtons();
            _historyWindow?.Refresh();
        }

        // 只在用户首次查看时创建管理窗口.
        private void ViewHistory()
        {
            _historyWindow ??= new ToolkitSearchHistoryWindow(_root, _viewHistory);
            _historyWindow.Show();
        }

        // 空历史仍可查看提示, 清空按钮在没有记录时禁用.
        private void RefreshHistoryButtons() => _clearHistory.SetEnabled(Config.CheatSearchHistory.Count > 0);

        // 复用窗口设置的即时通知及合并保存, 不触发重新构造页面.
        private void BindPanelOpacity(VisualElement root, ToolkitWindowSettings settings)
        {
            var field = ToolkitView.Require<Slider>(root, "panelOpacity");
            field.lowValue = Config.MIN_OPACITY;
            field.highValue = Config.MAX_OPACITY;
            _text.Track(field.labelElement);
            Action refresh = () =>
            {
                var value = settings.PanelOpacity;
                field.SetValueWithoutNotify(value);
                _text.SetText(field.labelElement, ToolkitLocalization.Format("ui.OptionsPanel.BindPanelOpacity", ("p0", (object)(value.ToString("0.00", CultureInfo.InvariantCulture)))));
            };
            refresh();
            EventCallback<ChangeEvent<float>> change = evt =>
            {
                var value = float.IsNaN(evt.newValue) || float.IsInfinity(evt.newValue) ? Config.DEFAULT_PANEL_OPACITY : Mathf.Clamp(evt.newValue, Config.MIN_PANEL_OPACITY, Config.MAX_OPACITY);
                field.SetValueWithoutNotify(value);
                settings.PanelOpacity = value;
            };
            field.RegisterValueChangedCallback(change);
            _unbind.Add(() => field.UnregisterValueChangedCallback(change));
            _refresh.Add(refresh);
        }

        // 显示名称和资源路径分开, 准备好新资源后才原位替换.
        private void BindStyle(VisualElement root)
        {
            var field = ToolkitView.Require<Button>(root, "uiStyle");
            var title = ToolkitView.Require<Label>(root, "uiStyleName");
            _text.Track(field.parent.Q<Label>(className: "unity-base-field__label"));
            title.enableRichText = false;
            title.parseEscapeSequences = false;
            _text.Track(title);
            Action refresh = () => _text.SetText(title, ToolkitFactory.GetStyleName(Config.UIStyle));
            refresh();
            Action click = () =>
            {
                if (_appearance.IsChanging) return;
                if (_styleMenu?.IsOpen == true)
                {
                    _styleMenu.Close(true);
                    return;
                }
                _styleMenu ??= new ToolkitDropdownPopup(field, field, Config.SELECTOR_VISIBLE_ITEMS);
                CloseSelectionMenus();
                var styles = ToolkitFactory.Styles;
                var names = new List<string>(styles.Count);
                foreach (var entry in styles) names.Add(entry.name);
                _styleMenu.Show(names, index =>
                {
                    if (!_visible || _appearance.IsChanging) return;
                    _appearance.RequestStyle(styles[index].path);
                });
            };
            field.clicked += click;
            _unbind.Add(() => field.clicked -= click);
            _refresh.Add(refresh);
        }

        // 字体按资源键选择, 异步加载期间不改变旧界面的布局.
        private void BindFont(VisualElement root)
        {
            var field = ToolkitView.Require<Button>(root, "uiFont");
            var title = ToolkitView.Require<Label>(root, "uiFontName");
            title.enableRichText = false;
            title.parseEscapeSequences = false;
            _text.Track(field.parent.Q<Label>(className: "unity-base-field__label"));
            _text.Track(title);
            Action refresh = () => _text.SetText(title, ToolkitFactory.GetFontName(Config.UIFont));
            refresh();
            Action click = () =>
            {
                if (_appearance.IsChanging) return;
                if (_fontMenu?.IsOpen == true)
                {
                    _fontMenu.Close(true);
                    return;
                }
                _fontMenu ??= new ToolkitDropdownPopup(field, field, Config.SELECTOR_VISIBLE_ITEMS);
                CloseSelectionMenus();
                var fonts = ToolkitFactory.Fonts;
                var names = new List<string>(fonts.Count);
                foreach (var entry in fonts) names.Add(entry.name);
                _fontMenu.Show(names, index =>
                {
                    if (!_visible || _appearance.IsChanging) return;
                    _appearance.RequestFont(fonts[index].path);
                });
            };
            field.clicked += click;
            _unbind.Add(() => field.clicked -= click);
            _refresh.Add(refresh);
        }

        // 两个入口共同加锁, 不影响其他设置, 日志或正在运行的宏.
        private void OnAppearanceChanging(bool changing)
        {
            if (!changing) _layout.Invalidate();
            if (changing) CloseSelectionMenus();
            ToolkitView.Require<Button>(_root, "uiStyle").SetEnabled(!changing);
            ToolkitView.Require<Button>(_root, "uiFont").SetEnabled(!changing);
            ToolkitView.Require<Button>(_root, "uiLanguage").SetEnabled(!changing);
            if (!changing) foreach (var refresh in _refresh) refresh();
        }

        // 输入结束后使用统一定义校正配置值.
        private void BindInteger(VisualElement root, string name, Func<int> getter, Action<int> setter, int minimum = Config.MIN_COMMAND_CAPACITY, int maximum = Config.MAX_COMMAND_CAPACITY, bool saveImmediately = false)
        {
            var field = ToolkitView.Require<IntegerField>(root, name);
            _text.Track(field.labelElement);
            field.SetValueWithoutNotify(getter());
            EventCallback<ChangeEvent<int>> changed = evt =>
            {
                var value = Mathf.Clamp(evt.newValue, minimum, maximum);
                field.SetValueWithoutNotify(value);
                setter(value);
                _dirty = true;
                if (saveImmediately) Save();
            };
            field.RegisterValueChangedCallback(changed);
            _unbind.Add(() => field.UnregisterValueChangedCallback(changed));
            _refresh.Add(() => field.SetValueWithoutNotify(getter()));
        }

        // 通常由页签和窗口生命周期保存, 快捷键行为可选择立即保存.
        private void BindToggle(VisualElement root, string name, Func<bool> getter, Action<bool> setter, bool saveImmediately = false)
        {
            var field = ToolkitView.Require<Toggle>(root, name);
            _text.Track(field.labelElement);
            field.SetValueWithoutNotify(getter());
            EventCallback<ChangeEvent<bool>> changed = evt =>
            {
                setter(evt.newValue);
                _dirty = true;
                if (saveImmediately) Save();
            };
            field.RegisterValueChangedCallback(changed);
            _unbind.Add(() => field.UnregisterValueChangedCallback(changed));
            _refresh.Add(() => field.SetValueWithoutNotify(getter()));
        }
    }
}
