/* MIT License - CommandSystem.cs; Created By : Prince Biscuit */
using System;
using System.Linq;

namespace RedSaw.CommandLineInterface
{
    /// <summary> Command system. </summary>
    public partial class CommandSystem
    {
        public readonly VirtualMachine vm;

        private readonly Lexer _lexer;

        private readonly SyntaxAnalyzer _syntaxAnalyzer;

        private readonly float _scoreThresholdCommand;

        private readonly float _scoreThresholdVariable;

        private readonly float _scoreThresholdType;

        private readonly QueryCache<Suggestion> _commandQueryCache;

        private readonly QueryCache<Suggestion> _variableQueryCache;

        private readonly QueryCache<Suggestion> _typeQueryCache;

        private readonly CharAutomaton _charAutomaton;

        private string _lastQueryStr = string.Empty;

        public void RegisterCommand(Command command) => vm.RegisterCallable(command);

        public void RegisterProperty(StackProperty property) => vm.RegisterProperty(property);

        public void RegisterValueParser(Type type, ValueParser parser, string alias = null) => vm.RegisterValueParser(type, parser, alias);

        public void RegisterValueParser<TType>(ValueParser parser, string alias = null) => vm.RegisterValueParser(typeof(TType), parser, alias);

        public object GetLocalVariable(string name) => vm.GetLocalVariable(name);

        public void SetLocalVariable(string name, object value) => vm.SetLocalVariable(name, value);

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

        public string TakeSuggestion(string currentInput, string primary)
        {
            if (_lastQueryStr == string.Empty) return currentInput + primary;

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

        public Suggestion[] QueryCommands(string query, int count, Func<string, string, float> scoreFunc, string tag = null)
        {
            query ??= string.Empty;
            var queryDetailInfo = query + ":" + tag ?? string.Empty;

            if (_commandQueryCache.GetCache(queryDetailInfo, out var result)) return result;

            var bestChoices = vm.AllCallables.Where(s => s is Command command && command.CompareTag(tag))
                .Select(s => new { value = s, score = scoreFunc(query, s.Name) })
                .Where(s => s.score > _scoreThresholdCommand)
                .OrderByDescending(s => s.score)
                .Take(Math.Max(1, count));

            result = bestChoices.Select((s) =>
            {
                var c = (Command)s.value;

                return new Suggestion(c.Name, c.description);
            })
                .ToArray();
            _commandQueryCache.Cache(queryDetailInfo, result);
            return result;
        }

        public Suggestion[] QueryVariable(string query, int count, Func<string, string, float> scoreFunc, string tag = null)
        {
            query ??= string.Empty;
            var queryDetailInfo = query + ":" + tag ?? string.Empty;

            if (_variableQueryCache.GetCache(queryDetailInfo, out var result)) return result;

            var bestChoices = vm.AllProperties.Where(s => s.CompareTag(tag))
                .Select(s => new { value = s, score = scoreFunc(query, s.Name) })
                .Where(s => s.score > _scoreThresholdVariable)
                .OrderByDescending(s => s.score)
                .Take(Math.Max(1, count));

            result = bestChoices.Select(s => new Suggestion(s.value.Name, s.value.description))
                .ToArray();
            _variableQueryCache.Cache(queryDetailInfo, result);
            return result;
        }

        public Suggestion[] QueryType(string query, Type type, int count, Func<string, string, float> scoreFunc)
        {
            query ??= string.Empty;
            var queryDetailInfo = $"{type.Name}.{query}";

            if (_typeQueryCache.GetCache(queryDetailInfo, out var result)) return result;

            var bestChoices = type.GetMembers()
                .Select(s => new { value = s, score = scoreFunc(query, s.Name) })
                .Where(s => s.score > _scoreThresholdType)
                .OrderByDescending(s => s.score)
                .Take(Math.Max(1, count));

            result = bestChoices.Select(s => new Suggestion(s.value.Name, s.value.GetMemberTypeName()))
                .ToArray();
            _typeQueryCache.Cache(queryDetailInfo, result);
            return result;
        }
    }
}
