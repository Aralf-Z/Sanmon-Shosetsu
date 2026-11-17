/* MIT License - CommandSystem.cs; Created By : Prince Biscuit */
using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> command system, provide interface to control VirtualMachine. </summary>
    public partial class CommandSystem
    {
        /// <summary> 通过构造选项控制无赋值接口的错误处理, 非字符串结果转换, 以及 void 结果是否视为 null. </summary>
        public CommandSystem(float scoreThresholdCommand = 0.3f, float scoreThresholdVariable = 0.1f, float scoreThresholdType = 0.1f, int typeReflectionQueryCache = 20, int commandQueryCacheCapacity = 20, int variableQueryCacheCapacity = 20, bool ignoreInvalidSetBehaviour = true, bool receiveValueFromNonStringType = true, bool treatVoidAsNull = true)
        {
            _lexer = new Lexer();
            _syntaxAnalyzer = new SyntaxAnalyzer();
            vm = new VirtualMachine(ignoreInvalidSetBehaviour, receiveValueFromNonStringType, treatVoidAsNull);
            _charAutomaton = new(vm.GetPropertyType, vm.GetCallableType);
            this._scoreThresholdCommand = Math.Clamp(scoreThresholdCommand, 0, 1);
            this._scoreThresholdVariable = Math.Clamp(scoreThresholdVariable, 0, 1);
            this._scoreThresholdType = Math.Clamp(scoreThresholdType, 0, 1);
            _commandQueryCache = new(commandQueryCacheCapacity);
            _variableQueryCache = new(variableQueryCacheCapacity);
            _typeQueryCache = new(typeReflectionQueryCache);
            foreach (var command in CommandCreator.CollectCommands<CommandAttribute>())
            {
                vm.RegisterCallable(command);
            }

            foreach (var property in CommandCreator.CollectProperties<CommandPropertyAttribute>())
            {
                vm.RegisterProperty(property);
            }

            foreach (var pack in CommandCreator.CollectValueParsers<CommandValueParserAttribute>())
            {
                vm.RegisterValueParser(pack.Item2, pack.Item1, pack.Item3);
            }
        }
    }
}
