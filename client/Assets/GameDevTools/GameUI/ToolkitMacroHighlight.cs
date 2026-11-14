using System;
using System.Collections.Generic;
using System.Text;
using GameConsole.Extension;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    /// <summary> 原生输入负责编辑, 显式 ScrollView 负责唯一有效滚动范围. </summary>
    internal sealed class ToolkitMacroHighlight : IDisposable
    {
        private readonly TextField _body; // 保存和复制始终读取纯文本.
        private readonly TextElement _text; // 仅依赖原生文字节点, 不要求内部 ScrollView 存在.
        private readonly Label _overlay; // 不参与拾取和布局的着色层.
        private readonly Label _numbers; // 固定在横向滚动范围之外.
        private readonly ScrollView _scroll; // UXML 显式声明的正文滚动容器.
        private readonly ScrollView _inner; // 可选原生容器, 内容完整撑开后没有有效滚动范围.
        private readonly VisualElement _numberViewport; // 行号裁剪区域.
        private readonly StyleLength[] _originalSize; // 输入框和文字尺寸覆盖在销毁时恢复.
        private readonly PickingMode _originalPicking; // 原生拾取状态.
        private readonly bool _originalRichText; // 原生显示状态.
        private readonly bool _originalFocus; // 原生全选设置.
        private readonly bool _originalMouseUp; // 同上.
        private readonly ScrollerVisibility _innerVertical; // 可选原生滚动设置.
        private readonly ScrollerVisibility _innerHorizontal; // 同上.
        private readonly HashSet<int> _errors = new(); // 当前错误行.
        private string _source; // 上次着色正文.
        private bool _dirty = true; // 合并内容和配色变化.
        private bool _composing; // 输入法组合期间显示原生文字.
        private string _composition; // 组合串增长也要重新测量, 不写入正文.
        private bool _editing; // 再次聚焦时恢复光标可见性.
        private bool _resize = true; // 合并测量和布局变化.
        private bool _caretPending; // 布局完成后保持光标可见.
        private int _layoutFrame; // 避免使用尚未排版的新尺寸.
        private int _cursor = -1; // 只有编辑光标变化才跟随, 滚轮和滑块不回跳.
        private int _selection = -1; // 拖选同样保留原生选择.
        private int _appearance = -1; // 字体或风格变化时重新测量.
        private float _fontSize; // 字体初次就绪时补测量.
        private float _lineHeight = 20; // 原生文字测得的行距.
        private bool _disposed; // 防止重复清理.

        /// <summary> 查找完成后才注册事件, 两个 Unity 版本共用相同输入与滚动流程. </summary>
        internal ToolkitMacroHighlight(TextField body, Label numbers, ScrollView scroll)
        {
            _body = body;
            _numbers = numbers;
            _scroll = scroll ?? throw new ArgumentNullException(nameof(scroll));
            var input = body.Q<VisualElement>(className: TextField.inputUssClassName);
            _text = input?.Q<TextElement>() ?? throw new InvalidOperationException(ToolkitLocalization.Get("ui.MacroHighlight.labels.2"));
            _inner = input.Q<ScrollView>();
            _numberViewport = numbers.parent;
            _originalSize = new[] { body.style.width, body.style.height, _text.style.minWidth, _text.style.minHeight };
            _originalPicking = _text.pickingMode;
            _originalRichText = _text.enableRichText;
            _originalFocus = body.selectAllOnFocus;
            _originalMouseUp = body.selectAllOnMouseUp;
            _innerVertical = _inner?.verticalScrollerVisibility ?? ScrollerVisibility.Hidden;
            _innerHorizontal = _inner?.horizontalScrollerVisibility ?? ScrollerVisibility.Hidden;
            _overlay = new Label { name = "macroHighlight", pickingMode = PickingMode.Ignore, enableRichText = true, focusable = false };
            try
            {
                _numbers.enableRichText = true;
                _scroll.mode = ScrollViewMode.VerticalAndHorizontal;
                _scroll.touchScrollBehavior = ScrollView.TouchScrollBehavior.Clamped;
                if (_inner != null)
                {
                    _inner.AddToClassList("macro-inner-scroll");
                    _inner.verticalScrollerVisibility = ScrollerVisibility.Hidden;
                    _inner.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
                }
                body.selectAllOnFocus = false;
                body.selectAllOnMouseUp = false;
                _text.pickingMode = PickingMode.Position;
                _text.enableRichText = false;
                _text.AddToClassList("macro-native-text");
                _overlay.AddToClassList("macro-highlight");
                _text.Add(_overlay);
                _text.RegisterCallback<GeometryChangedEvent>(OnGeometry);
                _scroll.contentViewport.RegisterCallback<GeometryChangedEvent>(OnViewportGeometry);
                _scroll.verticalScroller.valueChanged += OnScroll;
                Config.e_onChanged += Changed;
            }
            catch { Dispose(); throw; }
        }

        /// <summary> 校验只影响显示, 不修改输入内容. </summary>
        internal void SetDiagnostics(IEnumerable<MacroDiagnostic> errors)
        {
            _errors.Clear();
            foreach (var error in errors) if (error.Line > 0) _errors.Add(error.Line);
            _dirty = true;
        }

        /// <summary> 保留原生光标和输入法, 测量仅在正文或外观变化时进行. </summary>
        internal void Tick()
        {
            var focused = _body.panel?.focusController.focusedElement as VisualElement;
            var editing = focused != null && (_body == focused || _body.Contains(focused));
            if (editing && !_editing) _caretPending = true;
            _editing = editing;
            var composition = editing ? Input.compositionString : string.Empty;
            if (_composition != composition) { _composition = composition; _resize = true; if (editing) _caretPending = true; }
            var composing = !string.IsNullOrEmpty(composition);
            if (_composing != composing)
            {
                _composing = composing;
                _text.EnableInClassList("macro-composing", composing);
                _overlay.EnableInClassList("is-hidden", composing);
                _resize = true;
            }
            if (_source != _body.value)
            {
                _source = _body.value;
                _dirty = _resize = true;
                if (editing) _caretPending = true; else _scroll.scrollOffset = Vector2.zero;
            }
            if (_appearance != ToolkitFactory.AppearanceVersion || _fontSize != _text.resolvedStyle.fontSize)
            {
                _appearance = ToolkitFactory.AppearanceVersion;
                _fontSize = _text.resolvedStyle.fontSize;
                _resize = true;
            }
            if (_dirty) ColorLines();
            if (_resize) { if (editing) _caretPending = true; Resize(); }
            if (_inner != null && _inner.scrollOffset != Vector2.zero) _inner.scrollOffset = Vector2.zero;
            var selection = _text.selection;
            if (_cursor != selection.cursorIndex || _selection != selection.selectIndex)
            {
                _cursor = selection.cursorIndex;
                _selection = selection.selectIndex;
                if (editing) _caretPending = true;
            }
            if (editing && _caretPending && Time.frameCount > _layoutFrame) RevealCaret();
            Align();
            SyncGutter();
        }

        /// <summary> 清理事件和覆盖, 重试不会重复注册高亮或输入监听. </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Config.e_onChanged -= Changed;
            _text.UnregisterCallback<GeometryChangedEvent>(OnGeometry);
            _scroll.contentViewport.UnregisterCallback<GeometryChangedEvent>(OnViewportGeometry);
            _scroll.verticalScroller.valueChanged -= OnScroll;
            _overlay.RemoveFromHierarchy();
            _body.selectAllOnFocus = _originalFocus;
            _body.selectAllOnMouseUp = _originalMouseUp;
            _body.style.width = _originalSize[0];
            _body.style.height = _originalSize[1];
            _text.style.minWidth = _originalSize[2];
            _text.style.minHeight = _originalSize[3];
            _text.pickingMode = _originalPicking;
            _text.enableRichText = _originalRichText;
            _text.RemoveFromClassList("macro-native-text");
            _text.RemoveFromClassList("macro-composing");
            if (_inner == null) return;
            _inner.RemoveFromClassList("macro-inner-scroll");
            _inner.verticalScrollerVisibility = _innerVertical;
            _inner.horizontalScrollerVisibility = _innerHorizontal;
        }

        // 使用原生字体测量, 尾部哨兵保留空正文和最后一次换行的行高.
        private void Resize()
        {
            var viewport = _scroll.contentViewport.contentRect.size;
            if (!Valid(viewport.x) || !Valid(viewport.y) || viewport.x <= 0 || viewport.y <= 0) return;
            var measured = _text.MeasureTextSize((_body.value ?? string.Empty) + (_composing ? Input.compositionString : string.Empty) + "\u200b", 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
            var single = _text.MeasureTextSize("M", 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
            var two = _text.MeasureTextSize("M\nM", 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
            if (!Valid(measured.x) || !Valid(measured.y) || single.y <= 0) return;
            _lineHeight = Mathf.Max(single.y, two.y - single.y);
            var input = _body.Q<VisualElement>(className: TextField.inputUssClassName).resolvedStyle;
            var horizontal = input.paddingLeft + input.paddingRight + input.borderLeftWidth + input.borderRightWidth + 8;
            var vertical = input.paddingTop + input.paddingBottom + input.borderTopWidth + input.borderBottomWidth + 8;
            var width = Mathf.Max(viewport.x, Mathf.Ceil(measured.x + horizontal));
            var height = Mathf.Max(viewport.y, Mathf.Ceil(measured.y + vertical));
            _body.style.width = width;
            _body.style.height = height;
            _text.style.minWidth = Mathf.Max(1, width - horizontal + 8);
            _text.style.minHeight = Mathf.Max(1, height - vertical + 8);
            _layoutFrame = Time.frameCount;
            _resize = false;
        }

        // 不使用字符数估算光标位置, 不在普通滚动时写回偏移.
        private void RevealCaret()
        {
            var cursor = _text.selection.cursorPosition;
            var point = _scroll.contentContainer.WorldToLocal(_text.LocalToWorld(cursor));
            var size = _scroll.contentViewport.contentRect.size;
            if (!Valid(point.x) || !Valid(point.y)) return;
            var offset = _scroll.scrollOffset;
            offset.x = RevealAxis(offset.x, point.x, 8, size.x);
            offset.y = RevealAxis(offset.y, point.y, _lineHeight, size.y);
            offset.x = Mathf.Clamp(offset.x, 0, Mathf.Max(0, _scroll.horizontalScroller.highValue));
            offset.y = Mathf.Clamp(offset.y, 0, Mathf.Max(0, _scroll.verticalScroller.highValue));
            if ((_scroll.scrollOffset - offset).sqrMagnitude > .01f) _scroll.scrollOffset = offset;
            _caretPending = false;
        }

        private static float RevealAxis(float offset, float start, float length, float viewport) => start < offset ? start : start + length > offset + viewport ? start + length - viewport : offset; // 仅移动保持光标可见所需距离.
        private static bool Valid(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private void Align() { var rect = _text.contentRect; _overlay.style.left = rect.x; _overlay.style.top = rect.y; _overlay.style.width = rect.width; _overlay.style.height = rect.height; }
        private void SyncGutter()
        {
            if (_scroll.panel == null || _numberViewport.parent == null) return;
            var viewport = _scroll.contentViewport;
            var origin = _numberViewport.parent.WorldToLocal(viewport.LocalToWorld(viewport.contentRect.position));
            var text = _numberViewport.parent.WorldToLocal(_text.LocalToWorld(_text.contentRect.position));
            _numberViewport.style.top = origin.y;
            _numberViewport.style.height = viewport.contentRect.height;
            _numbers.style.top = text.y - origin.y;
            _numbers.style.translate = new Translate(0, 0, 0);
        }
        private void OnGeometry(GeometryChangedEvent evt) { Align(); SyncGutter(); }
        private void OnViewportGeometry(GeometryChangedEvent evt) { _resize = true; }
        private void OnScroll(float value) => SyncGutter();
        private void Changed() => _dirty = true;

        // 色标只写入不可拾取的显示层, 字符串和剪贴板仍由原生纯文本输入层保留.
        private void ColorLines()
        {
            var body = new StringBuilder();
            var numbers = new StringBuilder();
            var lines = (_source ?? string.Empty).Replace("\r\n", "\n").Split('\n');
            for (var index = 0; index < Math.Max(Config.DEFAULT_MACRO_BODY_LINES, lines.Length); index++)
            {
                if (index > 0) { body.Append('\n'); numbers.Append('\n'); }
                var error = _errors.Contains(index + 1);
                var line = index < lines.Length ? lines[index] : string.Empty;
                var keyword = MacroText.Keyword(line);
                var color = error ? MacroSyntaxColors.error : keyword == "WaitFor" ? Config.MacroWaitForColor : keyword == "Repeat" || keyword == "EndRepeat" ? Config.MacroRepeatColor : MacroSyntaxColors.text;
                body.Append("<color=").Append(color).Append('>').Append(line.Replace("<", "<noparse><</noparse>")).Append("</color>");
                if (error) numbers.Append("<color=").Append(MacroSyntaxColors.error).Append('>');
                numbers.Append(error ? "! " : string.Empty).Append(index + 1);
                if (error) numbers.Append("</color>");
            }
            _overlay.text = body.ToString();
            _numbers.text = numbers.ToString();
            _dirty = false;
        }
    }
}
