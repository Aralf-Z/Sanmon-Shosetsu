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
using System.Linq;

namespace RedSaw.CommandLineInterface
{
    /// <summary> Command system. </summary>
    public partial class CommandSystem
    {
        /// <summary> a simple virtual machine to execute user input. </summary>
        public readonly VirtualMachine vm;

        private readonly Lexer _lexer; // Lexer.

        private readonly SyntaxAnalyzer _syntaxAnalyzer; // Syntax analyzer.

        private readonly float _scoreThresholdCommand; // Score threshold command.

        private readonly float _scoreThresholdVariable; // Score threshold variable.

        private readonly float _scoreThresholdType; // Score threshold type.

        private readonly QueryCache<Suggestion> _commandQueryCache; // Command query cache.

        private readonly QueryCache<Suggestion> _variableQueryCache; // Variable query cache.

        private readonly QueryCache<Suggestion> _typeQueryCache; // Type query cache.

        private readonly CharAutomaton _charAutomaton; // Char automaton.

        private string _lastQueryStr = string.Empty; // Last query str.

        /// <summary> register a new console command whatever static method or instance method. </summary>
        public void RegisterCommand(Command command) => vm.RegisterCallable(command);

        /// <summary> register a new console property. </summary>
        public void RegisterProperty(StackProperty property) => vm.RegisterProperty(property);

        /// <summary> register a new console value parser. </summary>
        public void RegisterValueParser(Type type, ValueParser parser, string alias = null) => vm.RegisterValueParser(type, parser, alias);

        /// <summary> register a new console value parser. </summary>
        public void RegisterValueParser<TType>(ValueParser parser, string alias = null) => vm.RegisterValueParser(typeof(TType), parser, alias);

        /// <summary> get variable created at runtime. </summary>
        public object GetLocalVariable(string name) => vm.GetLocalVariable(name);

        /// <summary> set variable created at runtime. </summary>
        public void SetLocalVariable(string name, object value) => vm.SetLocalVariable(name, value);

        /// <summary> get current suggestions. </summary>
        public Suggestion[] GetCurrentSuggestions(string currentText, int count, Func<string, string, float> scoreFunc)
        {
            var queryStatus = _charAutomaton.Input(currentText);
            _lastQueryStr = queryStatus.queryStr;
            return queryStatus.suggestionType switch
            {
                SuggestionType.Variable => QueryVariable(queryStatus.queryStr, count, scoreFunc),
                SuggestionType.Command => QueryCommands(queryStatus.queryStr, count, scoreFunc),
                SuggestionType.Member => QueryType(queryStatus.queryStr, queryStatus.queryType, count, scoreFunc),
                _ => Array.Empty<Suggestion>(),
            };
        }

        /// <summary> Take suggestion. </summary>
        public string TakeSuggestion(string currentInput, string primary)
        {
            if (_lastQueryStr == string.Empty)
                return currentInput + primary;
            return currentInput[..^_lastQueryStr.Length] + primary;
        }

        /// <summary> execute given command. commandInput: command input, and it must not be empty or null. </summary>
        public Exception Execute(string commandInput, out object executeResult)
        {
            try
            {
                var lexerResult = _lexer.Parse(commandInput);
                var syntaxTree = _syntaxAnalyzer.Analyze(lexerResult);
                executeResult = vm.ExecuteRoot(syntaxTree);
                return null;
            }
            catch (CommandSystemException ex)
            {
                executeResult = null;
                return ex;
            }
            catch (Exception ex)
            {
                executeResult = null;
                return ex.InnerException ?? ex;
            }
        }

        /// <summary> query a group of commands with top score of all commands. query: query string. count: the count of commands you want to query. scoreFunc: score function. </summary>
        public Suggestion[] QueryCommands(string query, int count, Func<string, string, float> scoreFunc, string tag = null)
        {
            query ??= string.Empty;
            /* check if this query has been stored */
            var queryDetailInfo = query + ":" + tag ?? string.Empty;
            if (_commandQueryCache.GetCache(queryDetailInfo, out var result))
                return result;
            /* start to query */
            var bestChoices = vm.AllCallables.Where(s => s is Command command && command.CompareTag(tag)).Select(s => new { value = s, score = scoreFunc(query, s.Name) }).Where(s => s.score > _scoreThresholdCommand).OrderByDescending(s => s.score).Take(Math.Max(1, count));
            result = bestChoices.Select((s) =>
            {
                var c = (Command)s.value;
                return new Suggestion(c.Name, c.description);
            }).ToArray();
            _commandQueryCache.Cache(queryDetailInfo, result);
            return result;
        }

        /// <summary> Query variable. </summary>
        public Suggestion[] QueryVariable(string query, int count, Func<string, string, float> scoreFunc, string tag = null)
        {
            query ??= string.Empty;
            var queryDetailInfo = query + ":" + tag ?? string.Empty;
            if (_variableQueryCache.GetCache(queryDetailInfo, out var result))
                return result;
            var bestChoices = vm.AllProperties.Where(s => s.CompareTag(tag)).Select(s => new { value = s, score = scoreFunc(query, s.Name) }).Where(s => s.score > _scoreThresholdVariable).OrderByDescending(s => s.score).Take(Math.Max(1, count));
            result = bestChoices.Select(s => new Suggestion(s.value.Name, s.value.description)).ToArray();
            _variableQueryCache.Cache(queryDetailInfo, result);
            return result;
        }

        /// <summary> Query type. </summary>
        public Suggestion[] QueryType(string query, Type type, int count, Func<string, string, float> scoreFunc)
        {
            query ??= string.Empty;
            var queryDetailInfo = $"{type.Name}.{query}";
            if (_typeQueryCache.GetCache(queryDetailInfo, out var result))
                return result;
            var bestChoices = type.GetMembers().Select(s => new { value = s, score = scoreFunc(query, s.Name) }).Where(s => s.score > _scoreThresholdType).OrderByDescending(s => s.score).Take(Math.Max(1, count));
            result = bestChoices.Select(s => new Suggestion(s.value.Name, s.value.GetMemberTypeName())).ToArray();
            _typeQueryCache.Cache(queryDetailInfo, result);
            return result;
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
