namespace RedSaw.CommandLineInterface
{
    /// <summary> command syntax exception. </summary>
    public class CommandSyntaxException : CommandSystemException
    {
        /// <summary> Create a CommandSyntaxException instance. </summary>
        public CommandSyntaxException(string message) : base(message)
        {
        }
    }
}
