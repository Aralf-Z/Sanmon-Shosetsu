using System;
using System.Collections.Generic;
using GameConsole.UI;
using RedSaw.CommandLineInterface;

namespace GameConsole
{
    /// <summary> 使用独立日志服务的缓存, 转发原控制器的渲染和输入调用. </summary>
    internal sealed class ConsoleViewBridge : IConsoleRenderer, IConsoleInput, IStructuredConsoleRenderer
    {
        /// <summary> 界面创建前后始终使用同一个完整缓存. </summary>
        public ConsoleLogStore Logs => DevelopmentLogSystem.Logs;
        /// <summary> 始终接收带类型和堆栈的结构化日志. </summary>
        public bool AcceptsStructuredLogs => true;
        /// <summary> 未绑定或工具关闭时不处理命令输入. </summary>
        public bool IsVisible { get => _view?.IsOpen == true && _view.Renderer.IsVisible; set { if (_view != null) _view.Renderer.IsVisible = value; } }
        /// <summary> 输入焦点. </summary>
        public bool IsInputFieldFocus => IsVisible && _view.Renderer.IsInputFieldFocus;
        /// <summary> 输入文本. </summary>
        public string InputText { get => _view?.Renderer.InputText ?? string.Empty; set { if (_view != null) _view.Renderer.InputText = value; } }
        /// <summary> 光标前的输入文本. </summary>
        public string InputTextToCursor { get => _view?.Renderer.InputTextToCursor ?? string.Empty; set { if (_view != null) _view.Renderer.InputTextToCursor = value; } }
        /// <summary> 补全列表状态. </summary>
        public bool IsAlternativeOptionsActive { get => _view?.Renderer.IsAlternativeOptionsActive ?? false; set { if (_view != null) _view.Renderer.IsAlternativeOptionsActive = value; } }
        /// <summary> 补全列表. </summary>
        public List<string> AlternativeOptions { set { if (_view != null) _view.Renderer.AlternativeOptions = value; } }
        /// <summary> 补全选中项. </summary>
        public int AlternativeOptionsIndex { set { if (_view != null) _view.Renderer.AlternativeOptionsIndex = value; } }
        /// <summary> 向上选择. </summary>
        public bool MoveUp => IsVisible && _view.Input.MoveUp;
        /// <summary> 向下选择. </summary>
        public bool MoveDown => IsVisible && _view.Input.MoveDown;
        /// <summary> 聚焦控制台. </summary>
        bool IConsoleInput.Focus => IsVisible && _view.Input.Focus;
        /// <summary> 释放焦点. </summary>
        bool IConsoleInput.QuitFocus => IsVisible && _view.Input.QuitFocus;
        /// <summary> 总窗口开关只由总控处理. </summary>
        public bool ShowOrHide => false;

        private ToolkitView _view; // 初始化成功后才挂接的正式 UI.
        private Action<string> _onSubmit; // 原控制器提交回调.
        private Action<string> _onTextChanged; // 原控制器补全回调.

        /// <summary> 将唯一日志缓存交给界面, 不回放或重复生成输出记录. </summary>
        public void Attach(ToolkitView view)
        {
            _view = view;
            view.Renderer.BindOnSubmit(Submit);
            view.Renderer.BindOnTextChanged(TextChanged);
            view.Renderer.BindLogs(Logs);
        }

        /// <summary> 初始化失败或销毁时断开所有 UI 转发. </summary>
        public void Detach() => _view = null;
        /// <summary> 绑定控制器提交回调. </summary>
        public void BindOnSubmit(Action<string> callback) => _onSubmit = callback;
        /// <summary> 绑定控制器文本回调. </summary>
        public void BindOnTextChanged(Action<string> callback) => _onTextChanged = callback;
        /// <summary> 聚焦输入框. </summary>
        public void Focus() { if (IsVisible) _view.Renderer.Focus(); }
        /// <summary> 激活输入框. </summary>
        public void ActivateInput() { if (IsVisible) _view.Renderer.ActivateInput(); }
        /// <summary> 释放当前 UI 的输入焦点. </summary>
        public void QuitFocus() => _view?.Renderer.QuitFocus();
        /// <summary> 设置光标. </summary>
        public void SetInputCursorPosition(int pos) => _view?.Renderer.SetInputCursorPosition(pos);
        /// <summary> 滚动到最新输出. </summary>
        public void MoveScrollBarToEnd() => _view?.Renderer.MoveScrollBarToEnd();
        /// <summary> 输出无颜色日志. </summary>
        public void Output(string msg) => Output(msg, "#ffffff");
        /// <summary> 输出彩色日志, 隐藏 UI 时仍更新缓存. </summary>
        public void Output(string msg, string color = "#ffffff") => OutputLog(new ConsoleLogRecord(msg, msg, color));
        /// <summary> 批量输出日志. </summary>
        public void Output(string[] msgs) => Output(msgs, "#ffffff");
        /// <summary> 批量输出彩色日志. </summary>
        public void Output(string[] msgs, string color = "#ffffff") { foreach (var msg in msgs) Output(msg, color); }
        /// <summary> 同时清空缓存和可见输出. </summary>
        public void Clear()
        {
            if (_view != null) _view.Renderer.Clear();
            else DevelopmentLogSystem.Clear();
        }

        /// <summary> 原始记录直接进入唯一缓存, UI 每帧合并版本更新. </summary>
        public void OutputLog(ConsoleLogRecord record) => DevelopmentLogSystem.Append(record);

        // 关闭期间 InputField 的失焦事件不能执行命令.
        private void Submit(string text) { if (IsVisible) _onSubmit?.Invoke(text); }
        // 控制器主动更新文本时也需要回调, 以保留历史与补全语义.
        private void TextChanged(string text) => _onTextChanged?.Invoke(text);
    }
}
