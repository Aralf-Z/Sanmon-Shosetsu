using System;
using System.Collections.Generic;
using GameConsole.UI;
using RedSaw.CommandLineInterface;

namespace GameConsole
{
    /// <summary> 使用独立日志服务的缓存, 转发原控制器的渲染和输入调用. </summary>
    internal sealed class ConsoleViewBridge : IConsoleRenderer, IConsoleInput, IStructuredConsoleRenderer
    {
        public ConsoleLogStore Logs => DevelopmentLogSystem.Logs;

        public bool AcceptsStructuredLogs => true;

        public bool IsVisible
        {
            get => _view?.IsOpen == true && _view.Renderer.IsVisible;
            set
            {
                if (_view != null)
                {
                    _view.Renderer.IsVisible = value;
                }
            }
        }

        public bool IsInputFieldFocus => IsVisible && _view.Renderer.IsInputFieldFocus;

        public string InputText
        {
            get => _view?.Renderer.InputText ?? string.Empty;
            set
            {
                if (_view != null)
                {
                    _view.Renderer.InputText = value;
                }
            }
        }

        public string InputTextToCursor
        {
            get => _view?.Renderer.InputTextToCursor ?? string.Empty;
            set
            {
                if (_view != null)
                {
                    _view.Renderer.InputTextToCursor = value;
                }
            }
        }

        public bool IsAlternativeOptionsActive
        {
            get => _view?.Renderer.IsAlternativeOptionsActive ?? false;
            set
            {
                if (_view != null)
                {
                    _view.Renderer.IsAlternativeOptionsActive = value;
                }
            }
        }

        public List<string> AlternativeOptions
        {
            set
            {
                if (_view != null)
                {
                    _view.Renderer.AlternativeOptions = value;
                }
            }
        }

        public int AlternativeOptionsIndex
        {
            set
            {
                if (_view != null)
                {
                    _view.Renderer.AlternativeOptionsIndex = value;
                }
            }
        }

        public bool MoveUp => IsVisible && _view.Input.MoveUp;

        public bool MoveDown => IsVisible && _view.Input.MoveDown;

        bool IConsoleInput.Focus => IsVisible && _view.Input.Focus;

        bool IConsoleInput.QuitFocus => IsVisible && _view.Input.QuitFocus;

        public bool ShowOrHide => false;

        private ToolkitView _view;

        private Action<string> _onSubmit;

        private Action<string> _onTextChanged;

        public void Attach(ToolkitView view)
        {
            _view = view;
            view.Renderer.BindOnSubmit(Submit);
            view.Renderer.BindOnTextChanged(TextChanged);
            view.Renderer.BindLogs(Logs);
        }

        /// <summary> 初始化失败或销毁时断开所有 UI 转发. </summary>
        public void Detach() => _view = null;

        public void BindOnSubmit(Action<string> callback) => _onSubmit = callback;

        public void BindOnTextChanged(Action<string> callback) => _onTextChanged = callback;

        public void Focus()
        {
            if (IsVisible)
            {
                _view.Renderer.Focus();
            }
        }

        public void ActivateInput()
        {
            if (IsVisible)
            {
                _view.Renderer.ActivateInput();
            }
        }

        public void QuitFocus() => _view?.Renderer.QuitFocus();

        public void SetInputCursorPosition(int pos) => _view?.Renderer.SetInputCursorPosition(pos);

        public void MoveScrollBarToEnd() => _view?.Renderer.MoveScrollBarToEnd();

        public void Output(string msg) => Output(msg, "#ffffff");

        public void Output(string msg, string color = "#ffffff") => OutputLog(new ConsoleLogRecord(msg, msg, color));

        public void Output(string[] msgs) => Output(msgs, "#ffffff");

        public void Output(string[] msgs, string color = "#ffffff")
        {
            foreach (var msg in msgs)
            {
                Output(msg, color);
            }
        }

        public void Clear()
        {
            if (_view != null)
            {
                _view.Renderer.Clear();
            }
            else
            {
                DevelopmentLogSystem.Clear();
            }
        }

        public void OutputLog(ConsoleLogRecord record) => DevelopmentLogSystem.Append(record);

        // 关闭期间 InputField 的失焦事件不能执行命令.
        private void Submit(string text)
        {
            if (IsVisible)
            {
                _onSubmit?.Invoke(text);
            }
        }

        // 控制器主动更新文本时也需要回调, 以保留历史与补全语义.
        private void TextChanged(string text) => _onTextChanged?.Invoke(text);
    }
}
