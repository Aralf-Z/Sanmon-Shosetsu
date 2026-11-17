using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    internal static partial class ToolkitMixedText
    {
        private sealed class Binding : IDisposable
        {
            /// <summary> 原始文本控件, 保留 USS 选择器和交互事件. </summary>
            internal TextElement Element { get; }

            /// <summary> 完整图文原文, 不包含布局造成的省略. </summary>
            internal string Source { get; private set; } = string.Empty;

            internal bool IsDisposed { get; private set; }

            internal bool IsControl { get; private set; }

            private readonly VisualElement _content;

            private readonly IVisualElementScheduledItem _scheduled;

            private int _layoutKey;

            private int _rebuildFrame = -1; // 已挂载节点允许重建的最早帧, -1 表示尚未排队.

            private int _appearanceVersion;

            private int _languageVersion;

            private ToolkitMixedTextMode _mode;

            private VisualElement _controlOwner;

            private string _ownerClass; // 仅在释放时撤销本绑定自己添加的标记.

            private bool _dirty = true;

            internal Binding(TextElement element)
            {
                Element = element;
                _content = new VisualElement
                { name = "mixedTextContent", pickingMode = PickingMode.Ignore };
                _content.style.flexShrink = 0;
                _content.AddToClassList("toolkit-mixed-content");
                _scheduled = element.schedule.Execute(Rebuild);
                element.RegisterCallback<GeometryChangedEvent>(OnGeometry);
                element.RegisterCallback<CustomStyleResolvedEvent>(OnStyle);
                element.RegisterCallback<AttachToPanelEvent>(OnAttach);
                element.RegisterCallback<DetachFromPanelEvent>(OnDetach);
                element.generateVisualContent += OnGenerateVisualContent;
            }

            /// <summary> 同一原文不重复创建节点, 原生 text 保持为空以免显示标记. </summary>
            internal void SetText(string source, ToolkitMixedTextMode mode)
            {
                if (Source != source || _mode != mode)
                {
                    Source = source;
                    _mode = mode;
                    _dirty = true;
                }

                UpdateMode();
                Element.text = string.Empty;
                RequestRebuild();
            }

            internal void Refresh()
            {
                _dirty = true;
                RequestRebuild();
            }

            // 已挂载节点至少等到下一帧再重建, 让当前外观的 UIR 裁剪状态先完成初始化.
            private void RequestRebuild()
            {
                if (IsDisposed) return;

                if (Element.panel == null)
                {
                    Rebuild();
                    return;
                }

                if (Application.isPlaying && (_rebuildFrame < 0 || _appearanceVersion != ToolkitFactory.AppearanceVersion || _languageVersion != ToolkitLocalization.Version))
                {
                    _rebuildFrame = Time.frameCount + 1;
                    _appearanceVersion = ToolkitFactory.AppearanceVersion;
                    _languageVersion = ToolkitLocalization.Version;
                }

                _scheduled.ExecuteLater(0);
            }

            private bool TryBeginRebuild()
            {
                if (IsDisposed) return false;

                if (Element.panel != null && Application.isPlaying && _rebuildFrame > Time.frameCount)
                {
                    _scheduled.ExecuteLater(1);
                    return false;
                }

                _rebuildFrame = -1;
                return true;
            }

            private void Rebuild()
            {
                if (!TryBeginRebuild()) return;

                UpdateMode();
                if (_content.parent == null)
                {
                    Element.Add(_content);
                }

                var key = GetLayoutKey();

                if (!_dirty && _layoutKey == key) return;

                _layoutKey = key;
                _dirty = false;
                var source = ToolkitIcons.Resolve(Source);

                _content.Clear();
                if (IsControl && IsSingleIcon(source))
                {
                    BuildControl(source);
                }
                else
                {
                    BuildInline(source);
                }

                ToolkitTextOpacity.Track(_content);
            }

            private void BuildInline(string source)
            {
                var wrap = Element.resolvedStyle.whiteSpace == WhiteSpace.Normal;
                var layout = BuildLayout(Element, source, Element.contentRect.width, wrap);

                layout.AlignRows(Element.resolvedStyle.unityTextAlign);
                _content.style.width = layout.Size.x;
                _content.style.height = layout.Size.y;
                AlignContent(Element.resolvedStyle.unityTextAlign);
                foreach (var part in layout.parts)
                {
                    AddPart(part);
                }
            }

            private void BuildControl(string source)
            {
                var rectangle = Element.contentRect;
                var width = CONTROL_ICON_SIZE;
                var height = CONTROL_ICON_SIZE;

                if (Finite(rectangle.width))
                {
                    width = Mathf.Max(width, rectangle.width);
                }

                if (Finite(rectangle.height))
                {
                    height = Mathf.Max(height, rectangle.height);
                }

                _content.style.width = width;
                _content.style.height = height;
                _content.style.marginLeft = _content.style.marginRight = 0f;
                _content.style.marginTop = _content.style.marginBottom = 0f;
                var key = ToolkitIcons.tokens.Match(source).Groups[1].Value;
                var bounds = new Rect((width - CONTROL_ICON_SIZE) * .5f, (height - CONTROL_ICON_SIZE) * .5f, CONTROL_ICON_SIZE, CONTROL_ICON_SIZE);

                AddPart(new ToolkitInlineLayout.Part { icon = key, rectangle = bounds });
            }

            private void UpdateMode()
            {
                var owner = FindControl(Element);
                var indicator = Element.ClassListContains(INDICATOR_CLASS);
                var control = IsSingleIcon(Source) && _mode != ToolkitMixedTextMode.Inline && (_mode == ToolkitMixedTextMode.Control || owner != null || indicator);

                if (control == IsControl && _controlOwner == (owner ?? Element)) return;

                RemoveOwnerClass();
                IsControl = control;
                if (!control) return;

                _controlOwner = owner ?? Element;
                var className = owner != null
                    ? CONTROL_CLASS
                    : INDICATOR_CLASS;

                if (!_controlOwner.ClassListContains(className))
                {
                    _ownerClass = className;
                    _controlOwner.AddToClassList(className);
                }

                _dirty = true;
            }

            private void RemoveOwnerClass()
            {
                if (_controlOwner != null && _ownerClass != null)
                {
                    _controlOwner.RemoveFromClassList(_ownerClass);
                }

                _controlOwner = null;
                _ownerClass = null;
            }

            private int GetLayoutKey()
            {
                var style = Element.resolvedStyle;
                var box = HashCode.Combine(style.paddingLeft, style.paddingRight, style.paddingTop, style.paddingBottom, style.borderLeftWidth, style.borderRightWidth, style.borderTopWidth, style.borderBottomWidth);
                var font = ToolkitFontMetrics.GetKey(Element);
                var transform = Element.worldTransform;

                return HashCode.Combine(Element.contentRect, box, font, style.unityTextAlign, style.whiteSpace, IsControl, transform.m00, transform.m11);
            }

            private void AddPart(ToolkitInlineLayout.Part part)
            {
                VisualElement child;

                if (part.icon != null)
                {
                    child = new Image
                    { sprite = ToolkitIcons.Load(part.icon), scaleMode = ScaleMode.ScaleToFit, pickingMode = PickingMode.Ignore };
                }
                else
                {
                    child = CreateLabel(part.text);
                }

                var style = child.style;

                style.position = Position.Absolute;
                style.left = part.rectangle.x;
                style.top = part.rectangle.y;
                style.width = part.rectangle.width;
                style.height = part.rectangle.height;
                style.marginLeft = style.marginRight = style.marginTop = style.marginBottom = 0;
                style.paddingLeft = style.paddingRight = style.paddingTop = style.paddingBottom = 0;
                _content.Add(child);
            }

            private Label CreateLabel(string text)
            {
                var label = new Label(text)
                { enableRichText = false, parseEscapeSequences = false, pickingMode = PickingMode.Ignore };

                label.style.whiteSpace = WhiteSpace.NoWrap;
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.fontSize = Element.resolvedStyle.fontSize;
                label.selection.isSelectable = Element.selection.isSelectable;
                if (label.selection.isSelectable)
                {
                    label.pickingMode = PickingMode.Position;
                    label.focusable = true;
                }

                label.AddToClassList("toolkit-mixed-text");
                return label;
            }

            // 自动外边距在固定尺寸按钮内保留原文字的水平和垂直对齐.
            private void AlignContent(TextAnchor anchor)
            {
                var horizontal = (int)anchor % 3;
                var vertical = (int)anchor / 3;

                _content.style.marginLeft = horizontal > 0
                    ? new StyleLength(StyleKeyword.Auto)
                    : new StyleLength(0f);
                _content.style.marginRight = horizontal < 2
                    ? new StyleLength(StyleKeyword.Auto)
                    : new StyleLength(0f);
                _content.style.marginTop = vertical > 0
                    ? new StyleLength(StyleKeyword.Auto)
                    : new StyleLength(0f);
                _content.style.marginBottom = vertical < 2
                    ? new StyleLength(StyleKeyword.Auto)
                    : new StyleLength(0f);
            }

            private void OnGeometry(GeometryChangedEvent evt)
            {
                if (evt.newRect.size != evt.oldRect.size)
                {
                    RequestRebuild();
                }
            }

            // 仅样式变化而外框不变时也检查内容区, 绘制回调本身不修改视觉树.
            private void OnGenerateVisualContent(MeshGenerationContext context)
            {
                if (!IsDisposed && _layoutKey != GetLayoutKey())
                {
                    RequestRebuild();
                }
            }

            private void OnStyle(CustomStyleResolvedEvent evt)
            {
                _dirty = true;
                RequestRebuild();
            }

            private void OnAttach(AttachToPanelEvent evt)
            {
                _dirty = true;
                RequestRebuild();
            }

            private void OnDetach(DetachFromPanelEvent evt)
            {
                _scheduled.Pause();
                _rebuildFrame = -1;
            }

            public void Dispose()
            {
                IsDisposed = true;
                _scheduled.Pause();
                Element.UnregisterCallback<GeometryChangedEvent>(OnGeometry);
                Element.UnregisterCallback<CustomStyleResolvedEvent>(OnStyle);
                Element.UnregisterCallback<AttachToPanelEvent>(OnAttach);
                Element.UnregisterCallback<DetachFromPanelEvent>(OnDetach);
                Element.generateVisualContent -= OnGenerateVisualContent;
                RemoveOwnerClass();
                _content.RemoveFromHierarchy();
                Element.text = ToolkitIcons.PlainText(Source);
            }
        }
    }
}
