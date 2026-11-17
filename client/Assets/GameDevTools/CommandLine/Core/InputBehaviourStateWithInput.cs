namespace RedSaw.CommandLineInterface
{
    /// <summary> Input behaviour state with input. </summary>
    abstract class InputBehaviourStateWithInput : InputBehaviourState
    {
        public string CurrentInput { get; protected set; } = string.Empty;

        public InputBehaviourStateWithInput(InputBehaviourStateMachine stateMachine, string input = "", bool afterAssign = false) : base(stateMachine, afterAssign)
        {
            this.CurrentInput = input;
        }

        public override string ToString() => $"<State: {CurrentInput}>";
    }
}
