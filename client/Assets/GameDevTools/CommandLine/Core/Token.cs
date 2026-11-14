namespace RedSaw.CommandLineInterface
{
    /// <summary> lexer token. </summary>
    public readonly struct Token
    {
        /// <summary> Type. </summary>
        public readonly TokenType type;

        /// <summary> Source idx. </summary>
        public readonly int sourceIdx;

        /// <summary> Start idx. </summary>
        public readonly int startIdx;

        /// <summary> End idx. </summary>
        public readonly int endIdx;

        /// <summary> Create a Token instance. </summary>
        public Token(TokenType type, int sourceId, int startIdx, int endIdx)
        {
            this.type = type;
            this.sourceIdx = sourceId;
            this.startIdx = startIdx;
            this.endIdx = endIdx;
        }
    }
}
