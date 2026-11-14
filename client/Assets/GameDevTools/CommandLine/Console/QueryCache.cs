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
using System.Collections.Generic;

namespace RedSaw.CommandLineInterface
{
    /// <summary> query buffer. </summary>
    class QueryCache<T>
        where T : struct
    {
        private readonly Dictionary<string, T[]> _buffer = new(); // Buffer.

        private readonly List<string> _queryHistory = new(); // Query history.

        private readonly int _capacity; // Capacity.

        /// <summary> Create a QueryCache instance. </summary>
        public QueryCache(int capacity)
        {
            this._capacity = Math.Max(1, capacity);
        }

        /// <summary> store a query result. query: query string, this must not be a empty or null value. result: query result, must not be a null value. </summary>
        public void Cache(string query, T[] result)
        {
            if (_buffer.ContainsKey(query))
                return;
            if (_queryHistory.Count + 1 > _capacity)
            {
                _buffer.Remove(_queryHistory[0]);
                _queryHistory.RemoveAt(0);
            }

            _buffer.Add(query, result);
            _queryHistory.Add(query);
        }

        /// <summary> check if some query has been recorded. query: query string, and you should check if it is null or empty before you call this function. </summary>
        public bool GetCache(string query, out T[] result)
        {
            return _buffer.TryGetValue(query, out result);
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
