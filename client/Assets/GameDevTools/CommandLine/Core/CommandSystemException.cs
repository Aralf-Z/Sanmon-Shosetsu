namespace RedSaw.CommandLineInterface
{
    /// <summary> command system exception. </summary>
    public class CommandSystemException : System.Exception
    {
        public CommandSystemException(string message) : base(message)
        {
        }
    }
}
