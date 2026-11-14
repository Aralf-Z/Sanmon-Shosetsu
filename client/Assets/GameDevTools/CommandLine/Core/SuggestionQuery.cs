using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> save current suggestion query information, provide to current command system to get some suggestion back. </summary>
    public readonly struct SuggestionQuery
    {
        /// <summary> None. </summary>
        public static readonly SuggestionQuery NONE = new(SuggestionType.None, string.Empty);

        /// <summary> Suggestion type. </summary>
        public readonly SuggestionType suggestionType;

        /// <summary> Query str. </summary>
        public readonly string queryStr;

        /// <summary> Query type. </summary>
        public readonly Type queryType;

        /// <summary> Create a SuggestionQuery instance. </summary>
        public SuggestionQuery(SuggestionType suggestionType, string queryStr, Type queryType)
        {
            this.suggestionType = suggestionType;
            this.queryStr = queryStr;
            this.queryType = queryType;
        }

        /// <summary> Create a SuggestionQuery instance. </summary>
        public SuggestionQuery(SuggestionType suggestionType, string queryStr) : this(suggestionType, queryStr, null)
        {
        }

        /// <summary> To string. </summary>
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
