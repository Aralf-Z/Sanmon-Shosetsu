namespace RedSaw.CommandLineInterface
{
    /// <summary> Input behaviour state with input. </summary>
    abstract class InputBehaviourStateWithInput : InputBehaviourState
    {
        /// <summary> Current input. </summary>
        public string CurrentInput { get; protected set; } = string.Empty;

        /// <summary> Create a InputBehaviourStateWithInput instance. </summary>
        public InputBehaviourStateWithInput(InputBehaviourStateMachine stateMachine, string input = "", bool afterAssign = false) : base(stateMachine, afterAssign)
        {
            this.CurrentInput = input;
        }

        /// <summary> To string. </summary>
        public override string ToString() => $"<State: {CurrentInput}>";
    }
}
