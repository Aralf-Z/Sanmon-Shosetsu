using System;
using System.Collections.Generic;
using System.Reflection;

namespace RedSaw.CommandLineInterface
{
    /// <summary> command console. </summary>
    public class ConsoleController<TLog>
        where TLog : Enum
    {
        public readonly CommandSystem commandSystem;

        public event Action e_onFocusOut;

        public event Action e_onFocus;

        internal InputHistory History => _inputHistory;

        private readonly IConsoleRenderer _renderer;

        private readonly IConsoleInput _userInput;

        private readonly LogManager<TLog> _logManager;

        private readonly int _alternativeCommandCount;

        private readonly bool _shouldRecordFailedCommand;

        private readonly bool _outputWithTime;

        private readonly bool _outputStackTraceOfCommandExecution;

        private readonly InputHistory _inputHistory;

        private readonly LinearSelector _selector;

        private bool _ignoreTextChanged;

        public ConsoleController(IConsoleRenderer renderer, IConsoleInput userInput, int logCapacity = -1, int inputHistoryCapacity = 20, int commandQueryCacheCapacity = 20, int alternativeCommandCount = 8, bool shouldRecordFailedCommand = true, bool outputWithTime = true, bool outputStackTraceOfCommandExecution = true)
        {
            this._renderer = renderer;
            renderer.BindOnSubmit(OnSubmit);
            renderer.BindOnTextChanged(OnTextChanged);
            this._userInput = userInput;
            commandSystem = new CommandSystem(commandQueryCacheCapacity: commandQueryCacheCapacity);
            _logManager = new LogManager<TLog>(logCapacity);
            _logManager.e_onReceivedMessage += log =>
            {
                if (log.HasColor)
                {
                    renderer.Output(log.message, log.color);
                    return;
                }

                renderer.Output(log.message);
            };
            _inputHistory = new InputHistory(Math.Max(inputHistoryCapacity, 2));
            _selector = new LinearSelector();
            this._selector.e_onSelectionChanged += idx =>
            {
                renderer.AlternativeOptionsIndex = idx;
                renderer.MoveCursorToEnd();
            };
            this._alternativeCommandCount = Math.Max(alternativeCommandCount, 1);
            this._shouldRecordFailedCommand = shouldRecordFailedCommand;
            this._outputWithTime = outputWithTime;
            this._outputStackTraceOfCommandExecution = outputStackTraceOfCommandExecution;
        }

        public void Update()
        {
            if (_userInput.ShowOrHide)
            {
                _renderer.IsVisible = !_renderer.IsVisible;
            }

            if (!_renderer.IsVisible) return;

            if (_renderer.IsInputFieldFocus)
            {
                if (_userInput.QuitFocus)
                {
                    _renderer.QuitFocus();
                    e_onFocusOut?.Invoke();
                }
                else if (_userInput.MoveDown)
                {
                    if (_renderer.IsAlternativeOptionsActive)
                    {
                        _selector.MoveNext();
                    }
                    else
                    {
                        _ignoreTextChanged = true;
                        _renderer.InputText = _inputHistory.Next;
                        _renderer.MoveCursorToEnd();
                    }
                }
                else if (_userInput.MoveUp)
                {
                    if (_renderer.IsAlternativeOptionsActive)
                    {
                        _selector.MoveLast();
                    }
                    else
                    {
                        _ignoreTextChanged = true;
                        _renderer.InputText = _inputHistory.Last;
                        _renderer.MoveCursorToEnd();
                    }
                }

                return;
            }

            if (_userInput.Focus)
            {
                _renderer.Focus();
                _renderer.ActivateInput();
                e_onFocus?.Invoke();
            }
        }

        public void Output(string msg)
        {
            if (!TryStructuredOutput(msg, null, ConsoleLogType.Dev, null))
            {
                _logManager.Output(AddTimeInfo(msg));
            }
        }

        public void Output(string msg, string color = "#ffffff")
        {
            if (!TryStructuredOutput(msg, color, ConsoleLogType.Dev, null))
            {
                _logManager.Output(AddTimeInfo(msg), color);
            }
        }

        public void Output(string msg, string color, ConsoleLogType type, string stackTrace = null)
        {
            if (!TryStructuredOutput(msg, color, type, stackTrace))
            {
                Output(msg, color);
            }
        }

        public void Output(string[] msgs, string color = "#ffffff")
        {
            if (_renderer is IStructuredConsoleRenderer { AcceptsStructuredLogs: true })
            {
                foreach (var msg in msgs)
                {
                    Output(msg, color);
                }

                return;
            }

            foreach (var m in AddTimeInfo(msgs))
            {
                _logManager.Output(m, color);
            }
        }

        public void Output(string[] msgs)
        {
            if (_renderer is IStructuredConsoleRenderer { AcceptsStructuredLogs: true })
            {
                foreach (var msg in msgs)
                {
                    Output(msg);
                }

                return;
            }

            foreach (var m in AddTimeInfo(msgs))
            {
                _logManager.Output(m);
            }
        }

        public void ClearOutputPanel() => _renderer.Clear();

        public void OnTextChanged(string text)
        {
            if (_ignoreTextChanged)
            {
                _ignoreTextChanged = false;
                return;
            }

            string queryText = _renderer.InputTextToCursor;
            var result = commandSystem.GetCurrentSuggestions(queryText, _alternativeCommandCount, CLIUtils.FindSimilarity);

            if (result.Length == 0)
            {
                if (_renderer.IsAlternativeOptionsActive)
                {
                    _renderer.IsAlternativeOptionsActive = false;
                }

                return;
            }

            var optionsBuffer = new List<string>();
            var list = new List<string>();

            foreach (var elem in result)
            {
                optionsBuffer.Add(elem.primary);
                list.Add(elem.ToString());
            }

            if (!_renderer.IsAlternativeOptionsActive)
            {
                _renderer.IsAlternativeOptionsActive = true;
            }

            _selector.LoadOptions(optionsBuffer);
            _renderer.AlternativeOptions = list;
            _renderer.AlternativeOptionsIndex = _selector.SelectionIndex;
        }

        public void OnSubmit(string text)
        {
            if (_renderer.IsAlternativeOptionsActive && _selector.GetCurrentSelection(out string selection))
            {
                _renderer.InputTextToCursor = commandSystem.TakeSuggestion(_renderer.InputTextToCursor, selection);
                _renderer.IsAlternativeOptionsActive = false;
                _renderer.ActivateInput();
                _renderer.SetInputCursorPosition(_renderer.InputText.Length);
                return;
            }

            if (_renderer.IsAlternativeOptionsActive)
            {
                _renderer.IsAlternativeOptionsActive = false;
            }

            if (!TryStructuredOutput(text, null, ConsoleLogType.Dev, "用户输入内容", true))
            {
                Output(text);
            }

            if (text.Length > 0)
            {
                var ex = commandSystem.Execute(text, out object executeResult);

                if (ex == null)
                {
                    _inputHistory.Record(text);
                    OutputResult(executeResult);
                }
                else
                {
                    if (_shouldRecordFailedCommand)
                    {
                        _inputHistory.Record(text);
                    }

                    if (!TryStructuredOutput(ex.Message, "#f27a5f", ConsoleLogType.Dev, _outputStackTraceOfCommandExecution ? ex.StackTrace : null))
                    {
                        Output(ex.Message, "#f27a5f");
                        if (_outputStackTraceOfCommandExecution)
                        {
                            Output(ex.StackTrace, "#f27a5f");
                        }
                    }
                }

                _renderer.InputText = string.Empty;
            }

            _renderer.MoveScrollBarToEnd();
            _renderer.ActivateInput();
        }

        // 仅新的日志适配器走结构化通路, 不改变命令解析或旧日志管理器.
        private bool TryStructuredOutput(string message, string color, ConsoleLogType type, string stackTrace, bool isUserInput = false)
        {
            if (_renderer is not IStructuredConsoleRenderer { AcceptsStructuredLogs: true } renderer) return false;

            renderer.OutputLog(new ConsoleLogRecord(message, AddTimeInfo(message), color, type, stackTrace) { IsUserInput = isUserInput });
            return true;
        }

        private string AddTimeInfo(string msg)
        {
            if (_outputWithTime) return CLIUtils.TimeInfo + msg;

            return msg;
        }

        private string[] AddTimeInfo(string[] msgs)
        {
            if (_outputWithTime)
            {
                var timeInfo = CLIUtils.TimeInfo;

                for (int i = 0; i < msgs.Length; i++)
                {
                    msgs[i] = timeInfo + msgs[i];
                }

                return msgs;
            }

            return msgs;
        }

        private void OutputResult(object instance)
        {
            if (instance == null) return;

            var type = instance.GetType();
            var attr = type.GetCustomAttribute<DebugInfoAttribute>();

            if (attr == null)
            {
                Output(instance.ToString());
                return;
            }

            var debugInfos = DebugHelper.GetDebugInfos(instance);

            if (debugInfos.Length == 0) return;

            Output($"---------- {instance} start ----------");
            foreach (var (message, color) in debugInfos)
            {
                Output(message, color);
            }

            Output($"---------- {instance} end ----------");
        }
    }
}
