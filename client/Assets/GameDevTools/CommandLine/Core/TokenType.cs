namespace RedSaw.CommandLineInterface
{
    /// <summary> token type of lexer. </summary>
    public enum TokenType
    {
        None,
        Eof,
        Var,
        Input,
        String,
        True,
        False,
        Null, // null
        Int,
        Float,
        Assign,
        Dot,
        Comma,
        Colon,
        LBracket,
        RBracket,
        LParen,
        RParen,
    }
}
