using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 帮助标题栏拖动与位置复位, 使用透明层局部坐标, 不写入配置. </summary>
    internal sealed class ToolkitHelpPlacement : IDisposable
    {
        private readonly VisualElement _backdrop; // 已扣除外侧安全间距的可用区域.
        private readonly VisualElement _dialog; // 自然布局始终居中, 位移只用于用户拖动.
        private readonly VisualElement _header; // 关闭按钮不参与拖动.
        private readonly ToolkitPointerManipulator _drag; // 复用工具的捕获与取消逻辑.
        private Vector2 _offset; // 相对居中位置的位移.
        private Vector2 _beforeDrag; // 取消拖动时还原的位移.

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

        /// <summary> 每次从帮助入口打开时恢复默认居中位置. </summary>
        internal void Reset() { Cancel(); Move(Vector2.zero); }

        /// <summary> 关闭或隐藏时释放指针, 不影响正文和目录选择. </summary>
        internal void Cancel() => _drag.Cancel();

        /// <summary> 释放捕获和几何订阅. </summary>
        public void Dispose()
        {
            Cancel();
            _header.RemoveManipulator(_drag);
            _backdrop.UnregisterCallback<GeometryChangedEvent>(Geometry);
            _dialog.UnregisterCallback<GeometryChangedEvent>(Geometry);
        }

        // 保持帮助窗口可关闭, 不将主工具允许出屏的规则应用到帮助.
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
        private static float Clamp(float value, float minimum, float maximum) => minimum <= maximum ? Mathf.Clamp(value, minimum, maximum) : (minimum + maximum) * .5f;

        // 尺寸变化终止捕获并约束原位移, 平移事件不会递归更新布局.
        private void Geometry(GeometryChangedEvent evt)
        {
            if (evt.oldRect.size == evt.newRect.size) return;
            Cancel();
            Move(_offset);
        }
    }
}
