using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 带树形导航的共用分页帮助窗口, 隔离底层交互并保留原区域焦点. </summary>
    internal sealed partial class ToolkitHelpWindow : IDisposable
    {
        internal event Action<bool> e_onVisibilityChanged;

        internal bool IsOpen { get; private set; }

        internal bool BlocksInputThisFrame => IsOpen || _closedFrame == Time.frameCount;

        private readonly VisualElement _root;

        private readonly VisualElement _host;

        private readonly VisualElement _backdrop;

        private readonly VisualElement _dialog;

        private readonly VisualElement _viewport;

        private readonly Label _areaTitle;

        private readonly Label _featureTitle;

        private readonly Label _body;

        private readonly ToolkitPager _pager;

        private readonly VisualElement _pagerRoot;

        private readonly Button _close;

        private readonly ToolkitHelpNavigation _navigation;

        private readonly ToolkitHelpPlacement _placement;

        private readonly List<Action> _unbind = new();

        private readonly Dictionary<VisualElement, bool> _background = new(); // 保留底层根节点原有的启用状态.

        private readonly List<ToolkitHelpFeature> _features = new();

        private List<ToolkitHelpPage> _pages = new();

        private FocusSnapshot _returnFocus;

        private int _pageIndex;

        private int _closedFrame = -1; // 关闭帮助的帧号, 防止 Esc 继续关闭底层设置.

        private int _focusVersion; // 避免排队的旧焦点恢复覆盖新弹窗.

        private bool _disposed; // 防止销毁后执行延迟回调.

        private ToolkitHelpTopic _topic;

        private int _requestedFeature = -1;

        private int SelectedFeature => _pages.Count > 0 ? _pages[_pageIndex].FeatureIndex : 0;

        internal ToolkitHelpWindow(VisualElement root, Func<ToolkitHelpTopic> cheatTopic)
        {
            _root = root;
            _host = ToolkitView.Require<VisualElement>(root, "helpWindowHost");
            _backdrop = ToolkitView.Require<VisualElement>(_host, "helpBackdrop");
            _dialog = ToolkitView.Require<VisualElement>(_host, "helpDialog");
            _viewport = ToolkitView.Require<VisualElement>(_host, "helpBodyViewport");
            _areaTitle = ToolkitView.Require<Label>(_host, "helpAreaTitle");
            _featureTitle = ToolkitView.Require<Label>(_host, "helpFeatureTitle");
            _body = ToolkitView.Require<Label>(_host, "helpBody");
            _pagerRoot = ToolkitView.Require<VisualElement>(_host, "helpPager");
            _pager = new ToolkitPager(_pagerRoot, ChangePage);
            _close = ToolkitView.Require<Button>(_host, "closeHelp");
            _navigation = new ToolkitHelpNavigation(ToolkitView.Require<ScrollView>(_host, "helpNavigation"), SelectTopic);
            _placement = new ToolkitHelpPlacement(_backdrop, _dialog, ToolkitView.Require<VisualElement>(_host, "helpHeader"));
            _body.enableRichText = false;
            _body.parseEscapeSequences = false;
            _body.selection.isSelectable = true;
            _close.clicked += CloseFromButton;
            _viewport.RegisterCallback<GeometryChangedEvent>(Geometry);
            _backdrop.RegisterCallback<PointerDownEvent>(Outside, TrickleDown.TrickleDown);
            _root.RegisterCallback<KeyDownEvent>(KeyDown, TrickleDown.TrickleDown);
            _root.RegisterCallback<FocusInEvent>(FocusIn, TrickleDown.TrickleDown);
            Bind("helpWindow", () => ToolkitHelpTopic.Window);
            Bind("helpConsole", () => ToolkitHelpTopic.Console);
            Bind("helpStack", () => ToolkitHelpTopic.Stack);
            Bind("helpCheatCommands", () => ToolkitHelpTopic.CheatCommands);
            Bind("helpCheatDetails", cheatTopic);
            Bind("helpMacroLibrary", () => ToolkitHelpTopic.MacroLibrary);
            Bind("helpMacroEditor", () => ToolkitHelpTopic.MacroEditor);
            Bind("helpMacroSyntax", () => ToolkitHelpTopic.MacroSyntax);
            Bind("helpConsoleSettings", () => ToolkitHelpTopic.ConsoleSettings);
            Bind("helpInterfaceSettings", () => ToolkitHelpTopic.InterfaceSettings);
            Bind("helpToolSettings", () => ToolkitHelpTopic.ToolSettings);
            Bind("helpCheatSettings", () => ToolkitHelpTopic.CheatSettings);
            Bind("helpFloatingOptions", () => ToolkitHelpTopic.FloatingSettings);
            Bind("helpFloatingSettings", () => ToolkitHelpTopic.FloatingSettings);
        }

        /// <summary> 生命周期关闭时不恢复已隐藏页面的焦点. </summary>
        internal void Close(bool restoreFocus = false)
        {
            if (!IsOpen) return;

            IsOpen = false;
            _closedFrame = Time.frameCount;
            var version = ++_focusVersion;
            var focus = _returnFocus;

            _returnFocus = null;
            _host.AddToClassList("is-hidden");
            _placement.Cancel();
            CancelPagination();
            foreach (var pair in _background)
            {
                pair.Key.SetEnabled(pair.Value);
            }

            _background.Clear();
            e_onVisibilityChanged?.Invoke(false);
            if (restoreFocus && focus != null)
            {
                _root.schedule.Execute(() =>
                {
                    if (!_disposed && !IsOpen && _focusVersion == version)
                    {
                        focus.Restore();
                    }
                });
            }
        }

        /// <summary> 保留选中特性和窗口位置, 新语言重新分页到该特性第一页. </summary>
        internal void RefreshLanguage()
        {
            _navigation.RefreshLanguage();
            if (!IsOpen) return;

            var feature = _requestedFeature >= 0 ? _requestedFeature : SelectedFeature;

            CancelPagination();
            SelectTopic(_topic, feature);
        }

        public void Dispose()
        {
            _disposed = true;
            Close();
            foreach (var unbind in _unbind)
            {
                unbind();
            }

            _unbind.Clear();
            _close.clicked -= CloseFromButton;
            _pager.Dispose();
            _navigation.Dispose();
            _placement.Dispose();
            _viewport.UnregisterCallback<GeometryChangedEvent>(Geometry);
            _backdrop.UnregisterCallback<PointerDownEvent>(Outside, TrickleDown.TrickleDown);
            _root.UnregisterCallback<KeyDownEvent>(KeyDown, TrickleDown.TrickleDown);
            _root.UnregisterCallback<FocusInEvent>(FocusIn, TrickleDown.TrickleDown);
            e_onVisibilityChanged = null;
        }

        // 在按钮取得焦点之前保存输入框的光标, 点击后再读取动态主题.
        private void Bind(string name, Func<ToolkitHelpTopic> topic)
        {
            var button = ToolkitView.Require<Button>(_root, name);
            FocusSnapshot focus = null;
            EventCallback<PointerDownEvent> down = evt =>
            {
                if (evt.button == 0)
                {
                    focus = new FocusSnapshot(_root);
                }
            };
            EventCallback<PointerCancelEvent> cancel = evt => focus = null;
            EventCallback<KeyDownEvent> key = evt => focus = new FocusSnapshot(_root);
            Action click = () =>
            {
                Open(topic(), focus ?? new FocusSnapshot(_root));
                focus = null;
            };

            button.RegisterCallback(down, TrickleDown.TrickleDown);
            button.RegisterCallback(cancel);
            button.RegisterCallback(key, TrickleDown.TrickleDown);
            button.clicked += click;
            _unbind.Add(() =>
            {
                button.UnregisterCallback(down, TrickleDown.TrickleDown);
                button.UnregisterCallback(cancel);
                button.UnregisterCallback(key, TrickleDown.TrickleDown);
                button.clicked -= click;
            });
        }

        private void Open(ToolkitHelpTopic topic, FocusSnapshot focus)
        {
            if (_disposed) return;

            if (!IsOpen)
            {
                _returnFocus = focus;
                _focusVersion++;
                IsOpen = true;
                foreach (var child in _host.parent.Children())
                {
                    if (child == _host)
                    {
                        continue;
                    }

                    _background[child] = child.enabledSelf;
                    child.SetEnabled(false);
                }

                e_onVisibilityChanged?.Invoke(true);
            }

            _host.RemoveFromClassList("is-hidden");
            _host.BringToFront();
            _placement.Reset();
            CancelPagination();
            _pages = new List<ToolkitHelpPage>();
            SelectTopic(topic, 0);
            _close.Focus();
        }

        private void SelectTopic(ToolkitHelpTopic topic, int featureIndex)
        {
            if (_topic != topic || _pagination != null)
            {
                CancelPagination();
            }

            _topic = topic;
            _requestedFeature = featureIndex;
            _navigation.Select(topic, featureIndex);
            if (!_pageKey.HasValue || _pages.Count == 0)
            {
                var section = ToolkitHelpContents.Get(topic);

                _pages = new List<ToolkitHelpPage>();
                _pageIndex = 0;
                _areaTitle.text = section.Title;
                _featureTitle.text = section.Features.Count > 0 ? section.Features[featureIndex].Title : section.Title;
                ToolkitMixedText.SetText(_body, string.Empty);
                SetLoading(true);
            }
        }

        private void ShowPage()
        {
            if (_pages.Count == 0)
            {
                ToolkitMixedText.SetText(_body, ToolkitLocalization.Get("ui.HelpWindow.ShowPage"));
                return;
            }

            var page = _pages[_pageIndex];
            var title = _features[page.FeatureIndex].Title;

            _featureTitle.text = page.PartCount > 1
                ? $"{title} {page.PartIndex}"
                : title;
            ShowBody(page);
            _body.selection.cursorIndex = 0;
            _body.selection.selectIndex = 0;
            _pager.SetPage(_pageIndex, _pages.Count);
            _navigation.Select(_topic, page.FeatureIndex);
            if (!(_root.panel?.focusController.focusedElement is VisualElement focus) || !focus.enabledInHierarchy)
            {
                _dialog.Focus();
            }
        }

        private void ChangePage(int page)
        {
            if (!IsOpen || _pagerRoot.ClassListContains("help-pager-loading") || page < 0 || page >= _pages.Count) return;

            _pageIndex = page;
            ShowPage();
        }

        private void Previous() => _pager.Move(-1);

        private void Next() => _pager.Move(1);

        private void CloseFromButton() => Close(true);

        private void Geometry(GeometryChangedEvent evt)
        {
            if (evt.oldRect.size != evt.newRect.size)
            {
                _layoutFrame = Time.frameCount + 1;
            }
        }

        private void Outside(PointerDownEvent evt)
        {
            if (!IsOpen || evt.target != _backdrop || evt.button != 0) return;

            Close(true);
            evt.StopImmediatePropagation();
        }

        private void KeyDown(KeyDownEvent evt)
        {
            if (!IsOpen) return;

            if (evt.keyCode == KeyCode.Escape)
            {
                Close(true);
                ConsumeKey(evt);
                return;
            }

            if (evt.keyCode == KeyCode.Tab)
            {
                var targets = _dialog.Query<VisualElement>()
                    .ToList()
                    .FindAll(IsFocusTarget);
                var current = targets.IndexOf(_root.panel?.focusController.focusedElement as VisualElement);

                if (current < 0)
                {
                    current = evt.shiftKey ? 0 : -1;
                }

                if (targets.Count > 0)
                {
                    current = (current + (evt.shiftKey ? -1 : 1) + targets.Count) % targets.Count;
                    targets[current].Focus();
                }

                ConsumeKey(evt);
                return;
            }

            if (evt.keyCode != KeyCode.LeftArrow && evt.keyCode != KeyCode.RightArrow) return;

            if (evt.target is VisualElement target && _navigation.Contains(target)) return;

            if (evt.target is VisualElement input && _pager.ContainsInput(input)) return;

            var selecting = _root.panel?.focusController.focusedElement == _body && (_body.selection.cursorIndex != _body.selection.selectIndex || evt.shiftKey || evt.ctrlKey || evt.commandKey);

            if (selecting) return;

            if (evt.keyCode == KeyCode.LeftArrow)
            {
                Previous();
            }
            else
            {
                Next();
            }

            ConsumeKey(evt);
        }

        private void ConsumeKey(KeyDownEvent evt)
        {
            evt.PreventDefault();
            evt.StopImmediatePropagation();
        }

        // 收起的树节点不参与焦点循环, 保留导航控件自身的键盘行为.
        private bool IsFocusTarget(VisualElement element)
        {
            if (element == _dialog || !element.focusable || !element.enabledInHierarchy) return false;

            for (var parent = element; parent != null && parent != _dialog; parent = parent.parent)
            {
                if (parent.resolvedStyle.display == DisplayStyle.None || parent.resolvedStyle.visibility == Visibility.Hidden) return false;
            }

            return true;
        }

        // 底层的延迟聚焦也不能抢占帮助窗口的键盘输入.
        private void FocusIn(FocusInEvent evt)
        {
            if (!IsOpen || evt.target is VisualElement element && _host.Contains(element)) return;

            _root.schedule.Execute(() =>
            {
                if (IsOpen && !_disposed)
                {
                    _close.Focus();
                }
            });
        }

        private sealed class FocusSnapshot
        {
            private readonly VisualElement _focus;

            private readonly TextField _field;

            private readonly int _cursor;

            private readonly int _selection;

            private readonly Vector2 _scroll;

            internal FocusSnapshot(VisualElement root)
            {
                _focus = root.panel?.focusController.focusedElement as VisualElement;
                for (var element = _focus; element != null; element = element.parent)
                {
                    if (element is TextField field)
                    {
                        _field = field;
                        break;
                    }
                }

                if (_field == null) return;

                _cursor = _field.cursorIndex;
                _selection = _field.selectIndex;
                _scroll = _field.Q<ScrollView>()?.scrollOffset ?? Vector2.zero;
            }

            /// <summary> 恢复原光标和选区, 避免原生首次聚焦全选改变编辑状态. </summary>
            internal void Restore()
            {
                if (_focus?.panel == null || !_focus.enabledInHierarchy || _focus.resolvedStyle.display == DisplayStyle.None) return;

                if (_field == null)
                {
                    _focus.Focus();
                    return;
                }

                _field.Focus();
                _field.SelectRange(_cursor, _selection);
                var scroll = _field.Q<ScrollView>();

                if (scroll != null)
                {
                    scroll.scrollOffset = _scroll;
                }
            }
        }
    }
}
