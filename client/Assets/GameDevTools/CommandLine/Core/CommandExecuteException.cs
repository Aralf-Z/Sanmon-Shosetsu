namespace RedSaw.CommandLineInterface
{
    /// <summary> command execute exception. </summary>
    public class CommandExecuteException : CommandSystemException
    {
        public CommandExecuteException(string message) : base(message)
        {
        }
    }
}
