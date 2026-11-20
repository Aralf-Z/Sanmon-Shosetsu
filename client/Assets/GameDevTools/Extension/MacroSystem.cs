using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using RedSaw.CommandLineInterface;

namespace GameConsole.Extension
{
    /// <summary> 当前开发工具拥有的宏注册, 保存及执行服务. </summary>
    public sealed class MacroSystem : IDisposable
    {
        public event Action e_onChanged;

        public event Action e_onStatusChanged;

        /// <summary> 已收集的共享命令包装, 不重复执行参数收集器. </summary>
        public IReadOnlyList<CommandWrapper> Commands => _wrappers;

        public IReadOnlyList<Macro> Macros => _macros;

        public bool IsReady { get; private set; }

        public string RunningId { get; private set; }

        public string Status
        {
            get
            {
                if (_statusCache == null || _statusVersion != ToolkitLocalization.Version)
                {
                    _statusCache = _statusKey == null
                        ? _statusRaw
                        : ToolkitLocalization.Format(_statusKey, _statusArguments);
                    _statusVersion = ToolkitLocalization.Version;
                }

                return _statusCache;
            }
        }

        private string _statusKey = "ui.MacroSystem.labels";

        private (string name, object value)[] _statusArguments = Array.Empty<(string, object)>();

        private string _statusRaw;

        private string _statusCache;

        private int _statusVersion = -1;

        public string FilePath => Config.FilePath;

        private readonly Action _releaseRunner; // 销毁旧服务不会清除后来建立的新执行入口.

        private readonly CommandSystem _commands;

        private readonly MacroCompiler _compiler; // 无副作用编译器.

        private readonly MonoBehaviour _owner; // 协程与异步生命周期所属总控.

        private readonly Action<string> _output;

        private readonly List<CommandWrapper> _wrappers = new(); // 延迟构造共享缓存.

        private readonly List<Macro> _macros = new();

        private Command[] _source;

        private int _index;

        private CancellationTokenSource _execution; // 仅取消等待和后续调度.

        private readonly CancellationTokenSource _lifetime = new();

        private bool _disposed; // 防止异步完成后再次访问已销毁窗口.

        public MacroSystem(CommandSystem commands, MonoBehaviour owner, Action<string> output)
        {
            _commands = commands;
            _owner = owner;
            _output = output;
            _compiler = new MacroCompiler(commands);
            _macros.AddRange(Config.LoadMacros());
            _releaseRunner = Macro.BindRunner(macro => Run(macro));
        }

        public void RequestCommands() => _source ??= _commands.vm.AllCallables.OfType<Command>()
            .OrderBy(command => command.name)
            .ToArray();

        public void Tick()
        {
            if (_source == null || IsReady || _disposed) return;

            if (_index < _source.Length)
            {
                var command = _source[_index++];

                try
                {
                    var wrapper = new CommandWrapper(command);

                    _wrappers.Add(wrapper);
                    wrapper.RegisterMacros(this);
                }
                catch (Exception exception)
                {
                    _output(ToolkitLocalization.Format("ui.MacroSystem.Tick", ("p0", (object)(command.name)), ("p1", (object)(exception.GetBaseException().Message))));
                }
            }

            if (_index == _source.Length)
            {
                IsReady = true;
                e_onChanged?.Invoke();
            }
        }

        /// <summary> 仅由 CommandWrapper 的 MacroAttribute 定义注册内置宏, 单条错误不影响其他定义. </summary>
        internal void Register(CommandWrapper wrapper)
        {
            var command = wrapper.Command;

            foreach (var attribute in wrapper.Macro)
            {
                try
                {
                    AddBuiltIn("attribute:" + command.name + ":" + attribute.name, attribute.name, attribute.desc, MacroCompiler.BuildCommand(command, attribute.parameters));
                }
                catch (Exception exception)
                {
                    _output(ToolkitLocalization.Format("ui.MacroSystem.Register", ("p0", (object)(attribute.name)), ("p1", (object)(command.name)), ("p2", (object)(exception.GetBaseException().Message))));
                }
            }
        }

        public List<MacroDiagnostic> Validate(Macro macro) => _compiler.Compile(macro, out _);

        public string Format(string body) => string.Join("\n", (body ?? string.Empty).Replace("\r\n", "\n").Split('\n').Select(MacroText.FormatLine));

        /// <summary> 保存有效自定义宏, 改名不会改变持久化标识. </summary>
        public bool Save(Macro value, out string error)
        {
            error = string.Join("\n", Validate(value));
            if (value.IsBuiltIn || error.Length > 0) return false;

            var saved = new Macro(value.ToSource())
            { Id = value.Id ?? Guid.NewGuid()
                .ToString("N") };
            var items = _macros.Where(macro => !macro.IsBuiltIn && macro.Id != saved.Id)
                .Append(saved)
                .ToList();

            if (!Config.TrySaveMacros(items, null, out _, out error)) return false;

            _macros.RemoveAll(macro => !macro.IsBuiltIn && macro.Id == saved.Id);
            _macros.Add(saved);
            value.Id = saved.Id;
            e_onChanged?.Invoke();
            return true;
        }

        /// <summary> 删除成功后才更新库, 内置宏不能删除. </summary>
        public bool Delete(Macro value, out string error)
        {
            error = string.Empty;
            if (value.IsBuiltIn) return false;

            var items = _macros.Where(macro => !macro.IsBuiltIn && macro.Id != value.Id);

            if (!Config.TrySaveMacros(items, value.Id, out var pinsChanged, out error)) return false;
            _macros.RemoveAll(macro => macro.Id == value.Id);
            if (pinsChanged)
            {
                Config.NotifySettingsChanged();
            }

            e_onChanged?.Invoke();
            return true;
        }

        public bool Run(Macro value)
        {
            if (_disposed || RunningId != null) return false;

            var errors = _compiler.Compile(value, out var steps);

            if (errors.Count > 0)
            {
                SetRawStatus(string.Join("\n", errors));
                _output(Status);
                return false;
            }

            _execution = new CancellationTokenSource();
            RunningId = value.Id ?? "draft";
            ExecuteAsync(value.name, steps, _execution)
                .Forget();
            return true;
        }

        public void Stop() => _execution?.Cancel();

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            _releaseRunner();
            e_onChanged = null;
            e_onStatusChanged = null;
            try
            {
                Stop();
            }
            finally
            {
                try
                {
                    _lifetime.Cancel();
                }
                finally
                {
                    _lifetime.Dispose();
                }
            }
        }

        private void AddBuiltIn(string id, string name, string desc, string body)
        {
            var macro = new Macro(Macro.Compose(name, desc, body))
            { Id = id, IsBuiltIn = true, macro = body };
            var errors = Validate(macro);

            if (errors.Count > 0)
            {
                _output(ToolkitLocalization.Format("ui.MacroSystem.AddBuiltIn", ("p0", (object)(name)), ("p1", (object)(string.Join("; ", errors)))));
                return;
            }

            _macros.Add(macro);
        }

        private async UniTask ExecuteAsync(string name, List<MacroCompiler.Step> steps, CancellationTokenSource execution)
        {
            var line = 0;
            var remaining = 0;
            var lifetime = _lifetime.Token;

            try
            {
                for (var index = 0; index < steps.Count; index++)
                {
                    var step = steps[index];

                    execution.Token.ThrowIfCancellationRequested();
                    line = step.line;
                    SetStatus("ui.MacroSystem.ExecuteAsync", ("p0", (object)(name)), ("p1", (object)(line)));
                    if (step.repeatCount > 0)
                    {
                        remaining = step.repeatCount;
                        continue;
                    }

                    if (step.repeatEnd)
                    {
                        if (--remaining > 0)
                        {
                            index = step.jump - 1;
                        }

                        continue;
                    }

                    if (step.frame)
                    {
                        await UniTask.NextFrame(cancellationToken: execution.Token);
                    }
                    else if (step.delay)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(step.seconds), ignoreTimeScale: true, cancellationToken: execution.Token);
                    }
                    else
                    {
                        var result = _commands.vm.ExecuteRoot(step.tree);
                        var completion = MacroAwaiter.ObserveAsync(result, _owner, lifetime, message => _output(ToolkitLocalization.Format("ui.MacroSystem.ExecuteAsync.2", ("p0", (object)(name)), ("p1", (object)(step.line)), ("p2", (object)(message)))));

                        if (step.wait)
                        {
                            await completion.AsUniTask()
                                .AttachExternalCancellation(execution.Token);
                        }
                    }
                }

                SetStatus("ui.MacroSystem.ExecuteAsync.3", ("p0", (object)(name)));
            }
            catch (OperationCanceledException)
            {
                SetStatus("ui.MacroSystem.ExecuteAsync.4", ("p0", (object)(name)), ("p1", (object)(line)));
            }
            catch (Exception exception)
            {
                SetStatus("ui.MacroSystem.ExecuteAsync.5", ("p0", (object)(name)), ("p1", (object)(line)), ("p2", (object)(exception.GetBaseException().Message)));
            }
            finally
            {
                _output(Status);
                RunningId = null;
                _execution = null;
                execution.Dispose();
                if (!_disposed)
                {
                    e_onStatusChanged?.Invoke();
                }
            }
        }

        // 输出状态变化, 窗口不可见时也保留最新运行位置.
        private void SetStatus(string key, params (string name, object value)[] arguments)
        {
            _statusKey = key;
            _statusArguments = arguments;
            _statusCache = null;
            if (!_disposed)
            {
                e_onStatusChanged?.Invoke();
            }
        }

        // 保留原始异常内容, 只把工具自身的状态作为本地化模板.
        private void SetRawStatus(string status)
        {
            _statusKey = null;
            _statusRaw = status;
            _statusCache = null;
            if (!_disposed)
            {
                e_onStatusChanged?.Invoke();
            }
        }
    }
}
