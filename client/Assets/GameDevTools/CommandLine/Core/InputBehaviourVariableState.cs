namespace RedSaw.CommandLineInterface
{
    /// <summary> Input behaviour variable state. </summary>
    class InputBehaviourVariableState : InputBehaviourStateWithInput
    {
        /// <summary> Create a InputBehaviourVariableState instance. </summary>
        public InputBehaviourVariableState(InputBehaviourStateMachine stateMachine, string input = "") : base(stateMachine, input)
        {
        }

        /// <summary> Step forward. </summary>
        public override InputBehaviourState StepForward(char c)
        {
            if (CurrentInput.Length == 0)
            {
                if (char.IsLetter(c) || c == Lexer.UNDERLINE)
                {
                    CurrentInput += c;
                    return this;
                }

                return Unknown();
            }

            if (char.IsLetterOrDigit(c) || c == Lexer.UNDERLINE)
            {
                CurrentInput += c;
                return this;
            }

            if (c == Lexer.DOT)
            {
                return VariableMember(CurrentInput);
            }

            return Unknown();
        }

        /// <summary> Step backward. </summary>
        public override bool StepBackward()
        {
            if (CurrentInput.Length == 0)
                return true;
            CurrentInput = CurrentInput[..^1];
            return false;
        }

        /// <summary> Get suggestion. </summary>
        public override SuggestionQuery GetSuggestion() => new(SuggestionType.Variable, CurrentInput);

        /// <summary> To string. </summary>
        public override string ToString() => $"<Variable: {CurrentInput}>";
    }
}
