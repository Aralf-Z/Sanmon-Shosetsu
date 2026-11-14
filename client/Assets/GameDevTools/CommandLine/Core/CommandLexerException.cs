namespace RedSaw.CommandLineInterface
{
    /// <summary> command lexer exception. </summary>
    public class CommandLexerException : CommandSystemException
    {
        /// <summary> Create a CommandLexerException instance. </summary>
        public CommandLexerException(string message) : base(message)
        {
        }
    }
}
