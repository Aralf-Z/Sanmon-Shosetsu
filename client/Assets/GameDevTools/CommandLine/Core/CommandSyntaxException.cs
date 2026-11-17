namespace RedSaw.CommandLineInterface
{
    /// <summary> command syntax exception. </summary>
    public class CommandSyntaxException : CommandSystemException
    {
        public CommandSyntaxException(string message) : base(message)
        {
        }
    }
}
