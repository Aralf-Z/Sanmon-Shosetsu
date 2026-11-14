using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 以面板局部坐标实现窗口拖动, 缩放和浮标点击还原. </summary>
    public sealed class ToolkitPointerManipulator : PointerManipulator
    {
        private readonly VisualElement _coordinateRoot; // 将指针位置转换到窗口所属的局部空间.
        private readonly Func<Vector2> _getOrigin; // 开始操作时的位置或尺寸.
        private readonly Action<Vector2> _move; // 根据位移更新位置或尺寸.
        private readonly Action _finish; // 拖动结束后处理边缘吸附.
        private readonly Action _click; // 未超过拖动阈值时的点击操作.
        private readonly Action _doubleClick; // 未发生拖动的双击操作, 未指定时沿用普通点击.
        private readonly bool _ignoreButtons; // 窗眉上的页签和最小化按钮不启动拖动.
        private readonly Action _start; // 首次超过拖动阈值时开始预览.
        private readonly Action _cancel; // 操作中断时丢弃尚未提交的预览.
        private Vector2 _startPointer; // 捕获时的局部指针位置.
        private Vector2 _startOrigin; // 捕获时的位置或尺寸.
        private int _pointerId = -1; // 当前被捕获的指针.
        private bool _dragged; // 移动超过阈值后不触发点击.

        /// <summary> 移动回调接收初始值加上局部坐标位移. </summary>
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
            if (_pointerId < 0)
                return;
            if (ReleasePointer())
                _cancel?.Invoke();
        }

        // 只使用指针事件, 同时支持鼠标和触摸指针.
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
            if (_pointerId >= 0 || evt.button != 0)
                return;
            if (_ignoreButtons)
            {
                for (var element = evt.target as VisualElement; element != null && element != target; element = element.parent)
                    if (element is Button)
                        return;
            }
            _pointerId = evt.pointerId;
            _startPointer = _coordinateRoot.WorldToLocal((Vector2)evt.position);
            _startOrigin = _getOrigin();
            _dragged = false;
            target.CapturePointer(_pointerId);
            evt.StopPropagation();
        }

        // 使用局部坐标, 不受 Game 视图缩放和 PanelSettings DPI 缩放影响.
        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (evt.pointerId != _pointerId || !target.HasPointerCapture(_pointerId))
                return;
            UpdateDrag((Vector2)evt.position);
            evt.StopPropagation();
        }

        // 先结束捕获状态, 避免 CaptureOut 再次提交相同操作.
        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.pointerId != _pointerId || evt.button != 0)
                return;
            // 松手位置可能晚于本帧最后一个 Move, 必须先保存最终候选值.
            if (_dragged)
                UpdateDrag((Vector2)evt.position);
            evt.StopPropagation();
            if (_pointerId < 0)
                return;
            if (ReleasePointer())
                _finish?.Invoke();
            else if (evt.clickCount == 2 && _doubleClick != null)
                _doubleClick.Invoke();
            else
                _click?.Invoke();
        }

        // 捕获被其他控件抢占时也要结束当前操作.
        private void OnCaptureOut(PointerCaptureOutEvent evt)
        {
            if (evt.pointerId != _pointerId)
                return;
            if (ReleasePointer())
                (_cancel ?? _finish)?.Invoke();
        }

        // 系统取消触摸或鼠标操作时只取消预览, 不提交尺寸或触发点击.
        private void OnPointerCancel(PointerCancelEvent evt)
        {
            if (evt.pointerId != _pointerId)
                return;
            Cancel();
            evt.StopPropagation();
        }

        // 开始回调只执行一次, 并允许回调主动取消当前操作.
        private void UpdateDrag(Vector2 position)
        {
            var delta = _coordinateRoot.WorldToLocal(position) - _startPointer;
            if (!_dragged && delta.sqrMagnitude > 16f)
            {
                _dragged = true;
                _start?.Invoke();
            }
            if (_pointerId >= 0 && _dragged)
                _move(_startOrigin + delta);
        }

        // 先清理状态再释放捕获, 避免 CaptureOut 重入导致结束回调重复执行.
        private bool ReleasePointer()
        {
            var pointerId = _pointerId;
            var dragged = _dragged;
            _pointerId = -1;
            _dragged = false;
            if (pointerId >= 0 && target.HasPointerCapture(pointerId))
                target.ReleasePointer(pointerId);
            return dragged;
        }
    }
}
