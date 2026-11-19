using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 管理图标外的日志预览和设置弹窗, 不依赖 Console 页签的显示状态. </summary>
    public sealed class ToolkitFloatingPreview : IDisposable
    {
        private const float PANEL_WIDTH = 320f; // 预览与弹窗的首选宽度, 面板单位.

        private const float PANEL_GAP = 8f;

        private static readonly CustomStyleProperty<Color> FLOATING_BACKGROUND = new("--floating-log-background");

        public event Action<bool> e_onSettingsVisibilityChanged;

        public bool IsSettingsOpen { get; private set; }

        private readonly VisualElement _icon;

        private readonly VisualElement _preview;

        private readonly VisualElement _popup;

        private readonly Button _settingsButton;

        private readonly Button _closeButton;

        private readonly VisualElement[] _rows = new VisualElement[Config.MAX_FLOATING_LOG_COUNT];

        private readonly Label[] _labels = new Label[Config.MAX_FLOATING_LOG_COUNT];

        private readonly ToolkitFloatingSettingsControls _controls;

        private readonly ToolkitFloatingPerformance _performance;

        private readonly ToolkitWindowSettings _settings;

        private readonly ToolkitConsoleRenderer _renderer;

        private readonly ToolkitTextDisplay _text = new();

        private Rect _iconBounds;

        private Vector2 _viewport;

        private int _dockEdge;

        private int _logVersion = -1;

        private bool _visible; // 仅最小化且工具打开时显示.

        private Color _backgroundColor = new(.13f, .15f, .19f, 1f);

        private int _paletteVersion = -1;

        private (bool showLogs, int count, float background, float text)? _shownSettings;

        public ToolkitFloatingPreview(VisualElement root, ToolkitConsoleRenderer renderer, ToolkitWindowSettings settings)
        {
            _renderer = renderer;
            _settings = settings;
            _icon = ToolkitView.Require<VisualElement>(root, "floatingTool");
            _preview = ToolkitView.Require<VisualElement>(root, "floatingLogPreview");
            _popup = ToolkitView.Require<VisualElement>(root, "floatingSettings");
            _settingsButton = ToolkitView.Require<Button>(_icon, "floatingSettingsButton");
            _closeButton = ToolkitView.Require<Button>(_popup, "closeFloatingSettings");
            var lines = ToolkitView.Require<VisualElement>(_preview, "floatingPreviewLines");
            var template = ToolkitFactory.RequireTemplate("ToolkitFloatingLog");

            for (var i = 0; i < _rows.Length; i++)
            {
                _rows[i] = ToolkitFactory.InstantiateTemplate(template);
                _rows[i].pickingMode = PickingMode.Ignore;
                _labels[i] = ToolkitView.Require<Label>(_rows[i], "floatingLogText");
                _text.Track(_labels[i]);
                lines.Add(_rows[i]);
            }

            _controls = new ToolkitFloatingSettingsControls(_popup, settings, "floatingPopup");
            _performance = new ToolkitFloatingPerformance(root, settings);
            _settingsButton.clicked += ToggleSettings;
            _closeButton.clicked += CloseSettings;
            _icon.RegisterCallback<PointerUpEvent>(OnIconPointerUp);
            _popup.RegisterCallback<GeometryChangedEvent>(OnPopupGeometryChanged);
            _settings.e_onChanged += OnSettingsChanged;
            _preview.RegisterCallback<CustomStyleResolvedEvent>(OnPaletteResolved);
            _preview.RegisterCallback<GeometryChangedEvent>(OnPreviewGeometryChanged);
            ReadThemePalette();
            OnSettingsChanged();
        }

        public void SetVisible(bool visible)
        {
            if (_visible == visible) return;

            _visible = visible;
            _performance.SetVisible(visible);
            if (!visible)
            {
                SetSettingsOpen(false);
            }

            _logVersion = -1;
            UpdateVisibility();
            if (!visible) return;

            RefreshLogs();
            ApplyLayout();
        }

        public void UpdateGeometry(Rect iconBounds, Vector2 viewport, int dockEdge)
        {
            if (_iconBounds == iconBounds && _viewport == viewport && _dockEdge == dockEdge) return;

            _iconBounds = iconBounds;
            _viewport = viewport;
            _dockEdge = dockEdge;
            ApplyLayout();
        }

        /// <summary> 只刷新固定文案, 浮窗日志仍保留原始正文. </summary>
        internal void RefreshLanguage()
        {
            _controls.RefreshLanguage();
            _logVersion = -1;
            RefreshLogs();
        }

        public void Tick(bool keyboardBlocked = false)
        {
            if (_paletteVersion != ToolkitFactory.AppearanceVersion)
            {
                _paletteVersion = ToolkitFactory.AppearanceVersion;
                ReadThemePalette();
            }

            if (!_visible) return;

            _performance.Tick(Time.realtimeSinceStartupAsDouble, Time.frameCount, Time.unscaledDeltaTime, Application.isFocused);
            if (RefreshLogs()) ApplyLayout();
            _text.Tick();
            if (IsSettingsOpen)
            {
                _controls.Tick();
            }

            if (!keyboardBlocked && IsSettingsOpen && UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                SetSettingsOpen(false);
            }
        }

        /// <summary> 解除配置和按钮事件, 不保留浮标对旧视图的引用. </summary>
        public void Dispose()
        {
            SetVisible(false);
            _settings.e_onChanged -= OnSettingsChanged;
            _preview.UnregisterCallback<CustomStyleResolvedEvent>(OnPaletteResolved);
            _preview.UnregisterCallback<GeometryChangedEvent>(OnPreviewGeometryChanged);
            _settingsButton.clicked -= ToggleSettings;
            _closeButton.clicked -= CloseSettings;
            _icon.UnregisterCallback<PointerUpEvent>(OnIconPointerUp);
            _popup.UnregisterCallback<GeometryChangedEvent>(OnPopupGeometryChanged);
            _controls.Dispose();
            _performance.Dispose();
            _text.Dispose();
            e_onSettingsVisibilityChanged = null;
        }

        internal static Rect CalculateBounds(Rect icon, Vector2 viewport, Vector2 desiredSize, int dockEdge)
        {
            var size = Vector2.Min(desiredSize, Vector2.Max(Vector2.zero, viewport));
            var alignRight = dockEdge == 2 || dockEdge != 1 && icon.center.x > viewport.x * .5f;
            var x = alignRight
                ? icon.xMax - size.x
                : icon.xMin;
            var y = icon.yMax + PANEL_GAP;
            var below = viewport.y - y;
            var above = icon.yMin - PANEL_GAP;

            if (below < size.y)
            {
                if (above >= size.y)
                {
                    y = icon.yMin - PANEL_GAP - size.y;
                }
                else
                {
                    var inwardX = alignRight
                        ? icon.xMin - PANEL_GAP - size.x
                        : icon.xMax + PANEL_GAP;

                    if (inwardX >= 0 && inwardX + size.x <= viewport.x)
                    {
                        x = inwardX;
                        y = icon.yMin;
                    }
                    else if (above > below)
                    {
                        y = icon.yMin - PANEL_GAP - size.y;
                    }
                }
            }

            var position = new Vector2(Mathf.Clamp(x, 0, viewport.x - size.x), Mathf.Clamp(y, 0, viewport.y - size.y));

            return new Rect(position, size);
        }

        internal static float CalculatePreviewWidth(Vector2 viewport, bool reserveSettings)
        {
            var gap = Mathf.Min(PANEL_GAP, viewport.x * .05f);

            return Mathf.Min(PANEL_WIDTH, Mathf.Max(0, reserveSettings ? (viewport.x - gap) * .5f : viewport.x));
        }

        internal static Rect CalculateSettingsBounds(Rect logs, Vector2 viewport, Vector2 naturalSize)
        {
            var gap = Mathf.Min(PANEL_GAP, viewport.x * .05f);
            var leftSpace = Mathf.Max(0, logs.xMin - gap);
            var rightSpace = Mathf.Max(0, viewport.x - logs.xMax - gap);
            var onRight = rightSpace >= naturalSize.x || leftSpace < naturalSize.x && rightSpace >= leftSpace;
            var space = onRight
                ? rightSpace
                : leftSpace;
            var scale = Mathf.Min(1f, Mathf.Min(space / naturalSize.x, viewport.y / naturalSize.y));
            var size = naturalSize * scale;
            var x = onRight
                ? logs.xMax + gap
                : logs.xMin - gap - size.x;
            var position = new Vector2(Mathf.Clamp(x, 0, viewport.x - size.x), Mathf.Clamp(logs.yMin, 0, viewport.y - size.y));

            return new Rect(position, size);
        }

        // 图标按钮和右键都能打开设置, 不触发图标的还原操作.
        private void ToggleSettings() => SetSettingsOpen(!IsSettingsOpen);

        private void CloseSettings() => SetSettingsOpen(false);

        private void OnIconPointerUp(PointerUpEvent evt)
        {
            if (evt.button != 1) return;

            ToggleSettings();
            evt.StopPropagation();
        }

        // 设置窗口使用独立输入状态, 避免编辑数字时同时操作游戏.
        private void SetSettingsOpen(bool open)
        {
            if (IsSettingsOpen == open || open && !_visible) return;

            IsSettingsOpen = open;
            if (!open)
            {
                _popup.panel?.focusController.focusedElement?.Blur();
            }

            UpdateVisibility();
            ApplyLayout();
            e_onSettingsVisibilityChanged?.Invoke(open);
        }

        private void OnSettingsChanged()
        {
            var snapshot = (_settings.ShowLatestLogs, _settings.LogLineCount, _settings.BackgroundOpacity, _settings.TextOpacity);
            var metricsChanged = _performance.SetMetrics(_settings.FrameMetrics);

            if (_shownSettings.HasValue && _shownSettings.Value == snapshot)
            {
                if (metricsChanged) ApplyLayout();
                return;
            }

            _shownSettings = snapshot;
            ApplyBackground();
            foreach (var label in _labels)
            {
                label.style.opacity = _settings.TextOpacity;
            }

            _logVersion = -1;
            UpdateVisibility();
            RefreshLogs();
            ApplyLayout();
        }

        private void OnPaletteResolved(CustomStyleResolvedEvent evt) => ReadThemePalette();

        private void ReadThemePalette()
        {
            if (_preview.customStyle.TryGetValue(FLOATING_BACKGROUND, out var color))
            {
                _backgroundColor = color;
            }

            ApplyBackground();
            ApplyLayout();
        }

        private void ApplyBackground()
        {
            var color = _backgroundColor;

            color.a *= _settings.BackgroundOpacity;
            _preview.style.backgroundColor = color;
        }

        private bool RefreshLogs()
        {
            if (!_visible || !_settings.HasLogPreview || _logVersion == _renderer.LogVersion) return false;

            _logVersion = _renderer.LogVersion;
            var logs = _renderer.LatestLines;

            var logCount = Mathf.Min(logs.Count, _settings.LogLineCount);
            var start = logs.Count - logCount;
            var rowCount = Mathf.Max(1, logCount);

            for (var i = 0; i < _labels.Length; i++)
            {
                _rows[i].EnableInClassList("is-hidden", i >= rowCount);
                var text = i < logCount ? logs[start + i] : i == 0 ? ToolkitLocalization.Get("ui.FloatingPreview.RefreshLogs") : string.Empty;

                _text.SetText(_labels[i], text);
            }

            return true;
        }

        // 设置期间保留日志预览, 让滑动条修改即时反映到背景和文字.
        private void UpdateVisibility()
        {
            _preview.EnableInClassList("is-hidden", !_visible || !_settings.HasLogPreview);
            _popup.EnableInClassList("is-hidden", !_visible || !IsSettingsOpen);
        }

        // 高度由原生布局按实际字体与日志行撑开, 定位不再写回固定高度.
        private void ApplyLayout()
        {
            if (_viewport.x <= 0 || _viewport.y <= 0) return;

            var anchor = _performance.Layout(_iconBounds, _viewport, _dockEdge);
            if (!_settings.HasLogPreview)
            {
                ApplyPopupLayout(_iconBounds);
                return;
            }

            var width = CalculatePreviewWidth(_viewport, IsSettingsOpen);
            _preview.style.width = width;
            _preview.style.maxHeight = _viewport.y;
            var height = _preview.layout.height;

            if (float.IsNaN(height) || float.IsInfinity(height) || height < 0)
            {
                height = 0;
            }

            var logs = CalculateBounds(anchor, _viewport, new Vector2(width, height), _dockEdge);

            _preview.style.translate = new Translate(logs.x, logs.y, 0);
            ApplyPopupLayout(logs);
        }

        private void OnPreviewGeometryChanged(GeometryChangedEvent evt)
        {
            if (evt.newRect.size != evt.oldRect.size)
            {
                ApplyLayout();
            }
        }

        private void OnPopupGeometryChanged(GeometryChangedEvent evt)
        {
            if (evt.newRect.size != evt.oldRect.size)
            {
                ApplyLayout();
            }
        }

        // 设置面板始终横向贴近日志, 空间不足时等比缩小, 不覆盖日志区域.
        private void ApplyPopupLayout(Rect logs)
        {
            if (_viewport.x <= 0 || _viewport.y <= 0) return;

            var width = Mathf.Min(PANEL_WIDTH, _viewport.x);

            _popup.style.width = width;
            var height = _popup.layout.height;

            if (float.IsNaN(height) || height <= 0) return;

            var bounds = CalculateSettingsBounds(logs, _viewport, new Vector2(width, height));
            var scale = bounds.width / width;

            _popup.style.scale = new Scale(new Vector3(scale, scale, 1f));
            _popup.style.translate = new Translate(bounds.x, bounds.y, 0);
        }
    }
}
