namespace RedSaw.CommandLineInterface
{
    /// <summary> command lexer exception. </summary>
    public class CommandLexerException : CommandSystemException
    {
        public CommandLexerException(string message) : base(message)
        {
        }
    }
}
