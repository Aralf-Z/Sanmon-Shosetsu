using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 合并每帧的分隔线移动, 保留实例内的双栏宽度比例. </summary>
    internal sealed class ToolkitCheatSplitter : IDisposable
    {
        private readonly VisualElement _columns;

        private readonly VisualElement _left; // 以像素宽度控制的命令区.

        private readonly VisualElement _handle;

        private readonly ToolkitPointerManipulator _drag;

        private float _ratio = 0.25f;

        private float _startRatio;

        private float _candidate;

        private bool _dragging;

        private bool _dirty = true;

        internal ToolkitCheatSplitter(VisualElement root)
        {
            _columns = ToolkitView.Require<VisualElement>(root, "cheatColumns");
            _left = ToolkitView.Require<VisualElement>(root, "commandsColumn");
            _handle = ToolkitView.Require<VisualElement>(root, "cheatSplitter");
            _drag = new ToolkitPointerManipulator(_columns, () => new Vector2(_left.layout.width, 0), Move, Finish, start: Begin, cancel: Rollback);
            _handle.AddManipulator(_drag);
            _columns.RegisterCallback<GeometryChangedEvent>(OnGeometry);
        }

        internal void Tick()
        {
            if (!Application.isFocused)
            {
                Cancel();
            }

            if (!_dirty) return;

            var width = AvailableWidth();

            if (width <= 0 || float.IsNaN(width)) return;

            var left = ClampWidth(_dragging ? _candidate : width * _ratio, width);

            _left.style.flexGrow = 0;
            _left.style.flexBasis = StyleKeyword.Auto;
            _left.style.width = left;
            _dirty = false;
        }

        internal void Cancel()
        {
            _drag.Cancel();
            if (_dragging)
            {
                Rollback();
            }
        }

        public void Dispose()
        {
            Cancel();
            _handle.RemoveManipulator(_drag);
            _columns.UnregisterCallback<GeometryChangedEvent>(OnGeometry);
        }

        internal static float ClampWidth(float value, float available)
        {
            var scale = Mathf.Min(1f, available / 300f);

            return Mathf.Clamp(value, 120f * scale, available - 180f * scale);
        }

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

        private void Finish()
        {
            if (!_dragging) return;

            var width = AvailableWidth();

            if (width > 0)
            {
                _ratio = ClampWidth(_candidate, width) / width;
            }

            _dragging = false;
            _dirty = true;
            _handle.RemoveFromClassList("dragging");
        }

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
