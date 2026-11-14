using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 置顶行和网格共用拖拽, 捕获位于稳定宿主以免虚拟化回收中断提交. </summary>
    internal sealed class ToolkitPinnedDrag : IDisposable
    {
        private readonly VisualElement _host; // 稳定的事件捕获容器.
        private readonly bool _macros; // 独立的配置置顶序列.
        private readonly Dictionary<VisualElement, Binding> _bindings = new(); // 仅可见项参与命中.
        private readonly ScrollView _scroll; // 边缘停留时持续滚动可视区域.
        private readonly IVisualElementScheduledItem _scrollTask; // 仅拖动期间启用.
        private readonly TemplateContainer _preview; // 挂到文档顶层, 避免被列表和网格裁剪.
        private readonly Label _previewName; // 按下时的名称快照, 行回收不改变提示.
        private string _source; // 开始时的持久化标识.
        private VisualElement _sourceItem; // 仅用于来源外观, 不参与持久化和拖动身份.
        private VisualElement _target; // 当前插入标记所在项.
        private Vector2 _origin; // 面板坐标中的按下位置.
        private Vector2 _position; // 最新面板指针位置.
        private bool _dragged; // 越过阈值才进入拖动.
        private bool _after; // 插入在目标之前或之后.
        private int _pointer = -1; // 捕获的指针编号.

        /// <summary> 捕获宿主必须不随列表行回收而销毁. </summary>
        internal ToolkitPinnedDrag(VisualElement host, bool macros)
        {
            _host = host;
            _macros = macros;
            _scroll = host as ScrollView ?? host.Q<ScrollView>();
            _preview = ToolkitFactory.InstantiateTemplate("ToolkitPinnedDragPreview");
            _preview.pickingMode = PickingMode.Ignore;
            _preview.usageHints = UsageHints.DynamicTransform;
            _preview.AddToClassList("pinned-drag-preview");
            _previewName = ToolkitView.Require<Label>(_preview, "pinnedDragPreviewName");
            _scrollTask = host.schedule.Execute(AutoScroll).Every(16);
            _scrollTask.Pause();
            host.RegisterCallback<PointerMoveEvent>(Move);
            host.RegisterCallback<PointerUpEvent>(Up);
            host.RegisterCallback<PointerCancelEvent>(CancelPointer);
            host.RegisterCallback<PointerCaptureOutEvent>(CaptureOut);
            host.RegisterCallback<KeyDownEvent>(KeyDown);
            host.RegisterCallback<DetachFromPanelEvent>(HostDetached);
        }

        /// <summary> 绑定一次回调, 每次交互读取当前数据以适应池化控件. </summary>
        internal void Attach(VisualElement item, VisualElement handle, Func<string> getId, Func<string> getName)
        {
            EventCallback<PointerDownEvent> down = evt =>
            {
                var id = getId();
                if (evt.button != 0 || id == null || !Config.GetPins(_macros).Contains(id)) return;
                Cancel();
                _source = id;
                _sourceItem = item;
                _previewName.text = getName() ?? id;
                _origin = evt.position;
                _position = _origin;
                _pointer = evt.pointerId;
                _host.CapturePointer(_pointer);
                _scrollTask.Resume();
                evt.StopImmediatePropagation();
            };
            handle.RegisterCallback(down);
            _bindings.Add(item, new Binding { id = getId, handle = handle, down = down });
        }

        /// <summary> 行回收只清除过期目标, 来源标识和捕获保留在稳定宿主上. </summary>
        internal void Recycle(VisualElement item)
        {
            if (_target == item) ClearTarget();
            if (_sourceItem == item) ClearSource();
        }

        /// <summary> 永久删除一个行控件的绑定. </summary>
        internal void Detach(VisualElement item)
        {
            Recycle(item);
            if (!_bindings.TryGetValue(item, out var binding)) return;
            binding.handle.UnregisterCallback(binding.down);
            _bindings.Remove(item);
        }

        /// <summary> 隐藏页面, 改变筛选或布局时取消未提交的拖动. </summary>
        internal void Cancel()
        {
            var pointer = _pointer;
            _pointer = -1;
            _source = null;
            _dragged = false;
            _scrollTask.Pause();
            _preview.parent?.UnregisterCallback<KeyDownEvent>(KeyDown, TrickleDown.TrickleDown);
            _preview.RemoveFromHierarchy();
            ClearSource();
            ClearTarget();
            if (pointer >= 0 && _host.HasPointerCapture(pointer)) _host.ReleasePointer(pointer);
        }

        /// <summary> 注销稳定宿主和所有池项上的回调. </summary>
        public void Dispose()
        {
            Cancel();
            foreach (var item in _bindings.Keys.ToArray()) Detach(item);
            _host.UnregisterCallback<PointerMoveEvent>(Move);
            _host.UnregisterCallback<PointerUpEvent>(Up);
            _host.UnregisterCallback<PointerCancelEvent>(CancelPointer);
            _host.UnregisterCallback<PointerCaptureOutEvent>(CaptureOut);
            _host.UnregisterCallback<KeyDownEvent>(KeyDown);
            _host.UnregisterCallback<DetachFromPanelEvent>(HostDetached);
        }

        // 使用面板坐标进行命中, 网格按横向中点决定插入位置.
        private void Move(PointerMoveEvent evt)
        {
            if (evt.pointerId != _pointer) return;
            _position = evt.position;
            if (!_dragged && ((Vector2)evt.position - _origin).sqrMagnitude <= 16f) return;
            _dragged = true;
            UpdatePreview();
            UpdateTarget();
            evt.StopPropagation();
        }

        // 目标回收或滚动后重新命中, 不依赖来源行仍然可见.
        private void UpdateTarget()
        {
            ClearTarget();
            if (!_host.worldBound.Contains(_position)) return;
            foreach (var pair in _bindings)
            {
                var item = pair.Key;
                var id = pair.Value.id();
                if (item.panel == null || !item.visible || item.resolvedStyle.display == DisplayStyle.None || id == null || id == _source || !Config.GetPins(_macros).Contains(id) || !item.worldBound.Contains(_position)) continue;
                _target = item;
                _after = _macros ? _position.x > item.worldBound.center.x : _position.y > item.worldBound.center.y;
                item.AddToClassList(_macros ? (_after ? "drop-right" : "drop-left") : (_after ? "drop-bottom" : "drop-top"));
                break;
            }
        }

        // 指针停在边缘也能跨越可视范围排序, 来源行回收不取消稳定捕获.
        private void AutoScroll()
        {
            if (!Application.isFocused || _host.panel == null) { Cancel(); return; }
            if (_dragged) UpdatePreview();
            if (!_dragged || _scroll == null) return;
            var maximum = _scroll.verticalScroller.highValue;
            if (float.IsNaN(maximum) || float.IsInfinity(maximum)) return;
            maximum = Mathf.Max(0, maximum);
            var rect = _scroll.contentViewport.worldBound;
            var offset = _scroll.scrollOffset;
            var next = float.IsNaN(offset.y) || float.IsInfinity(offset.y) ? 0 : Mathf.Clamp(offset.y, 0, maximum);
            if (maximum > 0 && rect.Contains(_position))
            {
                var edge = Mathf.Min(24f, rect.height * .25f);
                var delta = _position.y < rect.yMin + edge ? -8 : _position.y > rect.yMax - edge ? 8 : 0;
                next = Mathf.Clamp(next + delta, 0, maximum);
            }
            if (next != offset.y) _scroll.scrollOffset = new Vector2(offset.x, next);
            UpdateTarget();
        }

        // 松手只提交一次配置, 不让事件继续触发按钮执行.
        private void Up(PointerUpEvent evt)
        {
            if (evt.pointerId != _pointer || evt.button != 0) return;
            _position = evt.position;
            if (_dragged) UpdateTarget();
            var source = _source;
            var target = _target != null && _bindings.TryGetValue(_target, out var binding) ? binding.id() : null;
            var after = _after;
            var commit = _dragged && target != null && _host.worldBound.Contains(evt.position);
            Cancel();
            evt.StopImmediatePropagation();
            if (commit) Config.MovePinned(source, target, _macros, after);
        }

        // 移出目标后删除插入边线.
        private void ClearTarget()
        {
            if (_target == null) return;
            foreach (var name in new[] { "drop-left", "drop-right", "drop-top", "drop-bottom" }) _target.RemoveFromClassList(name);
            _target = null;
        }

        // 预览只随指针移动, 不参与拾取或列表布局, 不持有回收行的数据引用.
        private void UpdatePreview()
        {
            if (_host.panel == null) return;
            if (_preview.parent == null)
            {
                var desktop = _host.panel.visualTree.Q<VisualElement>("toolkitDesktop") ?? _host.panel.visualTree;
                desktop.Add(_preview);
                desktop.RegisterCallback<KeyDownEvent>(KeyDown, TrickleDown.TrickleDown);
                _preview.BringToFront();
            }
            if (_sourceItem != null)
            {
                if (_bindings.TryGetValue(_sourceItem, out var binding) && binding.id() == _source) _sourceItem.AddToClassList("pin-drag-source");
                else ClearSource();
            }
            var parent = _preview.parent;
            var point = parent.WorldToLocal(_position);
            var size = _preview.layout.size;
            if (float.IsNaN(size.x) || size.x <= 0) size.x = 220f;
            if (float.IsNaN(size.y) || size.y <= 0) size.y = 34f;
            var bounds = parent.contentRect;
            var x = point.x + 14f;
            var y = point.y + 14f;
            if (x + size.x > bounds.xMax) x = point.x - size.x - 14f;
            if (y + size.y > bounds.yMax) y = point.y - size.y - 14f;
            x = Mathf.Clamp(x, bounds.xMin, Mathf.Max(bounds.xMin, bounds.xMax - size.x));
            y = Mathf.Clamp(y, bounds.yMin, Mathf.Max(bounds.yMin, bounds.yMax - size.y));
            _preview.style.translate = new Translate(x, y, 0);
        }

        // 清除来源行的拖动外观, 不清除独立保存的来源标识和名称快照.
        private void ClearSource()
        {
            _sourceItem?.RemoveFromClassList("pin-drag-source");
            _sourceItem = null;
        }

        private void CancelPointer(PointerCancelEvent evt) { if (evt.pointerId == _pointer) Cancel(); } // 系统中断不提交.
        private void CaptureOut(PointerCaptureOutEvent evt) { if (evt.pointerId == _pointer) Cancel(); } // 捕获丢失不提交.
        private void KeyDown(KeyDownEvent evt) { if (evt.keyCode == KeyCode.Escape && _pointer >= 0) { Cancel(); evt.StopImmediatePropagation(); } } // Escape 优先取消当前拖动.
        private void HostDetached(DetachFromPanelEvent evt) { if (evt.target == _host) Cancel(); } // 行回收不取消, 宿主拆除才清理顶层预览.

        private sealed class Binding
        {
            /// <summary> 从当前行读取标识. </summary>
            internal Func<string> id;
            /// <summary> 专用拖动柄, 与执行按钮分离. </summary>
            internal VisualElement handle;
            /// <summary> 用于注销. </summary>
            internal EventCallback<PointerDownEvent> down;
        }
    }
}
