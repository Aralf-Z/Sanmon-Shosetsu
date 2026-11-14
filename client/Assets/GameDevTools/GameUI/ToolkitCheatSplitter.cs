using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 合并每帧的分隔线移动, 保留实例内的双栏宽度比例. </summary>
    internal sealed class ToolkitCheatSplitter : IDisposable
    {
        private readonly VisualElement _columns; // 双栏可用区域.
        private readonly VisualElement _left; // 以像素宽度控制的命令区.
        private readonly VisualElement _handle; // 捕获指针的分隔线.
        private readonly ToolkitPointerManipulator _drag; // 使用现有面板坐标和捕获清理逻辑.
        private float _ratio = 0.25f; // 默认命令与参数宽度比为 1:3.
        private float _startRatio; // 中断时恢复拖动前比例.
        private float _candidate; // 合并本帧指针事件后的命令宽度.
        private bool _dragging; // 只有超过拖动阈值后允许提交.
        private bool _dirty = true; // 几何改变或指针移动后需要应用.

        /// <summary> 绑定静态分隔线并监听可用宽度变化. </summary>
        internal ToolkitCheatSplitter(VisualElement root)
        {
            _columns = ToolkitView.Require<VisualElement>(root, "cheatColumns");
            _left = ToolkitView.Require<VisualElement>(root, "commandsColumn");
            _handle = ToolkitView.Require<VisualElement>(root, "cheatSplitter");
            _drag = new ToolkitPointerManipulator(_columns, () => new Vector2(_left.layout.width, 0), Move, Finish, start: Begin, cancel: Rollback);
            _handle.AddManipulator(_drag);
            _columns.RegisterCallback<GeometryChangedEvent>(OnGeometry);
        }

        /// <summary> 由面板每帧调用一次, 不在每个 PointerMove 中重排. </summary>
        internal void Tick()
        {
            if (!Application.isFocused)
                Cancel();
            if (!_dirty)
                return;
            var width = AvailableWidth();
            if (width <= 0 || float.IsNaN(width))
                return;
            var left = ClampWidth(_dragging ? _candidate : width * _ratio, width);
            _left.style.flexGrow = 0;
            _left.style.flexBasis = StyleKeyword.Auto;
            _left.style.width = left;
            _dirty = false;
        }

        /// <summary> 切页, 关闭或最小化时恢复原比例并移除捕获. </summary>
        internal void Cancel()
        {
            _drag.Cancel();
            if (_dragging)
                Rollback();
        }

        /// <summary> 释放捕获及尺寸监听. </summary>
        public void Dispose()
        {
            Cancel();
            _handle.RemoveManipulator(_drag);
            _columns.UnregisterCallback<GeometryChangedEvent>(OnGeometry);
        }

        // 左右最小宽度为 120 和 180, 窄窗口按相同比例压缩.
        internal static float ClampWidth(float value, float available)
        {
            var scale = Mathf.Min(1f, available / 300f);
            return Mathf.Clamp(value, 120f * scale, available - 180f * scale);
        }

        // 分隔线自身宽度及边距不参与两栏比例.
        private float AvailableWidth() => Mathf.Max(0, _columns.contentRect.width - _handle.resolvedStyle.width - _handle.resolvedStyle.marginLeft - _handle.resolvedStyle.marginRight);

        // 开始移动时保留比例, 不受随后几何回调覆盖.
        private void Begin()
        {
            _startRatio = _ratio;
            _candidate = _left.layout.width;
            _dragging = true;
            _handle.AddToClassList("dragging");
        }

        // 指针事件仅保存候选值.
        private void Move(Vector2 value)
        {
            _candidate = value.x;
            _dirty = true;
        }

        // 松手保存最终指针对应比例, 下一次 Tick 统一应用宽度.
        private void Finish()
        {
            if (!_dragging)
                return;
            var width = AvailableWidth();
            if (width > 0)
                _ratio = ClampWidth(_candidate, width) / width;
            _dragging = false;
            _dirty = true;
            _handle.RemoveFromClassList("dragging");
        }

        // 捕获中断不保存中间拖动结果.
        private void Rollback()
        {
            _ratio = _startRatio;
            _dragging = false;
            _dirty = true;
            _handle.RemoveFromClassList("dragging");
        }

        // 可用区域变化使旧指针坐标失效, 回滚并按保留的比例重新约束.
        private void OnGeometry(GeometryChangedEvent evt)
        {
            Cancel();
            _dirty = true;
        }
    }
}
