using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using GameConsole.Extension;
using RedSaw.CommandLineInterface;

namespace GameConsole.UI
{
    /// <summary> 管理工具页签, 生命周期和控制台适配器. </summary>
    public sealed partial class ToolkitView : IDisposable
    {
        public bool IsMinimized => _isOpen && _windowController.IsMinimized;

        public ToolkitConsoleRenderer Renderer => _renderer;

        public ToolkitConsoleInput Input => _input;

        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                if (_disposed || _isOpen == value) return;

                if (!value)
                {
                    _macroAttempted = false;
                    _help.Close();
                    _renderer.QuitFocus();
                    SaveOptions();
                    _renderer.IsAlternativeOptionsActive = false;
                }

                _isOpen = value;
                _windowController.SetVisible(value);
                _renderer.WindowVisible = value;
                UpdatePageLoading();
                _input?.Reset();
                if (value && _currentTab == "console")
                {
                    _renderer.ActivateInput();
                }
            }
        }

        private readonly GameObject _gameObject;

        private readonly PanelSettings _panelSettings; // 此窗口独占的临时 PanelSettings.

        private readonly ToolkitNavigationBlocker _navigationBlocker; // 屏蔽独占面板的输入导航, 不影响游戏 UI.

        private readonly VisualElement _window;

        private readonly VisualElement _desktop;

        private readonly VisualElement _secondaryPages; // Options 的有界宿主, 页面内部仅使用单层纵向滚动.

        private readonly ToolkitConsoleRenderer _renderer;

        private readonly ToolkitConsoleInput _input;

        private readonly ToolkitCheatPanel _cheats;

        private readonly ToolkitCheatLogPreview _cheatLogs;

        private readonly ToolkitOptionsPanel _options;

        private readonly ToolkitAppearanceController _appearance;

        private readonly ToolkitTextOpacity _textOpacity; // 文字的最终层级透明度最低保留六成.

        private ToolkitMacroPanel _macros; // 保留会话草稿的宏编辑器.

        private readonly ToolkitWindowController _windowController;

        private readonly ToolkitWindowSettings _windowSettings;

        private readonly ToolkitHelpWindow _help; // 主工具共享的模态分页帮助.

        private readonly ToolkitTextDisplay _text = new();

        private ConsoleController<LogType> _console;

        private readonly Dictionary<string, VisualElement> _pages = new();

        private readonly Dictionary<string, Button> _tabs = new();

        private readonly List<Action> _unbind = new();

        private string _currentTab;

        private bool _isOpen;

        private bool _disposed; // 防止重复释放临时对象.

        private float _appliedOpacity = float.NaN; // 无关设置变化不重复写入透明度.

        private int _fontRevision = -1;

        public ToolkitView(GameObject gameObject, UIDocument document, PanelSettings panelSettings)
        {
            _gameObject = gameObject;
            _panelSettings = panelSettings;
            try
            {
                var root = document.rootVisualElement;

                _navigationBlocker = new ToolkitNavigationBlocker(root);
                root.AddToClassList("dev-document");
                root.pickingMode = PickingMode.Ignore;
                _desktop = Require<VisualElement>(root, "toolkitDesktop");
                _window = Require<VisualElement>(root, "toolkitWindow");
                _secondaryPages = Require<VisualElement>(root, "secondaryPages");
                _window.AddToClassList("is-hidden");
                _input = new ToolkitConsoleInput();
                _renderer = new ToolkitConsoleRenderer(root);
                _cheats = new ToolkitCheatPanel(root, _renderer);
                _windowSettings = new ToolkitWindowSettings();
                _appearance = new ToolkitAppearanceController(root, panelSettings);
                _options = new ToolkitOptionsPanel(Require<VisualElement>(root, "settingsContent"), _windowSettings, _appearance);
                _cheatLogs = new ToolkitCheatLogPreview(root, _renderer, _windowSettings);
                _windowController = new ToolkitWindowController(root, _renderer, _windowSettings);
                _windowSettings.e_onChanged += ApplyPanelOpacity;
                ApplyPanelOpacity();
                _help = new ToolkitHelpWindow(root, () => _cheats.HelpTopic);
                _help.e_onVisibilityChanged += OnHelpVisibilityChanged;
                _windowController.e_onMinimizedChanged += OnMinimizedChanged;
                _windowController.e_onFloatingSettingsChanged += OnFloatingSettingsChanged;
                foreach (var key in new[] { "console", "cheat", "macro", "option" })
                {
                    _pages.Add(key, Require<VisualElement>(root, key + "Page"));
                    var button = Require<Button>(root, key + "Tab");

                    _tabs.Add(key, button);
                    _text.Track(button);
                    Action click = () => SelectTab(key);

                    button.clicked += click;
                    _unbind.Add(() => button.clicked -= click);
                }

                var clear = Require<Button>(root, "clearOutput");

                clear.clicked += ClearOutput;
                _unbind.Add(() => clear.clicked -= ClearOutput);
                var close = Require<Button>(root, "closeWindow");
                Action closeWindow = () => IsOpen = false;

                close.clicked += closeWindow;
                _unbind.Add(() => close.clicked -= closeWindow);
                _text.Track(Require<Label>(root, "shortcutHint"));
                _text.Track(Require<Label>(root, "windowTitle"));
                Require<Label>(root, "toolkitVersion").text = Config.version;
                InitializeLocalization(root);
                SelectTab(_pages.ContainsKey(Config.MainTagger) ? Config.MainTagger : Config.defaultMainTagger);
                _textOpacity = new ToolkitTextOpacity(_desktop);
            }
            catch (Exception failure)
            {
                try
                {
                    Dispose();
                }
                catch (Exception cleanup)
                {
                    throw new AggregateException(failure, cleanup);
                }

                throw;
            }
        }

        public static T Require<T>(VisualElement root, string name)
            where T : VisualElement
        {
            return root.Q<T>(name) ?? throw new InvalidOperationException(ToolkitLocalization.Format("ui.View.Require", ("p0", (object)(name))));
        }

        internal static VisualElement GetOverlayRoot(VisualElement source)
        {
            return source.panel?.visualTree.Q<VisualElement>("toolkitDesktop") ?? throw new InvalidOperationException(ToolkitLocalization.Get("ui.View.GetOverlayRoot"));
        }

        public void BindConsole(ConsoleController<LogType> console, MacroSystem macros, Func<string, Sprite> iconLoader = null)
        {
            _console = console;
            _cheats.BindConsole(console, macros, iconLoader);
            _macroSystem = macros;
            _windowSettings.Bind();
        }

        public void RestoreWindow()
        {
            if (_disposed || !_isOpen) return;

            _help.Close();
            _windowController.SetMinimized(false);
        }

        /// <summary> 缩小为浮标, 保留页签, 草稿, 位置和尺寸. </summary>
        public void MinimizeWindow()
        {
            if (_disposed || !_isOpen) return;

            _windowController.SetMinimized(true);
        }

        /// <summary> 批量刷新日志, 仅打开的控制台处理快捷键. </summary>
        public void Tick()
        {
            if (_disposed) return;

            _fontRevision = ToolkitFactory.RefreshFontResources(_gameObject.GetComponent<UIDocument>().rootVisualElement, _fontRevision);
            _appearance.Tick();
            _windowSettings.Tick();
            _help.Tick();
            _windowController.Tick(_help.BlocksInputThisFrame);
            _input.Capture(_renderer.IsVisible && !_help.BlocksInputThisFrame, _renderer.IsInputFieldFocus, _renderer.IsLogDetailFocused);
            _renderer.Tick();
            _cheats.Tick();
            _macros?.Tick();
            _options.Tick();
            if (_isOpen)
            {
                _cheatLogs.Tick();
                _text.Tick();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            _isOpen = false;
            var failures = new List<Exception>();

            ReleaseViewResource(() =>
            {
                if (_renderer != null)
                {
                    _renderer.WindowVisible = false;
                }
            }, failures);
            ReleaseViewResource(() => _renderer?.QuitFocus(), failures);
            ReleaseViewResource(SaveOptions, failures);
            _window?.AddToClassList("is-hidden");
            if (_windowController != null)
            {
                _windowController.e_onMinimizedChanged -= OnMinimizedChanged;
                _windowController.e_onFloatingSettingsChanged -= OnFloatingSettingsChanged;
            }

            if (_help != null)
            {
                _help.e_onVisibilityChanged -= OnHelpVisibilityChanged;
            }

            if (_windowSettings != null)
            {
                _windowSettings.e_onChanged -= ApplyPanelOpacity;
            }

            foreach (var release in new Action[] { () => _help?.Dispose(), () => _windowController?.Dispose(), () => _cheatLogs?.Dispose(), () => _windowSettings?.Dispose(), () => _renderer?.Dispose(), () => _cheats?.Dispose(), () => _macros?.Dispose(), () => _options?.Dispose(), () => _appearance?.Dispose(), () => _textOpacity?.Dispose(), _text.Dispose })
            {
                ReleaseViewResource(release, failures);
            }

            foreach (var unbind in _unbind)
            {
                ReleaseViewResource(unbind, failures);
            }

            _unbind.Clear();
            ReleaseViewResource(() => _navigationBlocker?.Dispose(), failures);
            _input?.Reset();
            ReleaseViewResource(() =>
            {
                if (_gameObject != null)
                {
                    var root = _gameObject.GetComponent<UIDocument>().rootVisualElement;

                    ToolkitMixedText.Release(root);
                }
            }, failures);
            ReleaseViewResource(() =>
            {
                if (_gameObject == null) return;

                _gameObject.SetActive(false);
                UnityEngine.Object.Destroy(_gameObject);
            }, failures);
            ReleaseViewResource(() =>
            {
                if (_panelSettings != null)
                {
                    UnityEngine.Object.Destroy(_panelSettings);
                }
            }, failures);
            if (failures.Count > 0)
            {
                throw new AggregateException("GameDevTools view cleanup failed.", failures);
            }
        }

        // 一个控件的清理异常不能跳过其余事件, 调度项和窗口资源的释放.
        private static void ReleaseViewResource(Action release, List<Exception> failures)
        {
            try
            {
                release();
            }
            catch (Exception exception)
            {
                failures.Add(exception);
            }
        }

        private void ApplyPanelOpacity()
        {
            var opacity = _windowSettings.PanelOpacity;

            if (_appliedOpacity == opacity) return;

            _appliedOpacity = opacity;
            ToolkitTextOpacity.SetOpacity(opacity);
        }

        private void SelectTab(string key)
        {
            if (_currentTab == key)
            {
                if (key == "macro" && _macros == null)
                {
                    _macroAttempted = false;
                    UpdatePageLoading();
                }

                return;
            }

            _help.Close();
            _renderer.QuitFocus();
            if (_currentTab == "option" && key != _currentTab)
            {
                SaveOptions();
            }

            _renderer.IsAlternativeOptionsActive = false;
            if (key != "macro")
            {
                _macroAttempted = false;
            }

            _currentTab = key;
            foreach (var entry in _pages)
            {
                entry.Value.EnableInClassList("is-hidden", entry.Key != key);
                _tabs[entry.Key].EnableInClassList("selected", entry.Key == key);
            }

            _renderer.ConsoleSelected = key == "console";
            _secondaryPages.EnableInClassList("is-hidden", key != "option" && key != "macro");
            Config.MainTagger = key;
            UpdatePageLoading();
            if (_isOpen && !_windowController.IsMinimized && key == "console")
            {
                _renderer.ActivateInput();
            }
        }

        private void OnMinimizedChanged(bool minimized)
        {
            _help.Close();
            _renderer.QuitFocus();
            SaveOptions();
            _renderer.IsAlternativeOptionsActive = false;
            _renderer.WindowVisible = _isOpen && !minimized;
            UpdatePageLoading();
            _input.Reset();
            if (!minimized && _currentTab == "console")
            {
                _renderer.ActivateInput();
            }
        }

        private void OnFloatingSettingsChanged(bool open)
        {
            if (!open)
            {
                _help.Close();
                _windowSettings?.Save();
            }

            _renderer.QuitFocus();
            _input.Reset();
        }

        // 帮助期间继续刷新日志和宏状态, 但不收集底层输入或执行延迟聚焦.
        private void OnHelpVisibilityChanged(bool open)
        {
            _renderer.InputSuppressed = open;
            if (open)
            {
                _options.CloseSelectionMenus();
                _renderer.QuitFocus();
            }

            _input.Reset();
        }

        private void UpdatePageLoading()
        {
            var visible = _isOpen && !_windowController.IsMinimized;

            _cheats.SetVisible(visible && _currentTab == "cheat");
            _cheatLogs.SetVisible(visible && _currentTab == "cheat");
            _options.SetVisible(visible && _currentTab == "option");
            var macroVisible = visible && _currentTab == "macro";

            if (macroVisible && _macros == null && !_macroAttempted)
            {
                InitializeMacroPanel();
            }

            _macros?.SetVisible(macroVisible);
        }

        // 只在实际编辑过配置后写入, 避免初始化失败影响配置.
        private void SaveOptions()
        {
            _options?.Save();
            _windowSettings?.Save();
        }

        // 清空必须经过总控的代理, 同时清除 UI 创建前缓存的日志.
        private void ClearOutput() => _console?.ClearOutputPanel();
    }
}
