namespace RedSaw.CommandLineInterface
{
    /// <summary> Suggestion type. </summary>
    public enum SuggestionType
    {
        /// <summary>no suggestion</summary>
        None,
        /// <summary>search property or local variable</summary>
        Variable,
        /// <summary>search member of a type</summary>
        Member,
        /// <summary>search command</summary>
        Command,
    }
}
