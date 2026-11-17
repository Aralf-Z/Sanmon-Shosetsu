using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> save current suggestion query information, provide to current command system to get some suggestion back. </summary>
    public readonly struct SuggestionQuery
    {
        public static readonly SuggestionQuery NONE = new(SuggestionType.None, string.Empty);

        public readonly SuggestionType suggestionType;

        public readonly string queryStr;

        public readonly Type queryType;

        public SuggestionQuery(SuggestionType suggestionType, string queryStr, Type queryType)
        {
            this.suggestionType = suggestionType;
            this.queryStr = queryStr;
            this.queryType = queryType;
        }

        public SuggestionQuery(SuggestionType suggestionType, string queryStr) : this(suggestionType, queryStr, null)
        {
        }

        public override string ToString()
        {
            return suggestionType switch
            {
                SuggestionType.Variable => $"Variable -> \"{queryStr}\"",
                SuggestionType.Command => $"Command -> \"{queryStr}\"",
                SuggestionType.Member => $"Member -> \"{queryType.Name}.{queryStr}\"",
                _ => "No Suggestions",
            };
        }
    }
}
