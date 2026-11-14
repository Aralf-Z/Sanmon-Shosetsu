using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 以设置分区的实际内容宽度划分两列, 不依赖字段自身的固有尺寸. </summary>
    internal sealed class ToolkitOptionsLayout : IDisposable
    {
        private const float NAME_RATIO = .4f; // 分区内边距以内, 名称占四成, 控件占六成.

        private readonly List<Section> _sections = new(); // 仅处理 Options, 不影响浮标设置弹窗.
        private bool _dirty = true; // 多次几何变化合并到每帧一次.
        private int _appearanceVersion = -1; // 风格的边框和内边距也会改变有效宽度.

        /// <summary> 缓存分区及原生字段的真实标签和输入节点, 不移动或重建输入控件. </summary>
        internal ToolkitOptionsLayout(VisualElement root)
        {
            foreach (var panel in root.Query<VisualElement>(className: "settings-section").ToList())
            {
                var host = panel.Q<VisualElement>(className: "settings-section-rows");
                if (host == null) continue;
                var section = new Section(panel, host);
                foreach (var row in host.Children())
                {
                    if (!row.ClassListContains("setting-row")) continue;
                    VisualElement name = null;
                    VisualElement value = null;
                    // 使用实际 hierarchy, 不受原生字段 contentContainer 重定向影响.
                    for (var index = 0; index < row.hierarchy.childCount; index++)
                    {
                        var child = row.hierarchy[index];
                        if (child.ClassListContains("unity-base-field__label")) name = child;
                        else if (child.ClassListContains("unity-base-field__input") || child.ClassListContains("unity-base-slider__input") || child.ClassListContains("ui-style-picker")) value = child;
                    }
                    // 清空和查看历史是独立操作行, 没有名称列, 保留原来的布局.
                    if (name == null || value == null) continue;
                    row.RemoveFromClassList("unity-base-field__aligned");
                    section.rows.Add(new Row(row, name, value));
                }
                panel.RegisterCallback<GeometryChangedEvent>(Geometry);
                _sections.Add(section);
            }
        }

        /// <summary> 切页后在新几何就绪时重新应用, 不立即读取隐藏页的零尺寸. </summary>
        internal void Invalidate() => _dirty = true;

        /// <summary> 按分区宽度而非行的固有宽度计算, 控件右边缘与分区内边缘对齐. </summary>
        internal void Tick()
        {
            if (_appearanceVersion != ToolkitFactory.AppearanceVersion)
            {
                _appearanceVersion = ToolkitFactory.AppearanceVersion;
                _dirty = true;
            }
            if (!_dirty) return;
            _dirty = false;
            foreach (var section in _sections)
            {
                var width = section.panel.contentRect.width;
                if (width <= 0 || float.IsNaN(width) || float.IsInfinity(width)) { _dirty = true; continue; }
                SetWidth(section.host, width);
                foreach (var row in section.rows)
                {
                    SetWidth(row.root, width);
                    row.root.style.flexDirection = FlexDirection.Row;
                    row.root.style.marginLeft = 0;
                    row.root.style.marginRight = 0;
                    var available = Mathf.Max(0, width - Insets(row.root.resolvedStyle));
                    var nameWidth = available * NAME_RATIO;
                    SetWidth(row.name, nameWidth);
                    SetWidth(row.value, available - nameWidth);
                    row.name.style.marginLeft = row.name.style.marginRight = 0;
                    row.value.style.marginLeft = row.value.style.marginRight = 0;
                }
            }
        }

        /// <summary> 释放分区几何监听, 不修改设置值. </summary>
        public void Dispose()
        {
            foreach (var section in _sections) section.panel.UnregisterCallback<GeometryChangedEvent>(Geometry);
            _sections.Clear();
        }

        // 宽度是面板单位, 已由 UI Toolkit 处理屏幕缩放. 数值内联只用于动态几何约束.
        private static void SetWidth(VisualElement element, float width)
        {
            element.style.width = width;
            element.style.minWidth = 0;
            element.style.maxWidth = width;
            element.style.flexBasis = StyleKeyword.Auto;
            element.style.flexGrow = 0;
            element.style.flexShrink = 0;
        }

        // 字段的边框和内边距属于行本身, 不重复计入两列宽度.
        private static float Insets(IResolvedStyle style) => style.paddingLeft + style.paddingRight + style.borderLeftWidth + style.borderRightWidth;

        // 位移不会改变分列, 尺寸变化才要求重新计算.
        private void Geometry(GeometryChangedEvent evt) { if (evt.oldRect.size != evt.newRect.size) _dirty = true; }

        private sealed class Section
        {
            /// <summary> 分区本身是唯一的宽度依据. </summary>
            internal readonly VisualElement panel;
            /// <summary> 所有设置行共享的内容宿主. </summary>
            internal readonly VisualElement host;
            /// <summary> 带有名称和控件的设置行. </summary>
            internal readonly List<Row> rows = new();
            /// <summary> 保留分区和现有行宿主. </summary>
            internal Section(VisualElement panel, VisualElement host) { this.panel = panel; this.host = host; }
        }

        private sealed class Row
        {
            /// <summary> 原生字段或自定义下拉框行. </summary>
            internal readonly VisualElement root;
            /// <summary> 左侧完整名称槽位. </summary>
            internal readonly VisualElement name;
            /// <summary> 右侧下拉框, 输入框, 滑动条或开关. </summary>
            internal readonly VisualElement value;
            /// <summary> 缓存节点引用, 后续只调整几何. </summary>
            internal Row(VisualElement root, VisualElement name, VisualElement value) { this.root = root; this.name = name; this.value = value; }
        }
    }
}
