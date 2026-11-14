using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    internal sealed partial class ToolkitTextOpacity
    {
        // 原始颜色来自独立 USS 自定义属性, 悬停和选中仍由原选择器决定, 不读取自己覆盖后的颜色.
        private sealed class Node : IDisposable
        {
            private static readonly CustomStyleProperty<float> LOCAL_OPACITY = new("--dev-opacity-local");
            private static readonly CustomStyleProperty<Color> SELECTION_COLOR = new("--unity-selection-color");
            private static readonly CustomStyleProperty<Color> CURSOR_COLOR = new("--unity-cursor-color");
            private static readonly string[] _channels = { "background", "border-top", "border-right", "border-bottom", "border-left", "image-tint" }; // 顺序与原生颜色属性对应.
            private static readonly CustomStyleProperty<Color>[] _colors = CreateColors(); // 保留 USS 变量解析及伪状态颜色.
            private static readonly CustomStyleProperty<float>[] _specified = CreateFlags(); // 自定义属性默认继承, 每个节点由通用样式重置标记.

            /// <summary> 当前管理的实际节点. </summary>
            internal readonly VisualElement element;

            private readonly ToolkitTextOpacity _owner; // 不经过 Controller 或其他项目单例.
            private readonly StyleFloat _originalOpacity; // 释放时恢复原 inline 值及关键字.
            private readonly StyleColor[] _original; // 同上, 不把 resolved 值永久写回主题.
            private readonly Color[] _fallback = new Color[6]; // 没有显式主题颜色的原生控件保留其基础颜色.
            private readonly Color[] _raw = new Color[6]; // 最近一次 USS 解析出的未加权颜色.
            private readonly Image _image; // 图片内容与 background-image 分开加权.
            private readonly Color _imageTint; // Image 的原始 tint, 普通预设不修改这个值.
            private readonly bool _text; // Button 也属于 TextElement, 其子 Label 不再重复乘全局 alpha.
            private readonly ITextSelection _selection; // 光标和选区属于非文字绘制, 通过公开接口分别加权.
            private readonly Color _originalSelection; // 恢复原生选择样式.
            private readonly Color _originalCursor; // 恢复原生光标样式.
            private readonly IVisualElementScheduledItem _resolve; // 附加后仅补一次初始样式解析.
            private Color _selectionFallback; // 没有主题覆盖时沿用原生选择颜色.
            private Color _cursorFallback; // 没有主题覆盖时沿用原生光标颜色.
            private Color _selectionColor; // 未加权的主题颜色.
            private Color _cursorColor; // 同上.
            private float _localOpacity = 1f; // disabled 和拖拽来源淡化的原始值.
            private float _branchOpacity = 1f; // 不含全局值的原始父子淡化乘积.
            private float _textAlpha = 1f; // 本节点之后实际继承的文字层 alpha.
            private bool _resolved; // 避免未解析的基础颜色覆盖正常样式.
            private bool _suspended; // 脱离面板时释放事件, 池项再挂载复用数组和调度项.
            private bool _disposed; // 忽略延迟解析和释放中的样式事件.

            /// <summary> 保存节点原始样式并登记一次原生样式变更通知. </summary>
            internal Node(ToolkitTextOpacity owner, VisualElement target)
            {
                _owner = owner;
                element = target;
                _text = target is TextElement;
                _selection = (target as TextElement)?.selection;
                _originalSelection = _selection?.selectionColor ?? Color.clear;
                _originalCursor = _selection?.cursorColor ?? Color.clear;
                _image = target as Image;
                _imageTint = _image != null ? _image.tintColor : Color.white;
                var style = target.style;
                _originalOpacity = style.opacity;
                _original = new[] { style.backgroundColor, style.borderTopColor, style.borderRightColor, style.borderBottomColor, style.borderLeftColor, style.unityBackgroundImageTintColor };
                target.RegisterCallback<CustomStyleResolvedEvent>(OnStyle);
                target.RegisterCallback<DetachFromPanelEvent>(OnDetach);
                _resolve = target.schedule.Execute(Resolve);
                try { Apply(); }
                catch { Dispose(); throw; }
            }

            /// <summary> 父先子后计算, 表面颜色补偿最近文字祖先的 alpha. </summary>
            internal void Apply()
            {
                if (_disposed || _suspended) return;
                Node parent = null;
                for (var ancestor = element.hierarchy.parent; ancestor != null; ancestor = ancestor.hierarchy.parent)
                    if (_owner._nodes.TryGetValue(ancestor, out parent)) break;
                _branchOpacity = (parent?._branchOpacity ?? 1f) * _localOpacity;
                var inherited = parent?._textAlpha ?? 1f;
                _textAlpha = _text ? Mathf.Max(MINIMUM_TEXT_OPACITY, _owner._opacity * _branchOpacity) : inherited;
                element.style.opacity = _text ? _textAlpha / inherited : 1f;
                if (!_resolved) return;
                var surface = Mathf.Clamp01(_owner._opacity * _branchOpacity / _textAlpha);
                var style = element.style;
                style.backgroundColor = Weight(_raw[0], surface);
                style.borderTopColor = Weight(_raw[1], surface);
                style.borderRightColor = Weight(_raw[2], surface);
                style.borderBottomColor = Weight(_raw[3], surface);
                style.borderLeftColor = Weight(_raw[4], surface);
                style.unityBackgroundImageTintColor = Weight(_raw[5], surface);
                if (element is ToolkitHeaderPattern) element.MarkDirtyRepaint();
                if (_image != null) _image.tintColor = Weight(_imageTint, surface);
                if (_selection != null)
                {
                    _selection.selectionColor = Weight(_selectionColor, surface);
                    _selection.cursorColor = Weight(_cursorColor, surface);
                }
            }

            /// <summary> 脱离时恢复原样式, 日志池项不重复分配状态. </summary>
            internal void Suspend()
            {
                if (_disposed || _suspended) return;
                _suspended = true;
                _resolve.Pause();
                element.UnregisterCallback<CustomStyleResolvedEvent>(OnStyle);
                element.UnregisterCallback<DetachFromPanelEvent>(OnDetach);
                var style = element.style;
                style.opacity = _originalOpacity;
                style.backgroundColor = _original[0];
                style.borderTopColor = _original[1];
                style.borderRightColor = _original[2];
                style.borderBottomColor = _original[3];
                style.borderLeftColor = _original[4];
                style.unityBackgroundImageTintColor = _original[5];
                if (_image != null) _image.tintColor = _imageTint;
                if (_selection != null)
                {
                    _selection.selectionColor = _originalSelection;
                    _selection.cursorColor = _originalCursor;
                }
            }

            /// <summary> 池项重挂载后重新取得未加权颜色, 支持离屏期间的主题切换. </summary>
            internal void Resume()
            {
                if (_disposed || !_suspended) return;
                _suspended = false;
                _resolved = false;
                element.RegisterCallback<CustomStyleResolvedEvent>(OnStyle);
                element.RegisterCallback<DetachFromPanelEvent>(OnDetach);
                _resolve.ExecuteLater(0);
                Apply();
            }

            public void Dispose()
            {
                if (_disposed) return;
                Suspend();
                _disposed = true;
            }

            // 只在初次附加或 USS 状态改变时读取颜色, 不在普通日志更新和滚动时遍历.
            private void Resolve()
            {
                if (_disposed || _suspended || element.panel == null) return;
                if (!_resolved)
                {
                    var style = element.resolvedStyle;
                    _fallback[0] = style.backgroundColor;
                    _fallback[1] = style.borderTopColor;
                    _fallback[2] = style.borderRightColor;
                    _fallback[3] = style.borderBottomColor;
                    _fallback[4] = style.borderLeftColor;
                    _fallback[5] = style.unityBackgroundImageTintColor;
                    _selectionFallback = _selection?.selectionColor ?? Color.clear;
                    _cursorFallback = _selection?.cursorColor ?? Color.clear;
                    _resolved = true;
                }
                var custom = element.customStyle;
                var previous = _localOpacity;
                _selectionColor = custom.TryGetValue(SELECTION_COLOR, out var selection) ? selection : _selectionFallback;
                _cursorColor = custom.TryGetValue(CURSOR_COLOR, out var cursor) ? cursor : _cursorFallback;
                _localOpacity = custom.TryGetValue(LOCAL_OPACITY, out var opacity) ? Mathf.Clamp01(opacity) : 1f;
                for (var index = 0; index < _raw.Length; index++)
                    _raw[index] = custom.TryGetValue(_specified[index], out var enabled) && enabled > .5f && custom.TryGetValue(_colors[index], out var color) ? color : _fallback[index];
                if (previous != _localOpacity) _owner.RefreshTree(element);
                else Apply();
            }

            private void OnStyle(CustomStyleResolvedEvent evt) => Resolve(); // 状态色与实际样式在同一布局阶段提交.
            private void OnDetach(DetachFromPanelEvent evt) => _owner.Release(this); // 不保留已删除的日志堆栈和历史按钮.
            private static Color Weight(Color color, float alpha) { color.a *= alpha; return color; } // 保留主题原始半透明表面.
            private static CustomStyleProperty<Color>[] CreateColors()
            {
                var result = new CustomStyleProperty<Color>[_channels.Length];
                for (var index = 0; index < result.Length; index++) result[index] = new CustomStyleProperty<Color>("--dev-opacity-" + _channels[index]);
                return result;
            }
            private static CustomStyleProperty<float>[] CreateFlags()
            {
                var result = new CustomStyleProperty<float>[_channels.Length];
                for (var index = 0; index < result.Length; index++) result[index] = new CustomStyleProperty<float>("--dev-opacity-" + _channels[index] + "-set");
                return result;
            }
        }
    }
}
