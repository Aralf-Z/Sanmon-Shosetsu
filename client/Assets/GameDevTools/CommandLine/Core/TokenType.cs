namespace RedSaw.CommandLineInterface
{
    /// <summary> token type of lexer. </summary>
    public enum TokenType
    {
        None, // nothing 
        Eof, // end of file
        Var, // @<name>
        Input, // input
        String, // "string"
        True, // true
        False, // false
        Null, // null
        Int, // 123
        Float, // 1.3        
        Assign, // =
        Dot, // .
        Comma, // ,
        Colon, // :
        LBracket, // [
        RBracket, // ]
        LParen, // (
        RParen, // )
    }
}
