namespace RedSaw.CommandLineInterface
{
    /// <summary> command execute exception. </summary>
    public class CommandExecuteException : CommandSystemException
    {
        /// <summary> Create a CommandExecuteException instance. </summary>
        public CommandExecuteException(string message) : base(message)
        {
        }
    }
}
