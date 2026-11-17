namespace RedSaw.CommandLineInterface
{
    /// <summary> Input behaviour string state. </summary>
    class InputBehaviourStringState : InputBehaviourStateWithInput
    {
        private readonly char _quote;

        public InputBehaviourStringState(InputBehaviourStateMachine stateMachine, char quote, string input = "") : base(stateMachine, input)
        {
            this._quote = quote;
        }

        public override InputBehaviourState StepForward(char c)
        {
            if (c == _quote) return Wait();

            CurrentInput += c;
            return this;
        }

        public override bool StepBackward()
        {
            if (CurrentInput.Length == 0) return true;

            CurrentInput = CurrentInput[..^1];
            return false;
        }

        public override string ToString() => $"<String: {CurrentInput}>";
    }
}
