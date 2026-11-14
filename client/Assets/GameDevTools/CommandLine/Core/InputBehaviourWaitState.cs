namespace RedSaw.CommandLineInterface
{
    /// <summary> Input behaviour wait state. </summary>
    class InputBehaviourWaitState : InputBehaviourStateWithInput
    {
        /// <summary> Create a InputBehaviourWaitState instance. </summary>
        public InputBehaviourWaitState(InputBehaviourStateMachine stateMachine, string input = "") : base(stateMachine, input)
        {
        }

        /// <summary> Step forward. </summary>
        public override InputBehaviourState StepForward(char c)
        {
            switch (c)
            {
                case Lexer.VAR:
                    return Variable();
                case Lexer.DOUBLE_QUOTE:
                case Lexer.SINGLE_QUOTE:
                    return new InputBehaviourStringState(_stateMachine, c);
                default:
                    if (Lexer.IsWhiteSpace(c))
                        return Wait();
                    if (c == Lexer.ASSIGN)
                        return Wait();
                    if (char.IsLetter(c) || c == Lexer.UNDERLINE)
                        return Callable(c, shouldCollapse: true, AfterAssign);
                    return Unknown();
            }
        }

        /// <summary> Step backward. </summary>
        public override bool StepBackward() => true;

        /// <summary> To string. </summary>
        public override string ToString() => $"<Wait>";
    }
}
