using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> Input behaviour member state. </summary>
    class InputBehaviourMemberState : InputBehaviourStateWithInput
    {
        private readonly Type _queryType;

        public InputBehaviourMemberState(InputBehaviourStateMachine stateMachine, Type type) : base(stateMachine, string.Empty)
        {
            _queryType = type;
        }

        public override bool StepBackward()
        {
            if (CurrentInput.Length == 0) return true;

            CurrentInput = CurrentInput[..^1];
            return false;
        }

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
            else if (c == Lexer.DOT) return Member(_queryType, CurrentInput);
            return Callable(c);
        }

        public override SuggestionQuery GetSuggestion()
        {
            return new(SuggestionType.Member, CurrentInput, _queryType);
        }

        public override string ToString() => $"<Member: {CurrentInput}>";
    }
}
