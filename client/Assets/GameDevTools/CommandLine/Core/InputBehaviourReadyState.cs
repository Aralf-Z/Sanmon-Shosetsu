namespace RedSaw.CommandLineInterface
{
    /// <summary> Input behaviour ready state. </summary>
    class InputBehaviourReadyState : InputBehaviourStateWithInput
    {
        /// <summary> Create a InputBehaviourReadyState instance. </summary>
        public InputBehaviourReadyState(InputBehaviourStateMachine stateMachine) : base(stateMachine)
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
                case Lexer.UNDERLINE:
                    return Callable(c, shouldCollapse: true, AfterAssign);
                default:
                    if (Lexer.IsWhiteSpace(c))
                        return Wait();
                    if (c == Lexer.UNDERLINE || char.IsLetter(c))
                        return Callable(c);
                    return Unknown();
            }
        }

        /// <summary> Step backward. </summary>
        public override bool StepBackward() => false;

        /// <summary> To string. </summary>
        public override string ToString() => "<Ready>";
    }
}
