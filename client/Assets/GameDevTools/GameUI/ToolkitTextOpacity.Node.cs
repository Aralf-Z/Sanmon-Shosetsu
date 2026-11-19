using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    internal sealed partial class ToolkitTextOpacity
    {
        private sealed class Node : IDisposable
        {
            private static readonly CustomStyleProperty<float> LOCAL_OPACITY = new("--dev-opacity-local");

            private static readonly CustomStyleProperty<Color> SELECTION_COLOR = new("--unity-selection-color");

            private static readonly CustomStyleProperty<Color> CURSOR_COLOR = new("--unity-cursor-color");

            private static readonly string[] _channels =
            { "background", "border-top", "border-right", "border-bottom", "border-left", "image-tint" };

            private static readonly CustomStyleProperty<Color>[] _colors = CreateColors(); // 保留 USS 变量解析及伪状态颜色.

            private static readonly CustomStyleProperty<float>[] _specified = CreateFlags();

            internal readonly VisualElement element;

            private readonly ToolkitTextOpacity _owner;

            private readonly StyleFloat _originalOpacity;

            private readonly StyleColor[] _original;

            private readonly Color[] _fallback = new Color[6]; // 没有显式主题颜色的原生控件保留其基础颜色.

            private readonly Color[] _raw = new Color[6];

            private readonly Image _image;

            private readonly Color _imageTint;

            private readonly bool _text; // Button 也属于 TextElement, 其子 Label 不再重复乘全局 alpha.

            private readonly ITextSelection _selection;

            private readonly Color _originalSelection;

            private readonly Color _originalCursor;

            private readonly IVisualElementScheduledItem _resolve; // 附加后仅补一次初始样式解析.

            private Color _selectionFallback;

            private Color _cursorFallback;

            private Color _selectionColor;

            private Color _cursorColor;

            private float _backgroundOpacity = 1f;
            private float _localOpacity = 1f;

            private float _branchOpacity = 1f;

            private float _textAlpha = 1f; // 本节点之后实际继承的文字层 alpha.

            private bool _resolved; // 避免未解析的基础颜色覆盖正常样式.

            private bool _suspended;

            private bool _disposed; // 忽略延迟解析和释放中的样式事件.

            internal Node(ToolkitTextOpacity owner, VisualElement target)
            {
                _owner = owner;
                element = target;
                _text = target is TextElement;
                _selection = (target as TextElement)?.selection;
                _originalSelection = _selection?.selectionColor ?? Color.clear;
                _originalCursor = _selection?.cursorColor ?? Color.clear;
                _image = target as Image;
                _imageTint = _image != null
                    ? _image.tintColor
                    : Color.white;
                var style = target.style;

                _originalOpacity = style.opacity;
                _original = new[]
                { style.backgroundColor, style.borderTopColor, style.borderRightColor, style.borderBottomColor, style.borderLeftColor, style.unityBackgroundImageTintColor };
                target.RegisterCallback<CustomStyleResolvedEvent>(OnStyle);
                target.RegisterCallback<DetachFromPanelEvent>(OnDetach);
                _resolve = target.schedule.Execute(Resolve);
                try
                {
                    Apply();
                }
                catch
                {
                    Dispose();
                    throw;
                }
            }

            internal void Apply()
            {
                if (_disposed || _suspended) return;

                Node parent = null;

                for (var ancestor = element.hierarchy.parent; ancestor != null; ancestor = ancestor.hierarchy.parent)
                {
                    if (_owner._nodes.TryGetValue(ancestor, out parent))
                    {
                        break;
                    }
                }

                _branchOpacity = (parent?._branchOpacity ?? 1f) * _localOpacity;
                var inherited = parent?._textAlpha ?? 1f;

                _textAlpha = _text
                    ? Mathf.Max(MINIMUM_TEXT_OPACITY, _owner._opacity * _branchOpacity)
                    : inherited;
                element.style.opacity = _text
                    ? _textAlpha / inherited
                    : 1f;
                if (!_resolved) return;

                var surface = Mathf.Clamp01(_owner._opacity * _branchOpacity / _textAlpha);
                var style = element.style;

                style.backgroundColor = Weight(_raw[0], surface * _backgroundOpacity);
                style.borderTopColor = Weight(_raw[1], surface);
                style.borderRightColor = Weight(_raw[2], surface);
                style.borderBottomColor = Weight(_raw[3], surface);
                style.borderLeftColor = Weight(_raw[4], surface);
                style.unityBackgroundImageTintColor = Weight(_raw[5], surface);
                if (element is ToolkitHeaderPattern)
                {
                    element.MarkDirtyRepaint();
                }

                if (_image != null)
                {
                    _image.tintColor = Weight(_imageTint, surface);
                }

                if (_selection != null)
                {
                    _selection.selectionColor = Weight(_selectionColor, surface);
                    _selection.cursorColor = Weight(_cursorColor, surface);
                }
            }

            internal void SetBackgroundOpacity(float opacity)
            {
                if (_backgroundOpacity == opacity) return;

                _backgroundOpacity = opacity;
                Apply();
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
                if (_image != null)
                {
                    _image.tintColor = _imageTint;
                }

                if (_selection != null)
                {
                    _selection.selectionColor = _originalSelection;
                    _selection.cursorColor = _originalCursor;
                }
            }

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

                _selectionColor = custom.TryGetValue(SELECTION_COLOR, out var selection)
                    ? selection
                    : _selectionFallback;
                _cursorColor = custom.TryGetValue(CURSOR_COLOR, out var cursor)
                    ? cursor
                    : _cursorFallback;
                _localOpacity = custom.TryGetValue(LOCAL_OPACITY, out var opacity)
                    ? Mathf.Clamp01(opacity)
                    : 1f;
                for (var index = 0; index < _raw.Length; index++)
                {
                    _raw[index] = custom.TryGetValue(_specified[index], out var enabled) && enabled > .5f && custom.TryGetValue(_colors[index], out var color)
                        ? color
                        : _fallback[index];
                }

                if (previous != _localOpacity)
                {
                    _owner.RefreshTree(element);
                }
                else
                {
                    Apply();
                }
            }

            private void OnStyle(CustomStyleResolvedEvent evt) => Resolve();

            private void OnDetach(DetachFromPanelEvent evt) => _owner.Release(this); // 不保留已删除的日志堆栈和历史按钮.

            private static Color Weight(Color color, float alpha)
            {
                color.a *= alpha;
                return color;
            } // 保留主题原始半透明表面.

            private static CustomStyleProperty<Color>[] CreateColors()
            {
                var result = new CustomStyleProperty<Color>[_channels.Length];

                for (var index = 0; index < result.Length; index++)
                {
                    result[index] = new CustomStyleProperty<Color>("--dev-opacity-" + _channels[index]);
                }

                return result;
            }

            private static CustomStyleProperty<float>[] CreateFlags()
            {
                var result = new CustomStyleProperty<float>[_channels.Length];

                for (var index = 0; index < result.Length; index++)
                {
                    result[index] = new CustomStyleProperty<float>("--dev-opacity-" + _channels[index] + "-set");
                }

                return result;
            }
        }
    }
}
