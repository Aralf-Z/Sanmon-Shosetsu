using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Cysharp.Threading.Tasks;
using RedSaw.CommandLineInterface;

namespace GameConsole.Extension
{
    /// <summary> 编辑器可定位的宏正文诊断, 行号从一开始. </summary>
    public sealed class MacroDiagnostic
    {
        /// <summary> 正文行号, 零表示元数据错误. </summary>
        public int Line { get; }

        public string Message { get; }

        public MacroDiagnostic(int line, string message)
        {
            Line = line;
            Message = message;
        }

        public override string ToString() => Line > 0
            ? ToolkitLocalization.Format("ui.MacroCompiler.ToString", ("p0", (object)(Line)), ("p1", (object)(Message)))
            : Message;
    }

    /// <summary> 复用控制台语法树, 编译时不调用命令或读取属性. </summary>
    internal sealed class MacroCompiler
    {
        private readonly Dictionary<string, Command> _commands;

        private readonly Lexer _lexer = new();

        private readonly SyntaxAnalyzer _syntax = new();

        internal MacroCompiler(CommandSystem system) => _commands = system.vm.AllCallables.OfType<Command>()
            .ToDictionary(command => command.name);

        internal List<MacroDiagnostic> Compile(Macro macro, out List<Step> steps)
        {
            if (macro.IsBuiltIn) return CompileBuiltIn(macro, out steps);

            steps = new List<Step>();
            var errors = new List<MacroDiagnostic>();

            if (string.IsNullOrWhiteSpace(macro.name))
            {
                errors.Add(new MacroDiagnostic(0, ToolkitLocalization.Get("ui.MacroCompiler.Compile")));
            }

            var loops = new Stack<int>();
            var lines = (macro.macro ?? string.Empty).Replace("\r\n", "\n")
                .Split('\n');

            if (lines.Length > Config.MAX_MACRO_BODY_LINES)
            {
                errors.Add(new MacroDiagnostic(Config.MAX_MACRO_BODY_LINES + 1, ToolkitLocalization.Format("ui.MacroCompiler.Compile.2", ("p0", (object)(Config.MAX_MACRO_BODY_LINES)), ("p1", (object)(lines.Length)))));
                return errors;
            }

            for (var index = 0; index < lines.Length; index++)
            {
                var line = MacroText.Code(lines[index]);

                if (line.Length == 0)
                {
                    continue;
                }

                try
                {
                    MacroText.Split(line, '\0');
                    var tree = _syntax.Analyze(_lexer.Parse(line));

                    RequireCall(tree);
                    var step = new Step
                    { line = index + 1, tree = tree };

                    if (tree.L.data == "Repeat")
                    {
                        if (loops.Count > 0)
                        {
                            errors.Add(new MacroDiagnostic(index + 1, ToolkitLocalization.Get("ui.MacroCompiler.Compile.3")));
                        }

                        var args = tree.R.children;

                        if (args.Length != 1 || args[0].opcode != SyntaxTreeCode.FactorInt || !int.TryParse(args[0].data, NumberStyles.None, CultureInfo.InvariantCulture, out step.repeatCount) || step.repeatCount < Config.MIN_MACRO_REPEAT_COUNT || step.repeatCount > Config.MAX_MACRO_REPEAT_COUNT)
                        {
                            errors.Add(new MacroDiagnostic(index + 1, ToolkitLocalization.Get("ui.MacroCompiler.Compile.4")));
                        }

                        loops.Push(steps.Count);
                    }
                    else if (tree.L.data == "EndRepeat")
                    {
                        if (tree.R.children.Length != 0)
                        {
                            errors.Add(new MacroDiagnostic(index + 1, ToolkitLocalization.Get("ui.MacroCompiler.Compile.5")));
                        }

                        step.repeatEnd = true;
                        if (loops.Count == 0)
                        {
                            errors.Add(new MacroDiagnostic(index + 1, ToolkitLocalization.Get("ui.MacroCompiler.Compile.6")));
                        }
                        else
                        {
                            step.jump = loops.Pop() + 1;
                        }
                    }
                    else if (tree.L.data == "WaitFor")
                    {
                        var args = tree.R.children;

                        if (args.Length > 1)
                        {
                            throw new FormatException(ToolkitLocalization.Get("ui.MacroCompiler.Compile.7"));
                        }

                        if (args.Length == 0)
                        {
                            step.frame = true;
                        }
                        else if (args[0].opcode == SyntaxTreeCode.Call)
                        {
                            var returnType = ValidateCall(args[0]);

                            if (returnType == typeof(UniTaskVoid))
                            {
                                throw new FormatException(ToolkitLocalization.Get("ui.MacroCompiler.Compile.8"));
                            }

                            step.tree = args[0];
                            step.wait = true;
                        }
                        else
                        {
                            if (args[0].opcode != SyntaxTreeCode.FactorFloat && args[0].opcode != SyntaxTreeCode.FactorInt)
                            {
                                throw new FormatException(ToolkitLocalization.Get("ui.MacroCompiler.Compile.9"));
                            }

                            step.seconds = Convert.ToSingle(MacroText.ConvertLiteral(args[0].data, typeof(float)));
                            if (float.IsNaN(step.seconds) || float.IsInfinity(step.seconds) || step.seconds < 0)
                            {
                                throw new FormatException(ToolkitLocalization.Get("ui.MacroCompiler.Compile.10"));
                            }

                            step.delay = true;
                        }
                    }
                    else
                    {
                        ValidateCall(tree);
                    }

                    steps.Add(step);
                }
                catch (Exception exception)
                {
                    errors.Add(new MacroDiagnostic(index + 1, exception.GetBaseException().Message));
                }
            }

            foreach (var opening in loops)
            {
                errors.Add(new MacroDiagnostic(steps[opening].line, ToolkitLocalization.Get("ui.MacroCompiler.Compile.11")));
            }

            if (steps.Count == 0 && errors.Count == 0)
            {
                errors.Add(new MacroDiagnostic(0, ToolkitLocalization.Get("ui.MacroCompiler.Compile.12")));
            }

            return errors;
        }

        internal static string BuildCommand(Command command, string parameters) => $"{command.name}({parameters})";

        private List<MacroDiagnostic> CompileBuiltIn(Macro macro, out List<Step> steps)
        {
            steps = new List<Step>();
            var errors = new List<MacroDiagnostic>();

            if (string.IsNullOrWhiteSpace(macro.name))
            {
                errors.Add(new MacroDiagnostic(0, ToolkitLocalization.Get("ui.MacroCompiler.CompileBuiltIn")));
            }

            try
            {
                var tree = _syntax.Analyze(_lexer.Parse(macro.macro));

                RequireCall(tree);
                if (!_commands.TryGetValue(tree.L.data, out var command))
                {
                    throw new FormatException(ToolkitLocalization.Format("ui.MacroCompiler.CompileBuiltIn.2", ("p0", (object)(tree.L.data))));
                }

                var infos = command.methodInfo.GetParameters();
                var count = tree.R.children.Length;

                if (count > infos.Length)
                {
                    throw new FormatException(ToolkitLocalization.Format("ui.MacroCompiler.CompileBuiltIn.3", ("p0", (object)(command.name)), ("p1", (object)(infos.Length))));
                }

                for (var i = count; i < infos.Length; i++)
                {
                    if (!infos[i].HasDefaultValue)
                    {
                        throw new FormatException(ToolkitLocalization.Format("ui.MacroCompiler.CompileBuiltIn.4", ("p0", (object)(infos[i].Name))));
                    }
                }

                steps.Add(new Step { line = 1, tree = tree });
            }
            catch (Exception exception)
            {
                errors.Add(new MacroDiagnostic(1, exception.GetBaseException().Message));
            }

            return errors;
        }

        // 每行必须是现有命令调用, 不允许宏名称或任意成员方法作为入口.
        private static void RequireCall(SyntaxTree tree)
        {
            if (tree.opcode != SyntaxTreeCode.Call || tree.children.Length != 2 || tree.L.opcode != SyntaxTreeCode.FactorInput)
            {
                throw new FormatException(ToolkitLocalization.Get("ui.MacroCompiler.RequireCall"));
            }
        }

        private Type ValidateCall(SyntaxTree tree)
        {
            RequireCall(tree);
            if (tree.L.data == "Repeat" || tree.L.data == "EndRepeat" || tree.L.data == "WaitFor")
            {
                throw new FormatException(ToolkitLocalization.Get("ui.MacroCompiler.ValidateCall"));
            }

            if (!_commands.TryGetValue(tree.L.data, out var command))
            {
                var suggestion = _commands.Keys.OrderBy(name => Distance(name.ToLowerInvariant(), tree.L.data.ToLowerInvariant()))
                    .FirstOrDefault();

                throw new FormatException(ToolkitLocalization.Format("ui.MacroCompiler.ValidateCall.2", ("p0", (object)(tree.L.data))) + (suggestion == null ? string.Empty : ToolkitLocalization.Format("ui.MacroCompiler.ValidateCall.3", ("p0", (object)(suggestion)))));
            }

            var args = tree.R.children;
            var infos = command.methodInfo.GetParameters();

            if (args.Length > infos.Length)
            {
                throw new FormatException(ToolkitLocalization.Format("ui.MacroCompiler.ValidateCall.4", ("p0", (object)(command.name)), ("p1", (object)(infos.Length))));
            }

            for (var i = 0; i < infos.Length; i++)
            {
                if (i >= args.Length)
                {
                    if (!infos[i].HasDefaultValue)
                    {
                        throw new FormatException(ToolkitLocalization.Format("ui.MacroCompiler.ValidateCall.5", ("p0", (object)(infos[i].Name))));
                    }

                    continue;
                }

                var arg = args[i];

                if (arg.opcode == SyntaxTreeCode.Call)
                {
                    ValidateCall(arg);
                }
                else if (arg.IsLeaf && arg.opcode != SyntaxTreeCode.Loadvar && MacroText.IsSimple(infos[i].ParameterType))
                {
                    try
                    {
                        MacroText.ConvertLiteral(arg.data, infos[i].ParameterType, arg.opcode == SyntaxTreeCode.FactorNull);
                    }
                    catch (Exception)
                    {
                        throw new FormatException(ToolkitLocalization.Format("ui.MacroCompiler.ValidateCall.6", ("p0", (object)(infos[i].Name)), ("p1", (object)(infos[i].ParameterType.Name)), ("p2", (object)(arg.data))));
                    }
                }
            }

            return command.ReturnType;
        }

        private static int Distance(string first, string second)
        {
            var row = Enumerable.Range(0, second.Length + 1)
                .ToArray();

            for (var i = 1; i <= first.Length; i++)
            {
                var previous = row[0];

                row[0] = i;
                for (var j = 1; j <= second.Length; j++)
                {
                    var old = row[j];

                    row[j] = Math.Min(Math.Min(row[j] + 1, row[j - 1] + 1), previous + (first[i - 1] == second[j - 1] ? 0 : 1));
                    previous = old;
                }
            }

            return row[second.Length];
        }

        /// <summary> 一个步骤持有独立语法树, 不引用编辑器可变文本. </summary>
        internal sealed class Step
        {
            /// <summary> 循环起点的次数, 普通步骤为零. </summary>
            internal int repeatCount;

            internal bool repeatEnd;

            internal int jump;

            internal int line;

            internal SyntaxTree tree;

            internal bool frame;

            /// <summary> 等待实际秒数. </summary>
            internal bool delay;

            /// <summary> 实际时间, 单位秒. </summary>
            internal float seconds;

            internal bool wait;
        }
    }
}
