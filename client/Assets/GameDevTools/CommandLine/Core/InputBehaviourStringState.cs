namespace RedSaw.CommandLineInterface
{
    /// <summary> Input behaviour string state. </summary>
    class InputBehaviourStringState : InputBehaviourStateWithInput
    {
        private readonly char _quote; // Quote.

        /// <summary> Create a InputBehaviourStringState instance. </summary>
        public InputBehaviourStringState(InputBehaviourStateMachine stateMachine, char quote, string input = "") : base(stateMachine, input)
        {
            this._quote = quote;
        }

        /// <summary> Step forward. </summary>
        public override InputBehaviourState StepForward(char c)
        {
            if (c == _quote)
                return Wait();
            CurrentInput += c;
            return this;
        }

        /// <summary> Step backward. </summary>
        public override bool StepBackward()
        {
            if (CurrentInput.Length == 0)
                return true;
            CurrentInput = CurrentInput[..^1];
            return false;
        }

        /// <summary> To string. </summary>
        public override string ToString() => $"<String: {CurrentInput}>";
    }
}
