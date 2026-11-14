using UnityEngine;
using UnityEngine.UIElements;
using PointerType = UnityEngine.UIElements.PointerType;

namespace GameConsole.UI
{
    internal sealed partial class ToolkitConsoleViewport
    {
        private void RegisterInput()
        {
            _scroll.RegisterCallback<WheelEvent>(Wheel, TrickleDown.TrickleDown);
            _scroll.RegisterCallback<KeyDownEvent>(Key, TrickleDown.TrickleDown);
            _scroll.RegisterCallback<PointerDownEvent>(PointerDown, TrickleDown.TrickleDown);
            _scroll.RegisterCallback<PointerMoveEvent>(PointerMove, TrickleDown.TrickleDown);
            _scroll.RegisterCallback<PointerUpEvent>(PointerUp, TrickleDown.TrickleDown);
            _scroll.RegisterCallback<PointerCancelEvent>(PointerCancel, TrickleDown.TrickleDown);
            _scroll.RegisterCallback<PointerCaptureOutEvent>(CaptureOut, TrickleDown.TrickleDown);
            _scroll.RegisterCallback<MouseDownEvent>(MouseDown, TrickleDown.TrickleDown);
            _scroll.RegisterCallback<MouseUpEvent>(MouseUp, TrickleDown.TrickleDown);
            _host.RegisterCallback<PointerDownEvent>(ForwardTouchDown, TrickleDown.TrickleDown);
            _host.RegisterCallback<PointerUpEvent>(ForwardTouchUp, TrickleDown.TrickleDown);
            _host.RegisterCallback<PointerCancelEvent>(ForwardTouchCancel, TrickleDown.TrickleDown);
        }

        private void UnregisterInput()
        {
            _scroll.UnregisterCallback<WheelEvent>(Wheel, TrickleDown.TrickleDown);
            _scroll.UnregisterCallback<KeyDownEvent>(Key, TrickleDown.TrickleDown);
            _scroll.UnregisterCallback<PointerDownEvent>(PointerDown, TrickleDown.TrickleDown);
            _scroll.UnregisterCallback<PointerMoveEvent>(PointerMove, TrickleDown.TrickleDown);
            _scroll.UnregisterCallback<PointerUpEvent>(PointerUp, TrickleDown.TrickleDown);
            _scroll.UnregisterCallback<PointerCancelEvent>(PointerCancel, TrickleDown.TrickleDown);
            _scroll.UnregisterCallback<PointerCaptureOutEvent>(CaptureOut, TrickleDown.TrickleDown);
            _scroll.UnregisterCallback<MouseDownEvent>(MouseDown, TrickleDown.TrickleDown);
            _scroll.UnregisterCallback<MouseUpEvent>(MouseUp, TrickleDown.TrickleDown);
            _host.UnregisterCallback<PointerDownEvent>(ForwardTouchDown, TrickleDown.TrickleDown);
            _host.UnregisterCallback<PointerUpEvent>(ForwardTouchUp, TrickleDown.TrickleDown);
            _host.UnregisterCallback<PointerCancelEvent>(ForwardTouchCancel, TrickleDown.TrickleDown);
        }

        // 原生触摸起止监听位于 contentContainer, 可视层在其外部时只转发触摸事件.
        // Move 仍沿共同父级交给原生 ScrollView, 捕获和惯性均不自行实现.
        private void ForwardTouchDown(PointerDownEvent evt)
        {
            if (evt.pointerType == PointerType.mouse || !evt.isPrimary) return;
            using var copy = PointerDownEvent.GetPooled(evt);
            copy.target = _scroll.contentContainer;
            _scroll.contentContainer.SendEvent(copy);
            if (copy.isPropagationStopped) evt.StopPropagation();
        }

        private void ForwardTouchUp(PointerUpEvent evt)
        {
            if (evt.pointerType == PointerType.mouse || !evt.isPrimary) return;
            using var copy = PointerUpEvent.GetPooled(evt);
            copy.target = _scroll.contentContainer;
            _scroll.contentContainer.SendEvent(copy);
            if (copy.isPropagationStopped) evt.StopPropagation();
        }

        private void ForwardTouchCancel(PointerCancelEvent evt)
        {
            if (evt.pointerType == PointerType.mouse || !evt.isPrimary) return;
            using var copy = PointerCancelEvent.GetPooled(evt);
            copy.target = _scroll.contentContainer;
            _scroll.contentContainer.SendEvent(copy);
        }

        // 只接受用户滚动或惯性产生的偏移, 不采用原生布局重算时的临时夹取值.
        private void OnScroll(float value)
        {
            if (_applying || !_visible) return;
            var user = IsScrollbarDragging || _pointerMoved || _userPending;
            if (user || !Following && !_extentPending)
            {
                var previous = _offset;
                _offset = Mathf.Clamp(value, 0, _heights.MaxOffset(_height));
                _anchor = default;
                Following = !IsScrollbarDragging && !_upwardPending && AtBottom();
                if (_offset < previous && _offset <= 80f) { Following = false; _historyRequested = true; }
                PositionRows();
            }
            _dirty = true;
        }

        private void Wheel(WheelEvent evt)
        {
            if (!_visible || evt.delta.y == 0) return;
            _userPending = true;
            _anchor = default;
            if (evt.delta.y < 0) RequestUpward();
        }

        private void Key(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.UpArrow && evt.keyCode != KeyCode.PageUp && evt.keyCode != KeyCode.Home && evt.keyCode != KeyCode.DownArrow && evt.keyCode != KeyCode.PageDown && evt.keyCode != KeyCode.End) return;
            _userPending = true;
            _anchor = default;
            if (evt.keyCode == KeyCode.UpArrow || evt.keyCode == KeyCode.PageUp || evt.keyCode == KeyCode.Home) RequestUpward();
        }

        private void RequestUpward()
        {
            _upwardPending = true;
            Following = false;
            if (_offset <= 80f) _historyRequested = true;
        }

        // 原生滚动条按下时立即停止跟随, 不接管其捕获或消费事件.
        private void PointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;
            _pointerDown = true;
            if (evt.pointerType != PointerType.mouse) _touchPointer = evt.pointerId;
            if (IsScrollbarTarget(evt.target)) BeginScrollbarDrag(evt.pointerId);
        }

        private void PointerMove(PointerMoveEvent evt)
        {
            if (!_pointerDown) return;
            _pointerMoved = true;
            _userPending = true;
        }

        private void PointerUp(PointerUpEvent evt)
        {
            if (evt.button != 0) return;
            _pointerDown = false;
            if (evt.pointerId == _touchPointer) { _touchPointer = -1; _userPending = true; }
            if (evt.pointerId == _scrollbarPointer) EndScrollbarDrag();
        }

        private void PointerCancel(PointerCancelEvent evt)
        {
            _pointerDown = false;
            if (evt.pointerId == _touchPointer) { _touchPointer = -1; _userPending = true; }
            if (evt.pointerId == _scrollbarPointer) EndScrollbarDrag();
        }

        // 捕获在滚动条内部切换时继续交给原生控件处理.
        private void CaptureOut(PointerCaptureOutEvent evt)
        {
            if (evt.pointerId == _scrollbarPointer && IsScrollbarTarget(evt.relatedTarget)) return;
            if (evt.pointerId == _touchPointer && evt.relatedTarget is VisualElement element && _scroll.Contains(element)) return;
            _pointerDown = false;
            if (evt.pointerId == _touchPointer) { _touchPointer = -1; _userPending = true; }
            if (evt.pointerId == _scrollbarPointer) EndScrollbarDrag();
        }

        private void MouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0 && IsScrollbarTarget(evt.target)) BeginScrollbarDrag(PointerId.mousePointerId);
        }

        private void MouseUp(MouseUpEvent evt)
        {
            if (evt.button == 0 && _scrollbarPointer == PointerId.mousePointerId) EndScrollbarDrag();
        }

        private bool IsScrollbarTarget(object target) => target is VisualElement element && (element == _scroll.verticalScroller || _scroll.verticalScroller.Contains(element));

        private void BeginScrollbarDrag(int pointer)
        {
            if (!_visible || _disposed || IsScrollbarDragging) return;
            _scrollbarPointer = pointer;
            Following = false;
            _anchor = default;
            _userPending = _upwardPending = false;
        }

        private void EndScrollbarDrag()
        {
            if (!IsScrollbarDragging) return;
            _scrollbarPointer = -1;
            _pointerDown = _pointerMoved = false;
            _anchor = default;
            _userPending = true;
            _upwardPending = false;
            _dirty = true;
        }
    }
}
