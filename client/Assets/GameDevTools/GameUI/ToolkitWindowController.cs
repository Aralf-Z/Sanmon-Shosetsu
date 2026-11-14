using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 管理相对视口的窗口尺寸, 拖动缩放和浮标停靠. </summary>
    public sealed class ToolkitWindowController : IDisposable
    {
        private const float SNAP_DISTANCE = 24f; // 浮标距边缘小于此面板单位时自动停靠.

        /// <summary> 最小化状态变化时通知宿主更新焦点和页面加载. </summary>
        public event Action<bool> e_onMinimizedChanged;

        /// <summary> 浮标设置弹窗开关时通知宿主更新工具内部的焦点和设置. </summary>
        public event Action<bool> e_onFloatingSettingsChanged;

        /// <summary> 当前是否显示浮标. </summary>
        public bool IsMinimized { get; private set; }

        /// <summary> 当前是否通过浮标编辑设置. </summary>
        public bool IsFloatingSettingsOpen => _floatingPreview.IsSettingsOpen;

        /// <summary> 只更新浮标设置和空日志提示, 不改变显示状态. </summary>
        internal void RefreshLanguage() => _floatingPreview.RefreshLanguage();

        /// <summary> 当前窗口在文档中的位置与大小. </summary>
        public Rect WindowBounds => _bounds;

        /// <summary> 当前浮标在文档中的位置与大小. </summary>
        public Rect FloatingBounds => new(_floatingPosition, FloatingSize);

        private readonly VisualElement _root; // 铺满当前 Game 视口的文档.
        private readonly VisualElement _window; // 可移动缩放的主窗口.
        private readonly VisualElement _floating; // 最小化后用于拖动和还原的小图标.
        private readonly VisualElement _header; // 除按钮外均可拖动的窗眉.
        private readonly VisualElement _resize; // 右下角缩放热区.
        private readonly VisualElement _resizePreview; // 独立于内容布局的缩放预览边框.
        private readonly Button _minimize; // 窗眉最小化按钮.
        private readonly ToolkitWindowRail _rail; // 不参与主窗口内容布局的页签和悬停详情.
        private readonly ToolkitFloatingPreview _floatingPreview; // 独立于图标裁剪区域的预览和设置.
        private readonly ToolkitPointerManipulator _drag; // 窗眉拖动.
        private readonly ToolkitPointerManipulator _scale; // 右下角缩放.
        private readonly ToolkitPointerManipulator _floatDrag; // 浮标拖动和点击.
        private Rect _bounds; // 主窗口几何信息, 最小化时保留以供还原.
        private Rect _resizeBounds; // 拖动中的候选边界, 松手前不写入主窗口.
        private Vector2 _viewport; // 当前文档尺寸, 单位为面板单位.
        private Vector2 _floatingPosition; // 浮标独立的位置.
        private int _dockEdge; // 0 为自由浮动, 1 到 4 为左, 右, 上, 下.
        private int _resizePreviewFrame = -1; // 同一帧最多更新一次预览几何.
        private bool _isResizing; // 超过拖动阈值后进入边框预览.
        private bool _resizePreviewDirty; // 合并本帧多次指针移动.
        private bool _visible; // 工具整体是否启用显示.
        private bool _disposed; // 释放后不再响应尺寸事件.

        private Vector2 FloatingSize => Vector2.Min(new Vector2(76f, 56f), _viewport); // 各风格共用命中区, 徽标外观由 USS 决定.

        /// <summary> 绑定窗口交互和公共 Config 的浮标设置. </summary>
        public ToolkitWindowController(VisualElement root, ToolkitConsoleRenderer renderer, ToolkitWindowSettings settings)
        {
            _root = root;
            _window = ToolkitView.Require<VisualElement>(root, "toolkitWindow");
            _floating = ToolkitView.Require<VisualElement>(root, "floatingTool");
            _header = ToolkitView.Require<VisualElement>(root, "windowHeader");
            _resize = ToolkitView.Require<VisualElement>(root, "windowResizeHandle");
            _resizePreview = ToolkitView.Require<VisualElement>(root, "windowResizePreview");
            _minimize = ToolkitView.Require<Button>(root, "minimizeWindow");
            _rail = new ToolkitWindowRail(root);
            _floatingPreview = new ToolkitFloatingPreview(root, renderer, settings);
            _floatingPreview.e_onSettingsVisibilityChanged += OnFloatingSettingsChanged;
            _drag = new ToolkitPointerManipulator(root, () => _bounds.position, MoveWindow, ignoreButtons: true, doubleClick: ResetWindowPosition);
            _scale = new ToolkitPointerManipulator(root, () => _bounds.size, PreviewResize, CommitResize, start: BeginResize, cancel: CancelResize);
            _floatDrag = new ToolkitPointerManipulator(root, () => _floatingPosition, MoveFloating, DockFloating, Restore, ignoreButtons: true);
            _header.AddManipulator(_drag);
            _resize.AddManipulator(_scale);
            _floating.AddManipulator(_floatDrag);
            _minimize.clicked += Minimize;
            _root.RegisterCallback<GeometryChangedEvent>(OnViewportChanged);
            SetVisible(false);
        }

        /// <summary> 快捷键重新打开时还原主窗口, 关闭时隐藏窗口和浮标. </summary>
        public void SetVisible(bool visible)
        {
            _visible = visible;
            IsMinimized = false;
            CancelPointers();
            UpdateVisibility();
            UpdateViewport();
        }

        /// <summary> 切换浮标状态, 不改变主窗口之前的位置与大小. </summary>
        public void SetMinimized(bool minimized)
        {
            if (_disposed || !_visible || IsMinimized == minimized)
                return;
            CancelPointers();
            IsMinimized = minimized;
            UpdateVisibility();
            e_onMinimizedChanged?.Invoke(minimized);
        }

        /// <summary> 即使 Console 页隐藏, 浮标仍读取新的输出日志. </summary>
        public void Tick(bool keyboardBlocked = false)
        {
            if (_disposed)
                return;
            UpdateViewport();
            if (!Application.isFocused)
            {
                _scale.Cancel();
                CancelResize();
            }
            UpdateResizePreview();
            _floatingPreview.Tick(keyboardBlocked);
        }

        /// <summary> 显式解除所有指针捕获和事件订阅. </summary>
        public void Dispose()
        {
            _disposed = true;
            CancelPointers();
            _header.RemoveManipulator(_drag);
            _resize.RemoveManipulator(_scale);
            _floating.RemoveManipulator(_floatDrag);
            _minimize.clicked -= Minimize;
            _root.UnregisterCallback<GeometryChangedEvent>(OnViewportChanged);
            _floatingPreview.e_onSettingsVisibilityChanged -= OnFloatingSettingsChanged;
            _floatingPreview.Dispose();
            _rail.Dispose();
            e_onMinimizedChanged = null;
            e_onFloatingSettingsChanged = null;
            _floating.AddToClassList("is-hidden");
        }

        // 最小化按钮只改变新 UI 的呈现状态.
        private void Minimize() => SetMinimized(true);

        // 浮标未发生拖动的点击还原窗口.
        private void Restore() => SetMinimized(false);

        // 屏幕尺寸更新后按原比例调整窗口, 并重新约束停靠位置.
        private void OnViewportChanged(GeometryChangedEvent evt) => UpdateViewport();

        // 宽高分别默认占视口的 5/6, 最小占四分之一.
        private void UpdateViewport()
        {
            var size = _root.layout.size;
            if (_disposed || size == _viewport)
                return;
            _scale.Cancel();
            CancelResize();
            if (float.IsNaN(size.x) || float.IsNaN(size.y) || float.IsInfinity(size.x) || float.IsInfinity(size.y) || size.x <= 0 || size.y <= 0)
                return;
            if (_viewport.x <= 0 || _viewport.y <= 0)
            {
                _bounds = new Rect(DefaultWindowPosition(size), size * (5f / 6f));
                _floatingPosition = new Vector2(size.x - FloatingSize.x, size.y * 0.5f);
                _dockEdge = 2;
            }
            else
            {
                var ratio = new Vector2(size.x / _viewport.x, size.y / _viewport.y);
                _bounds = new Rect(Vector2.Scale(_bounds.position, ratio), Vector2.Scale(_bounds.size, ratio));
                _floatingPosition = Vector2.Scale(_floatingPosition, ratio);
            }
            _viewport = size;
            ResizeWindow(_bounds.size);
            ApplyFloating();
        }

        // 主窗口自由移动, 仅最小化浮标受视口边界和停靠约束.
        private void MoveWindow(Vector2 position)
        {
            _bounds.position = position;
            ApplyWindow();
        }

        // 与首次打开共用默认位置, 为外侧页签预留空间.
        private static Vector2 DefaultWindowPosition(Vector2 viewport)
        {
            return new Vector2(Mathf.Max(viewport.x / 12f, ToolkitWindowRail.ReservedWidth(viewport.x)), viewport.y / 12f);
        }

        // 双击窗眉仅恢复位置, 保留当前尺寸且不写入配置.
        private void ResetWindowPosition()
        {
            if (_disposed || !_visible || IsMinimized) return;
            CancelPointers();
            UpdateViewport();
            if (_viewport.x <= 0 || _viewport.y <= 0) return;
            MoveWindow(DefaultWindowPosition(_viewport));
        }

        // 保留当前位置, 尺寸限制不依赖窗口是否出界.
        private void ResizeWindow(Vector2 size)
        {
            _bounds.size = ClampWindowSize(size);
            ApplyWindow();
        }

        // 实际窗口与预览使用同一套尺寸限制, 保持左上角固定.
        private Vector2 ClampWindowSize(Vector2 size)
        {
            return new Vector2(Mathf.Clamp(size.x, _viewport.x * .25f, _viewport.x), Mathf.Clamp(size.y, _viewport.y * .25f, _viewport.y));
        }

        // 只有有效拖动才显示预览, 点击缩放角不会修改窗口.
        private void BeginResize()
        {
            _resizeBounds = _bounds;
            _isResizing = true;
            _resizePreviewDirty = true;
        }

        // 指针事件只保存候选尺寸, 不触发日志列表和页面重排.
        private void PreviewResize(Vector2 size)
        {
            UpdateViewport();
            if (!_isResizing)
                return;
            var clamped = ClampWindowSize(size);
            if (clamped == _resizeBounds.size)
                return;
            _resizeBounds.size = clamped;
            _resizePreviewDirty = true;
        }

        // 所有 Move 合并到每帧一次, 边框不包含任何页面内容.
        private void UpdateResizePreview()
        {
            if (!_isResizing || !_resizePreviewDirty || _resizePreviewFrame == Time.frameCount)
                return;
            _resizePreviewFrame = Time.frameCount;
            _resizePreviewDirty = false;
            _resizePreview.style.width = _resizeBounds.width;
            _resizePreview.style.height = _resizeBounds.height;
            _resizePreview.style.translate = new Translate(_resizeBounds.x, _resizeBounds.y, 0);
            _resizePreview.RemoveFromClassList("is-hidden");
        }

        // 松手后一次提交最终尺寸, 同步更新紧凑布局并移除预览.
        private void CommitResize()
        {
            UpdateViewport();
            if (!_isResizing)
                return;
            if (_bounds.size != _resizeBounds.size)
                ResizeWindow(_resizeBounds.size);
            CancelResize();
        }

        // 关闭窗口, 失去捕获或视口变化时丢弃候选尺寸.
        private void CancelResize()
        {
            _isResizing = false;
            _resizePreviewDirty = false;
            _resizePreview.AddToClassList("is-hidden");
        }

        // 只有交互几何使用内联尺寸与 translate, 外观和紧凑布局全部由 USS 控制.
        private void ApplyWindow()
        {
            _window.style.width = _bounds.width;
            _window.style.height = _bounds.height;
            _window.style.translate = new Translate(_bounds.x, _bounds.y, 0);
            _window.EnableInClassList("compact-window", _bounds.width < 760f);
            _window.EnableInClassList("narrow-window", _bounds.width < 540f);
            _rail.Apply(_bounds, _viewport);
        }

        // 拖离边缘后恢复自由浮动, 松手时再判断吸附.
        private void MoveFloating(Vector2 position)
        {
            _dockEdge = 0;
            _floatingPosition = position;
            ApplyFloating();
        }

        // 只在靠近边缘时停靠, 中间位置允许自由悬浮.
        private void DockFloating()
        {
            var remaining = Vector2.Max(Vector2.zero, _viewport - FloatingSize);
            var distances = new[] { _floatingPosition.x, remaining.x - _floatingPosition.x, _floatingPosition.y, remaining.y - _floatingPosition.y };
            var nearest = SNAP_DISTANCE;
            for (var i = 0; i < distances.Length; i++)
                if (distances[i] <= nearest)
                {
                    nearest = distances[i];
                    _dockEdge = i + 1;
                }
            ApplyFloating();
        }

        // 日志行数或视口改变后, 停靠边仍与窗口边缘对齐.
        private void ApplyFloating()
        {
            var size = FloatingSize;
            var remaining = Vector2.Max(Vector2.zero, _viewport - size);
            _floatingPosition = new Vector2(Mathf.Clamp(_floatingPosition.x, 0, remaining.x), Mathf.Clamp(_floatingPosition.y, 0, remaining.y));
            if (_dockEdge == 1) _floatingPosition.x = 0;
            if (_dockEdge == 2) _floatingPosition.x = remaining.x;
            if (_dockEdge == 3) _floatingPosition.y = 0;
            if (_dockEdge == 4) _floatingPosition.y = remaining.y;
            _floating.style.width = size.x;
            _floating.style.height = size.y;
            _floating.style.translate = new Translate(_floatingPosition.x, _floatingPosition.y, 0);
            _floating.EnableInClassList("docked", _dockEdge != 0);
            _floatingPreview.UpdateGeometry(FloatingBounds, _viewport, _dockEdge);
        }

        // 浮标弹窗拥有独立的编辑状态, 不改变主窗口最小化状态.
        private void OnFloatingSettingsChanged(bool open) => e_onFloatingSettingsChanged?.Invoke(open);

        // 关闭浮标或窗口时同步移除拖动捕获.
        private void UpdateVisibility()
        {
            _window.EnableInClassList("is-hidden", !_visible || IsMinimized);
            _rail.SetVisible(_visible && !IsMinimized);
            _floating.EnableInClassList("is-hidden", !_visible || !IsMinimized);
            _floatingPreview.SetVisible(_visible && IsMinimized);
        }

        // 不让隐藏的控件继续追踪指针.
        private void CancelPointers()
        {
            _drag.Cancel();
            _scale.Cancel();
            CancelResize();
            _floatDrag.Cancel();
        }
    }
}
