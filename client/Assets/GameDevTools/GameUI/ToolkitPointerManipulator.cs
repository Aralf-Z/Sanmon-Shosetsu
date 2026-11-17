using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 以面板局部坐标实现窗口拖动, 缩放和浮标点击还原. </summary>
    public sealed class ToolkitPointerManipulator : PointerManipulator
    {
        private readonly VisualElement _coordinateRoot;

        private readonly Func<Vector2> _getOrigin;

        private readonly Action<Vector2> _move;

        private readonly Action _finish;

        private readonly Action _click;

        private readonly Action _doubleClick;

        private readonly bool _ignoreButtons;

        private readonly Action _start;

        private readonly Action _cancel;

        private Vector2 _startPointer;

        private Vector2 _startOrigin;

        private int _pointerId = -1;

        private bool _dragged; // 移动超过阈值后不触发点击.

        public ToolkitPointerManipulator(VisualElement coordinateRoot, Func<Vector2> getOrigin, Action<Vector2> move, Action finish = null, Action click = null, bool ignoreButtons = false, Action start = null, Action cancel = null, Action doubleClick = null)
        {
            _coordinateRoot = coordinateRoot;
            _getOrigin = getOrigin;
            _move = move;
            _finish = finish;
            _click = click;
            _doubleClick = doubleClick;
            _ignoreButtons = ignoreButtons;
            _start = start;
            _cancel = cancel;
        }

        /// <summary> 隐藏窗口或销毁时主动结束捕获, 不触发点击还原. </summary>
        public void Cancel()
        {
            if (_pointerId < 0) return;

            if (ReleasePointer())
            {
                _cancel?.Invoke();
            }
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<PointerCaptureOutEvent>(OnCaptureOut);
            target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
        }

        // 注销时先释放捕获, 防止鼠标在窗口外松开后遗留拖动状态.
        protected override void UnregisterCallbacksFromTarget()
        {
            Cancel();
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<PointerCaptureOutEvent>(OnCaptureOut);
            target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
        }

        // 窗眉按钮保留正常点击行为, 其余区域捕获左键.
        private void OnPointerDown(PointerDownEvent evt)
        {
            if (_pointerId >= 0 || evt.button != 0) return;

            if (_ignoreButtons)
            {
                for (var element = evt.target as VisualElement; element != null && element != target; element = element.parent)
                {
                    if (element is Button) return;
                }
            }

            _pointerId = evt.pointerId;
            _startPointer = _coordinateRoot.WorldToLocal((Vector2)evt.position);
            _startOrigin = _getOrigin();
            _dragged = false;
            target.CapturePointer(_pointerId);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (evt.pointerId != _pointerId || !target.HasPointerCapture(_pointerId)) return;

            UpdateDrag((Vector2)evt.position);
            evt.StopPropagation();
        }

        // 先结束捕获状态, 避免 CaptureOut 再次提交相同操作.
        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.pointerId != _pointerId || evt.button != 0) return;
            // 松手位置可能晚于本帧最后一个 Move, 必须先保存最终候选值.

            if (_dragged)
            {
                UpdateDrag((Vector2)evt.position);
            }

            evt.StopPropagation();
            if (_pointerId < 0) return;

            if (ReleasePointer())
            {
                _finish?.Invoke();
            }
            else if (evt.clickCount == 2 && _doubleClick != null)
            {
                _doubleClick.Invoke();
            }
            else
            {
                _click?.Invoke();
            }
        }

        private void OnCaptureOut(PointerCaptureOutEvent evt)
        {
            if (evt.pointerId != _pointerId) return;

            if (ReleasePointer())
            {
                (_cancel ?? _finish)?.Invoke();
            }
        }

        private void OnPointerCancel(PointerCancelEvent evt)
        {
            if (evt.pointerId != _pointerId) return;

            Cancel();
            evt.StopPropagation();
        }

        private void UpdateDrag(Vector2 position)
        {
            var delta = _coordinateRoot.WorldToLocal(position) - _startPointer;

            if (!_dragged && delta.sqrMagnitude > 16f)
            {
                _dragged = true;
                _start?.Invoke();
            }

            if (_pointerId >= 0 && _dragged)
            {
                _move(_startOrigin + delta);
            }
        }

        // 先清理状态再释放捕获, 避免 CaptureOut 重入导致结束回调重复执行.
        private bool ReleasePointer()
        {
            var pointerId = _pointerId;
            var dragged = _dragged;

            _pointerId = -1;
            _dragged = false;
            if (pointerId >= 0 && target.HasPointerCapture(pointerId))
            {
                target.ReleasePointer(pointerId);
            }

            return dragged;
        }
    }
}
