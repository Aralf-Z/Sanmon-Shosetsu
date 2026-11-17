using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 帮助标题栏拖动与位置复位, 使用透明层局部坐标, 不写入配置. </summary>
    internal sealed class ToolkitHelpPlacement : IDisposable
    {
        private readonly VisualElement _backdrop;

        private readonly VisualElement _dialog;

        private readonly VisualElement _header;

        private readonly ToolkitPointerManipulator _drag;

        private Vector2 _offset;

        private Vector2 _beforeDrag;

        /// <summary> 拖动不改变正文尺寸, 不触发重新分页. </summary>
        internal ToolkitHelpPlacement(VisualElement backdrop, VisualElement dialog, VisualElement header)
        {
            _backdrop = backdrop;
            _dialog = dialog;
            _header = header;
            _drag = new ToolkitPointerManipulator(backdrop, () => _offset, Move, ignoreButtons: true, start: () => _beforeDrag = _offset, cancel: () => Move(_beforeDrag));
            _header.AddManipulator(_drag);
            _backdrop.RegisterCallback<GeometryChangedEvent>(Geometry);
            _dialog.RegisterCallback<GeometryChangedEvent>(Geometry);
        }

        internal void Reset()
        {
            Cancel();
            Move(Vector2.zero);
        }

        internal void Cancel() => _drag.Cancel();

        public void Dispose()
        {
            Cancel();
            _header.RemoveManipulator(_drag);
            _backdrop.UnregisterCallback<GeometryChangedEvent>(Geometry);
            _dialog.UnregisterCallback<GeometryChangedEvent>(Geometry);
        }

        private void Move(Vector2 offset)
        {
            var bounds = _backdrop.contentRect;
            var layout = _dialog.layout;

            if (bounds.width > 0 && bounds.height > 0 && !float.IsNaN(layout.width) && !float.IsNaN(layout.height))
            {
                offset.x = Clamp(offset.x, bounds.xMin - layout.xMin, bounds.xMax - layout.xMax);
                offset.y = Clamp(offset.y, bounds.yMin - layout.yMin, bounds.yMax - layout.yMax);
            }

            _offset = offset;
            _dialog.style.translate = new Translate(offset.x, offset.y, 0);
        }

        // 极小视口中范围反向时取中心, 避免无效夹取.
        private static float Clamp(float value, float minimum, float maximum) => minimum <= maximum
            ? Mathf.Clamp(value, minimum, maximum)
            : (minimum + maximum) * .5f;

        // 尺寸变化终止捕获并约束原位移, 平移事件不会递归更新布局.
        private void Geometry(GeometryChangedEvent evt)
        {
            if (evt.oldRect.size == evt.newRect.size) return;

            Cancel();
            Move(_offset);
        }
    }
}
