namespace RedSaw.CommandLineInterface
{
    /// <summary> command system exception. </summary>
    public class CommandSystemException : System.Exception
    {
        /// <summary> Create a CommandSystemException instance. </summary>
        public CommandSystemException(string message) : base(message)
        {
        }
    }
}
