namespace RedSaw.CommandLineInterface
{
    /// <summary> Input behaviour command state. </summary>
    class InputBehaviourCommandState : InputBehaviourStateWithInput
    {
        public InputBehaviourCommandState(InputBehaviourStateMachine stateMachine, char firstChar, bool shouldCollapse = false, bool afterAssign = false) : base(stateMachine, firstChar.ToString(), afterAssign)
        {
            CurrentInput = firstChar.ToString();
            this.ShouldCollapse = shouldCollapse;
        }

        public override InputBehaviourState StepForward(char c)
        {
            if (Lexer.IsWhiteSpace(c)) return Wait();

            if (c == Lexer.DOT) return CommandMember(CurrentInput);

            if (c == Lexer.UNDERLINE || char.IsLetterOrDigit(c))
            {
                CurrentInput += c;
                return this;
            }

            return Unknown();
        }

        public override bool StepBackward()
        {
            if (CurrentInput.Length == 0) return true;

            CurrentInput = CurrentInput[..^1];
            return false;
        }

        public override SuggestionQuery GetSuggestion() => new(SuggestionType.Command, CurrentInput);

        public override string ToString() => $"<Command: {CurrentInput}>";
    }
}
