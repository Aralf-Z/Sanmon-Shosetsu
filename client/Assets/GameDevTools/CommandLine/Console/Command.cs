/*
MIT License - CommandSystem.cs
Created By : Prince Biscuit
Created Date : 2024/01/01

Description : CommandSystem.cs is a part of GameCLI package. 
              use for any game that need a command line interface.

version : 0.2.0
@2024.01.28 : Totally refactor the CommandSystem.cs, now it has more functions

*/

using System.Reflection;

namespace RedSaw.CommandLineInterface
{
    /// <summary> command, usually represent by an static method. </summary>
    public class Command : StackMethod
    {
        /// <summary> Name. </summary>
        public readonly string name;

        /// <summary> Description. </summary>
        public readonly string description;

        /// <summary> Tag. </summary>
        public readonly string tag;

        /// <summary> Name. </summary>
        public override string Name => name;

        /// <summary> static method command. method: the method info, must be a static method. </summary>
        public Command(string name, string description, string tag, MethodInfo method) : base(null, method)
        {
            this.name = name;
            this.description = description;
            this.tag = tag;
        }

        /// <summary> instance method command. instance: the instance of method. method: the method info. </summary>
        public Command(string name, string description, string tag, object instance, MethodInfo method) : base(instance, method)
        {
            this.name = name;
            this.description = description;
            this.tag = tag;
        }

        /// <summary> check if this command has given tag. </summary>
        public bool CompareTag(string tag)
        {
            if (tag == null || tag.Length == 0)
                return true;
            return this.tag == tag;
        }

        /// <summary> To string. </summary>
        public override string ToString()
        {
            return $"{name} : {description}";
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
