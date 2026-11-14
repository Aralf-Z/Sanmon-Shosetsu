using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 管理图标外的日志预览和设置弹窗, 不依赖 Console 页签的显示状态. </summary>
    public sealed class ToolkitFloatingPreview : IDisposable
    {
        private const float PANEL_WIDTH = 320f; // 预览与弹窗的首选宽度, 面板单位.
        private const float PANEL_GAP = 8f; // 图标与外围面板之间的间距.
        private static readonly CustomStyleProperty<Color> FLOATING_BACKGROUND = new("--floating-log-background"); // 每套风格提供自己的预览底色.

        /// <summary> 设置弹窗开关时通知宿主更新工具内部的焦点和设置. </summary>
        public event Action<bool> e_onSettingsVisibilityChanged;

        /// <summary> 当前是否正在编辑浮标设置. </summary>
        public bool IsSettingsOpen { get; private set; }

        private readonly VisualElement _icon; // 只有图标参与拖动和停靠.
        private readonly VisualElement _preview; // 图标同级的日志区域, 不接受指针事件.
        private readonly VisualElement _popup; // 图标同级的设置弹窗.
        private readonly Button _settingsButton; // 图标上的设置入口.
        private readonly Button _closeButton; // 设置弹窗关闭入口.
        private readonly VisualElement[] _rows = new VisualElement[Config.MAX_FLOATING_LOG_COUNT]; // 预先创建的日志模板, 不在输出时分配控件.
        private readonly Label[] _labels = new Label[Config.MAX_FLOATING_LOG_COUNT]; // 对应每条日志的文本.
        private readonly ToolkitFloatingSettingsControls _controls; // 与 Options 共用的配置绑定.
        private readonly ToolkitWindowSettings _settings; // 即时生效的浮标设置.
        private readonly ToolkitConsoleRenderer _renderer; // 独立的最新日志缓存.
        private readonly ToolkitTextDisplay _text = new(); // 颜色日志按实际可用宽度生成省略展示, 不修改缓存.
        private Rect _iconBounds; // 图标在文档中的位置和大小.
        private Vector2 _viewport; // 当前文档可用尺寸.
        private int _dockEdge; // 1 到 4 分别为左, 右, 上, 下, 0 为自由悬浮.
        private int _logVersion = -1; // 只在输出或设置改变时刷新文本.
        private int _rowCount = 1; // 实际可显示的日志条数, 无日志时显示占位文本.
        private bool _visible; // 仅最小化且工具打开时显示.
        private Color _backgroundColor = new(.13f, .15f, .19f, 1f); // 风格尚未解析前的后备色.
        private int _paletteVersion = -1; // 风格提交后读一次配色, 不逐帧遍历日志行.
        private (bool showLogs, int count, float background, float text)? _shownSettings; // 整体透明度变化不重新格式化独立日志预览.

        /// <summary> 连接可编辑的 UXML 模板, 预先准备最多十条日志. </summary>
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

        /// <summary> 最小化时强制刷新缓存, 隐藏或还原时同步关闭所有外围面板. </summary>
        public void SetVisible(bool visible)
        {
            if (_visible == visible)
                return;
            _visible = visible;
            if (!visible)
                SetSettingsOpen(false);
            _logVersion = -1;
            RefreshLogs();
            UpdateVisibility();
        }

        /// <summary> 图标移动和视口变化时更新相邻面板的位置. </summary>
        public void UpdateGeometry(Rect iconBounds, Vector2 viewport, int dockEdge)
        {
            if (_iconBounds == iconBounds && _viewport == viewport && _dockEdge == dockEdge)
                return;
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

        /// <summary> 最小化期间仍读取最新日志, Escape 关闭设置但不还原窗口. </summary>
        public void Tick(bool keyboardBlocked = false)
        {
            if (_paletteVersion != ToolkitFactory.AppearanceVersion)
            {
                _paletteVersion = ToolkitFactory.AppearanceVersion;
                ReadThemePalette();
            }
            if (!_visible)
                return;
            RefreshLogs();
            _text.Tick();
            if (IsSettingsOpen)
                _controls.Tick();
            if (!keyboardBlocked && IsSettingsOpen && UnityEngine.Input.GetKeyDown(KeyCode.Escape))
                SetSettingsOpen(false);
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
            _text.Dispose();
            e_onSettingsVisibilityChanged = null;
        }

        /// <summary> 优先在下方靠边对齐, 空间不足时向上翻转或放到图标内侧. </summary>
        internal static Rect CalculateBounds(Rect icon, Vector2 viewport, Vector2 desiredSize, int dockEdge)
        {
            var size = Vector2.Min(desiredSize, Vector2.Max(Vector2.zero, viewport));
            var alignRight = dockEdge == 2 || dockEdge != 1 && icon.center.x > viewport.x * .5f;
            var x = alignRight ? icon.xMax - size.x : icon.xMin;
            var y = icon.yMax + PANEL_GAP;
            var below = viewport.y - y;
            var above = icon.yMin - PANEL_GAP;
            if (below < size.y)
            {
                if (above >= size.y)
                    y = icon.yMin - PANEL_GAP - size.y;
                else
                {
                    var inwardX = alignRight ? icon.xMin - PANEL_GAP - size.x : icon.xMax + PANEL_GAP;
                    if (inwardX >= 0 && inwardX + size.x <= viewport.x)
                    {
                        x = inwardX;
                        y = icon.yMin;
                    }
                    else if (above > below)
                        y = icon.yMin - PANEL_GAP - size.y;
                }
            }
            var position = new Vector2(Mathf.Clamp(x, 0, viewport.x - size.x), Mathf.Clamp(y, 0, viewport.y - size.y));
            return new Rect(position, size);
        }

        /// <summary> 设置展开时为横向相邻布局预留空间, 窄视口内适当缩小日志背景宽度. </summary>
        internal static float CalculatePreviewWidth(Vector2 viewport, bool reserveSettings)
        {
            var gap = Mathf.Min(PANEL_GAP, viewport.x * .05f);
            return Mathf.Min(PANEL_WIDTH, Mathf.Max(0, reserveSettings ? (viewport.x - gap) * .5f : viewport.x));
        }

        /// <summary> 设置面板优先放在日志右侧, 不足时改到左侧, 并保持完整内容等比可见. </summary>
        internal static Rect CalculateSettingsBounds(Rect logs, Vector2 viewport, Vector2 naturalSize)
        {
            var gap = Mathf.Min(PANEL_GAP, viewport.x * .05f);
            var leftSpace = Mathf.Max(0, logs.xMin - gap);
            var rightSpace = Mathf.Max(0, viewport.x - logs.xMax - gap);
            var onRight = rightSpace >= naturalSize.x || leftSpace < naturalSize.x && rightSpace >= leftSpace;
            var space = onRight ? rightSpace : leftSpace;
            var scale = Mathf.Min(1f, Mathf.Min(space / naturalSize.x, viewport.y / naturalSize.y));
            var size = naturalSize * scale;
            var x = onRight ? logs.xMax + gap : logs.xMin - gap - size.x;
            var position = new Vector2(Mathf.Clamp(x, 0, viewport.x - size.x), Mathf.Clamp(logs.yMin, 0, viewport.y - size.y));
            return new Rect(position, size);
        }

        // 图标按钮和右键都能打开设置, 不触发图标的还原操作.
        private void ToggleSettings() => SetSettingsOpen(!IsSettingsOpen);

        // 提供无参数的按钮关闭回调.
        private void CloseSettings() => SetSettingsOpen(false);

        // 右键只弹出设置, 左键仍由原拖动操纵器处理.
        private void OnIconPointerUp(PointerUpEvent evt)
        {
            if (evt.button != 1)
                return;
            ToggleSettings();
            evt.StopPropagation();
        }

        // 设置窗口使用独立输入状态, 避免编辑数字时同时操作游戏.
        private void SetSettingsOpen(bool open)
        {
            if (IsSettingsOpen == open || open && !_visible)
                return;
            IsSettingsOpen = open;
            if (!open)
                _popup.panel?.focusController.focusedElement?.Blur();
            UpdateVisibility();
            ApplyLayout();
            e_onSettingsVisibilityChanged?.Invoke(open);
        }

        // 文字整体 Alpha 也作用于富文本颜色, 与背景透明度独立.
        private void OnSettingsChanged()
        {
            var snapshot = (_settings.ShowLatestLogs, _settings.LogLineCount, _settings.BackgroundOpacity, _settings.TextOpacity);
            if (_shownSettings.HasValue && _shownSettings.Value == snapshot) return;
            _shownSettings = snapshot;
            ApplyBackground();
            foreach (var label in _labels)
                label.style.opacity = _settings.TextOpacity;
            _logVersion = -1;
            RefreshLogs();
            UpdateVisibility();
        }

        // 自定义属性在样式解析后通知, 风格切换不需要修改透明度设置才能刷新颜色.
        private void OnPaletteResolved(CustomStyleResolvedEvent evt) => ReadThemePalette();

        // 只读取当前风格的颜色, 不重建日志或覆盖原日志中的富文本颜色.
        private void ReadThemePalette()
        {
            if (_preview.customStyle.TryGetValue(FLOATING_BACKGROUND, out var color)) _backgroundColor = color;
            ApplyBackground();
            ApplyLayout();
        }

        // 配色来自 USS, 透明度始终来自独立浮窗设置, 不与整体透明度叠乘.
        private void ApplyBackground()
        {
            var color = _backgroundColor;
            color.a *= _settings.BackgroundOpacity;
            _preview.style.backgroundColor = color;
        }

        // 最新预览直接读取原始输出缓存, Console 暂停加载不影响此处更新.
        private void RefreshLogs()
        {
            if (_logVersion == _renderer.LogVersion)
                return;
            _logVersion = _renderer.LogVersion;
            var logs = _renderer.LatestLines;
            _rowCount = Mathf.Max(1, Mathf.Min(logs.Count, _settings.LogLineCount));
            var start = Mathf.Max(0, logs.Count - _settings.LogLineCount);
            for (var i = 0; i < _labels.Length; i++)
            {
                _rows[i].EnableInClassList("is-hidden", i >= _rowCount);
                _text.SetText(_labels[i], i < _rowCount && logs.Count > 0 ? logs[start + i] : i == 0 ? ToolkitLocalization.Get("ui.FloatingPreview.RefreshLogs") : string.Empty);
            }
            ApplyLayout();
        }

        // 设置期间保留日志预览, 让滑动条修改即时反映到背景和文字.
        private void UpdateVisibility()
        {
            _preview.EnableInClassList("is-hidden", !_visible || !_settings.ShowLatestLogs);
            _popup.EnableInClassList("is-hidden", !_visible || !IsSettingsOpen);
        }

        // 高度由原生布局按实际字体与日志行撑开, 定位不再写回固定高度.
        private void ApplyLayout()
        {
            if (_viewport.x <= 0 || _viewport.y <= 0)
                return;
            var width = CalculatePreviewWidth(_viewport, IsSettingsOpen && _settings.ShowLatestLogs);
            _preview.style.width = width;
            _preview.style.maxHeight = _viewport.y;
            var height = _preview.layout.height;
            if (float.IsNaN(height) || float.IsInfinity(height) || height < 0) height = 0;
            var logs = CalculateBounds(_iconBounds, _viewport, new Vector2(width, height), _dockEdge);
            _preview.style.translate = new Translate(logs.x, logs.y, 0);
            ApplyPopupLayout(_settings.ShowLatestLogs ? logs : _iconBounds);
        }

        // 字体或显示状态改变后, 使用布局完成的背景尺寸重新处理靠边和上下翻转.
        private void OnPreviewGeometryChanged(GeometryChangedEvent evt)
        {
            if (evt.newRect.size != evt.oldRect.size) ApplyLayout();
        }

        // 设置面板按全部控件的自然高度布局, 不通过滚动条隐藏设置项.
        private void OnPopupGeometryChanged(GeometryChangedEvent evt)
        {
            if (evt.newRect.size != evt.oldRect.size)
                ApplyLayout();
        }

        // 设置面板始终横向贴近日志, 空间不足时等比缩小, 不覆盖日志区域.
        private void ApplyPopupLayout(Rect logs)
        {
            if (_viewport.x <= 0 || _viewport.y <= 0)
                return;
            var width = Mathf.Min(PANEL_WIDTH, _viewport.x);
            _popup.style.width = width;
            var height = _popup.layout.height;
            if (float.IsNaN(height) || height <= 0)
                return;
            var bounds = CalculateSettingsBounds(logs, _viewport, new Vector2(width, height));
            var scale = bounds.width / width;
            _popup.style.scale = new Scale(new Vector3(scale, scale, 1f));
            _popup.style.translate = new Translate(bounds.x, bounds.y, 0);
        }

    }
}
