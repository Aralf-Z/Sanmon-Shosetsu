using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using RedSaw.CommandLineInterface;
using UnityEngine;

namespace GameConsole.Extension
{
    /// <summary> 当前开发工具拥有的宏注册, 保存及执行服务. </summary>
    public sealed class MacroSystem : IDisposable
    {
        /// <summary> 宏库变化后刷新编辑器与按钮网格. </summary>
        public event Action e_onChanged;
        /// <summary> 运行状态与当前行变化通知. </summary>
        public event Action e_onStatusChanged;
        /// <summary> 已收集的共享命令包装, 不重复执行参数收集器. </summary>
        public IReadOnlyList<CommandWrapper> Commands => _wrappers;
        /// <summary> 当前启动的全部宏. </summary>
        public IReadOnlyList<Macro> Macros => _macros;
        /// <summary> 首次命令收集是否完成. </summary>
        public bool IsReady { get; private set; }
        /// <summary> 正在运行的宏标识, 空表示未运行. </summary>
        public string RunningId { get; private set; }
        /// <summary> 可读执行状态. </summary>
        public string Status
        {
            get
            {
                if (_statusCache == null || _statusVersion != ToolkitLocalization.Version)
                {
                    _statusCache = _statusKey == null ? _statusRaw : ToolkitLocalization.Format(_statusKey, _statusArguments);
                    _statusVersion = ToolkitLocalization.Version;
                }
                return _statusCache;
            }
        }
        private string _statusKey = "ui.MacroSystem.labels"; // 状态持有键和参数, 不捕获已经结束的执行上下文.
        private (string name, object value)[] _statusArguments = Array.Empty<(string, object)>(); // 最近状态的不可变参数快照.
        private string _statusRaw; // 上游原始错误文本不尝试反向翻译.
        private string _statusCache; // 无状态或语言变化时复用现有字符串.
        private int _statusVersion = -1; // 最近格式化的语言版本.
        /// <summary> 自定义宏保存路径. </summary>
        public string FilePath => Path.Combine(Application.persistentDataPath, "development_macros.json");

        private readonly Action _releaseRunner; // 销毁旧服务不会清除后来建立的新执行入口.
        private readonly CommandSystem _commands; // 唯一控制台命令系统.
        private readonly MacroCompiler _compiler; // 无副作用编译器.
        private readonly MonoBehaviour _owner; // 协程与异步生命周期所属总控.
        private readonly Action<string> _output; // 输出经过原日志桥.
        private readonly List<CommandWrapper> _wrappers = new(); // 延迟构造共享缓存.
        private readonly List<Macro> _macros = new(); // 内置定义和本地自定义定义.
        private Command[] _source; // 首次打开功能页后取得命令快照.
        private int _index; // 每帧最多收集一个命令.
        private CancellationTokenSource _execution; // 仅取消等待和后续调度.
        private bool _disposed; // 防止异步完成后再次访问已销毁窗口.

        /// <summary> 准备运行服务, 不在启动时执行参数预设收集器. </summary>
        public MacroSystem(CommandSystem commands, MonoBehaviour owner, Action<string> output)
        {
            _commands = commands;
            _owner = owner;
            _output = output;
            _compiler = new MacroCompiler(commands);
            Load();
            _releaseRunner = Macro.BindRunner(macro => Run(macro));
        }

        /// <summary> 任一使用宏或命令的页面都能触发同一个缓存的收集. </summary>
        public void RequestCommands() => _source ??= _commands.vm.AllCallables.OfType<Command>().OrderBy(command => command.name).ToArray();

        /// <summary> 总控逐帧推进收集, 窗口关闭不影响已启动的宏. </summary>
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
                catch (Exception exception) { _output(ToolkitLocalization.Format("ui.MacroSystem.Tick", ("p0", (object)(command.name)), ("p1", (object)(exception.GetBaseException().Message)))); }
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
                try { AddBuiltIn("attribute:" + command.name + ":" + attribute.name, attribute.name, attribute.desc, MacroCompiler.BuildCommand(command, attribute.parameters)); }
                catch (Exception exception) { _output(ToolkitLocalization.Format("ui.MacroSystem.Register", ("p0", (object)(attribute.name)), ("p1", (object)(command.name)), ("p2", (object)(exception.GetBaseException().Message)))); }
            }
        }

        /// <summary> 编译编辑内容, 不调用任何业务命令. </summary>
        public List<MacroDiagnostic> Validate(Macro macro) => _compiler.Compile(macro, out _);

        /// <summary> 格式化时不更改注释, 字符串及命令名称. </summary>
        public string Format(string body) => string.Join("\n", (body ?? string.Empty).Replace("\r\n", "\n").Split('\n').Select(MacroText.FormatLine));

        /// <summary> 保存有效自定义宏, 改名不会改变持久化标识. </summary>
        public bool Save(Macro value, out string error)
        {
            error = string.Join("\n", Validate(value));
            if (value.IsBuiltIn || error.Length > 0) return false;
            var saved = new Macro(value.ToSource()) { Id = value.Id ?? Guid.NewGuid().ToString("N") };
            var items = _macros.Where(macro => !macro.IsBuiltIn && macro.Id != saved.Id).Append(saved).ToList();
            if (!Write(items, out error)) return false;
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
            if (!Write(_macros.Where(macro => !macro.IsBuiltIn && macro.Id != value.Id), out error)) return false;
            _macros.RemoveAll(macro => macro.Id == value.Id);
            Config.SetPinned(value.Id, true, false);
            e_onChanged?.Invoke();
            return true;
        }

        /// <summary> 校验后启动独立步骤快照, 同时只运行一个宏. </summary>
        public bool Run(Macro value)
        {
            if (_disposed || RunningId != null) return false;
            var errors = _compiler.Compile(value, out var steps);
            if (errors.Count > 0) { SetRawStatus(string.Join("\n", errors)); _output(Status); return false; }
            _execution = new CancellationTokenSource();
            RunningId = value.Id ?? "draft";
            ExecuteAsync(value.name, steps, _execution).Forget();
            return true;
        }

        /// <summary> 取消等待及后续命令, 不强行终止已调用的业务异步方法. </summary>
        public void Stop() => _execution?.Cancel();

        /// <summary> 总控销毁时停止调度并释放订阅. </summary>
        public void Dispose()
        {
            _disposed = true;
            _releaseRunner();
            Stop();
            e_onChanged = null;
            e_onStatusChanged = null;
        }

        // 内置宏独立编译后复用步骤执行器, 参数原文不经过自定义宏文本预处理.
        private void AddBuiltIn(string id, string name, string desc, string body)
        {
            var macro = new Macro(Macro.Compose(name, desc, body)) { Id = id, IsBuiltIn = true, macro = body };
            var errors = Validate(macro);
            if (errors.Count > 0) { _output(ToolkitLocalization.Format("ui.MacroSystem.AddBuiltIn", ("p0", (object)(name)), ("p1", (object)(string.Join("; ", errors))))); return; }
            _macros.Add(macro);
        }

        // 读取失败保留原文件, 不用空库覆盖用户数据.
        private void Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return;
                var data = JsonUtility.FromJson<Store>(File.ReadAllText(FilePath));
                foreach (var entry in data?.macros ?? new List<Entry>())
                {
                    var macro = new Macro(entry.source) { Id = entry.id };
                    if (string.IsNullOrEmpty(macro.Id) || _macros.Any(item => item.Id == macro.Id)) continue;
                    _macros.Add(macro);
                }
            }
            catch (Exception exception) { _output(ToolkitLocalization.Format("ui.MacroSystem.Load", ("p0", (object)(exception.Message)))); }
        }

        // 先生成完整文件再替换, 保存失败时不修改内存库.
        private bool Write(IEnumerable<Macro> macros, out string error)
        {
            try
            {
                var data = new Store { macros = macros.Select(macro => new Entry { id = macro.Id, source = macro.ToSource() }).ToList() };
                Directory.CreateDirectory(Application.persistentDataPath);
                var temporary = FilePath + ".tmp";
                File.WriteAllText(temporary, JsonUtility.ToJson(data, true));
                if (File.Exists(FilePath)) File.Replace(temporary, FilePath, null);
                else File.Move(temporary, FilePath);
                error = string.Empty;
                return true;
            }
            catch (Exception exception) { error = exception.Message; return false; }
        }

        // 每个命令执行一次, 等待异步结果期间控制台仍可接收输入.
        private async UniTask ExecuteAsync(string name, List<MacroCompiler.Step> steps, CancellationTokenSource execution)
        {
            var line = 0;
            var remaining = 0;
            try
            {
                for (var index = 0; index < steps.Count; index++)
                {
                    var step = steps[index];
                    execution.Token.ThrowIfCancellationRequested();
                    line = step.line;
                    SetStatus("ui.MacroSystem.ExecuteAsync", ("p0", (object)(name)), ("p1", (object)(line)));
                    if (step.repeatCount > 0) { remaining = step.repeatCount; continue; }
                    if (step.repeatEnd)
                    {
                        if (--remaining > 0) index = step.jump - 1;
                        continue;
                    }
                    if (step.frame) await UniTask.NextFrame(cancellationToken: execution.Token);
                    else if (step.delay) await UniTask.Delay(TimeSpan.FromSeconds(step.seconds), ignoreTimeScale: true, cancellationToken: execution.Token);
                    else
                    {
                        var result = _commands.vm.ExecuteRoot(step.tree);
                        var completion = MacroAwaiter.ObserveAsync(result, _owner, message => _output(ToolkitLocalization.Format("ui.MacroSystem.ExecuteAsync.2", ("p0", (object)(name)), ("p1", (object)(step.line)), ("p2", (object)(message)))));
                        if (step.wait) await completion.AsUniTask().AttachExternalCancellation(execution.Token);
                    }
                }
                SetStatus("ui.MacroSystem.ExecuteAsync.3", ("p0", (object)(name)));
            }
            catch (OperationCanceledException) { SetStatus("ui.MacroSystem.ExecuteAsync.4", ("p0", (object)(name)), ("p1", (object)(line))); }
            catch (Exception exception) { SetStatus("ui.MacroSystem.ExecuteAsync.5", ("p0", (object)(name)), ("p1", (object)(line)), ("p2", (object)(exception.GetBaseException().Message))); }
            finally
            {
                _output(Status);
                RunningId = null;
                _execution = null;
                execution.Dispose();
                if (!_disposed) e_onStatusChanged?.Invoke();
            }
        }

        // 输出状态变化, 窗口不可见时也保留最新运行位置.
        private void SetStatus(string key, params (string name, object value)[] arguments)
        {
            _statusKey = key;
            _statusArguments = arguments;
            _statusCache = null;
            if (!_disposed) e_onStatusChanged?.Invoke();
        }

        // 保留原始异常内容, 只把工具自身的状态作为本地化模板.
        private void SetRawStatus(string status)
        {
            _statusKey = null;
            _statusRaw = status;
            _statusCache = null;
            if (!_disposed) e_onStatusChanged?.Invoke();
        }

        [Serializable]
        private sealed class Store
        {
            /// <summary> 自定义宏数据. </summary>
            public List<Entry> macros = new();
        }

        [Serializable]
        private sealed class Entry
        {
            /// <summary> 持久化标识. </summary>
            public string id;
            /// <summary> 完整文本. </summary>
            public string source;
        }
    }
}
