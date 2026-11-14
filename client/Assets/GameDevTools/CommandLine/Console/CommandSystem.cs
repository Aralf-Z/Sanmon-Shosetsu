/*
MIT License - CommandSystem.cs
Created By : Prince Biscuit
Created Date : 2024/01/01

Description : CommandSystem.cs is a part of GameCLI package. 
              use for any game that need a command line interface.

version : 0.2.0
@2024.01.28 : Totally refactor the CommandSystem.cs, now it has more functions

*/
using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> command system, provide interface to control VirtualMachine. </summary>
    public partial class CommandSystem
    {
        /// <summary> Create a CommandSystem instance. logCapacity: the capacity of logs that shows on console. ignoreInvalidSetBehaviour: while execute command &quot;func() = 999&quot; usually, we assgin value to object with a setter interface, but if target value has no setter interface, the flag will determine if vm should ignore this error or not. receiveValueFromNonStringType: while execute command &quot;@myValue = myFunc()&quot; if &apos;myValue&apos; is string type and &apos;myFunc&apos; return a non-string value, this flag will determine if vm should convert return value to string type. treatVoidAsNull: while execute command input &quot;a = myfunc()&quot; if &quot;myfunc&quot; has no return value, then we should set a &apos;null&apos; value to &apos;a&apos;. </summary>
        public CommandSystem(float scoreThresholdCommand = 0.3f, float scoreThresholdVariable = 0.1f, float scoreThresholdType = 0.1f, int typeReflectionQueryCache = 20, int commandQueryCacheCapacity = 20, int variableQueryCacheCapacity = 20, bool ignoreInvalidSetBehaviour = true, bool receiveValueFromNonStringType = true, bool treatVoidAsNull = true)
        {
            /* initialize virtual machine */
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
            /* load custom commands */
            foreach (var command in CommandCreator.CollectCommands<CommandAttribute>())
            {
                vm.RegisterCallable(command);
            }

            /* load custom properties */
            foreach (var property in CommandCreator.CollectProperties<CommandPropertyAttribute>())
            {
                vm.RegisterProperty(property);
            }

            /* load value parsers */
            foreach (var pack in CommandCreator.CollectValueParsers<CommandValueParserAttribute>())
            {
                vm.RegisterValueParser(pack.Item2, pack.Item1, pack.Item3);
            }
        }
    }
// /// <summary>command system</summary>
// public static class CommandSystem{
// static readonly VirtualMachine vm;
// static readonly Dictionary<string, Command> commands = new();
// static readonly QueryBuffer queryBuffer = new(20);
// static readonly CommandParser parser = new();
// static Action<string> output;
// static Action<string> outputErr;
// static void Output(string message) => output?.Invoke(message);
// static void OutputErr(string message) => outputErr?.Invoke(message);
// /// <summary>get all commands' informations</summary>
// public static IEnumerable<Command> TotalCommands => commands.Values;
// static CommandSystem(){
//     foreach(var command in CommandCreator.CollectCommands<CommandAttribute>()){
//         if(commands.ContainsKey(command.name)){
//             commands[command.name] = command;
//             continue;
//         }
//         commands.Add(command.name, command);
//     }
// }
// public static void CollectDefaultCommand(){
//     // foreach(var command in CommandCreator.CollectCommands<DefaultCommandAttribute>()){
//     //     if(commands.ContainsKey(command.name))continue;
//     //     commands.Add(command.name, command);
//     // }
// }
// /// <summary>execute given command</summary>
// /// <param name="input">the command you want to execute</param>
// /// <returns>if success</returns>
// public static bool Execute(string input){
//     if(!parser.Parse(input, out string[] result)){
//         OutputErr($"invalid command: {input} ");
//         return false;
//     }
//     string commandName = result[0];
//     if(!commands.TryGetValue(commandName, out Command command)){
//         OutputErr($"unknown command name: {commandName}");
//         return false;
//     }
//     Exception e = null;
//     if(result.Length == 1){
//         // e = command.Execute(new string[]{});
//     }else{
//         string[] args = new string[result.Length - 1];
//         for(int i = 1; i < result.Length; i ++){
//             args[i - 1] = result[i];
//         }
//         // e = command.Execute(args);
//     }
//     if(e != null){
//         OutputErr($"command error : {e.Message}");
//         OutputErr(e.StackTrace);
//         return false;
//     }
//     return true;
// }
// /// <summary>execute given command</summary>
// /// <param name="input">the command you want to execute</param>
// /// <returns>if success</returns>
// public static bool ExecuteSlience(string input){
//     if(!parser.Parse(input, out string[] result))return false;
//     if(!commands.TryGetValue(result[0], out Command command))return false;
//     Exception e = null;
//     if(result.Length == 1){
//         // e = command.Execute(new string[]{});
//     }else{
//         string[] args = new string[result.Length - 1];
//         for(int i = 1; i < result.Length; i ++){
//             args[i - 1] = result[i];
//         }
//         // e = command.Execute(args);
//     }
//     return e == null;
// }
// /// <summary>you can set a output function of CommandSystem
// /// to receive normal message </summary>
// public static void SetOutputFunc(Action<string> func) => output = func;
// /// <summary>you can set a output function of CommandSystem
// /// to receive error message </summary>
// public static void SetOutputErrFunc(Action<string> func) => outputErr = func;
// /// <summary>query a group of commands with top score of all commands</summary>
// /// <param name="query">query string</param>
// /// <param name="count">the count of commands you want to query</param>
// /// <param name="scoreFunc">score function</param>
// /// <returns>return a group of commands</returns>
// public static Command[] QueryCommands(string query, int count, Func<string, string, float> scoreFunc, string tag = null){
//     /* ensure that query must be a valid string */
//     if(query == null || query.Length == 0)return new Command[0];
//     /* check if this query has been stored */ 
//     var queryDetailInfo = query + ":" + tag ?? string.Empty;
//     if(queryBuffer.GetCache(queryDetailInfo, out var result))return result;
//     /* start to query */
//     count = Math.Max(1, count);
//     var bestChoices = commands
//     .Where(s => s.Value.CompareTag(tag))
//     .Select(s => new {value = s, score = scoreFunc(query, s.Key)})
//     .Where(s => s.score > 0)
//     .OrderByDescending(s => s.score)
//     .Take(count)
//     .ToArray();
//     result = bestChoices.Select(s => s.value.Value).ToArray();
//     queryBuffer.Cache(queryDetailInfo, result);
//     return result;
// }
// }
}
