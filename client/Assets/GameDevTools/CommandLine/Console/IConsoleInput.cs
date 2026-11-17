namespace RedSaw.CommandLineInterface
{
    /// <summary> define the user input of CommandConsole, cause Unity has two different input solutions. </summary>
    public interface IConsoleInput
    {
        bool MoveUp { get; }

        bool MoveDown { get; }

        bool Focus { get; }

        bool QuitFocus { get; }

        bool ShowOrHide { get; }
    }
}
