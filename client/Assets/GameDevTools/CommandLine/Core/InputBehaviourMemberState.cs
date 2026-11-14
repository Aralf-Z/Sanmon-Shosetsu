using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> Input behaviour member state. </summary>
    class InputBehaviourMemberState : InputBehaviourStateWithInput
    {
        private readonly Type _queryType; // Query type.

        /// <summary> Create a InputBehaviourMemberState instance. </summary>
        public InputBehaviourMemberState(InputBehaviourStateMachine stateMachine, Type type) : base(stateMachine, string.Empty)
        {
            _queryType = type;
        }

        /// <summary> Step backward. </summary>
        public override bool StepBackward()
        {
            if (CurrentInput.Length == 0)
                return true;
            CurrentInput = CurrentInput[..^1];
            return false;
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
            else if (c == Lexer.DOT)
            {
                return Member(_queryType, CurrentInput);
            }

            return Callable(c);
        }

        /// <summary> Get suggestion. </summary>
        public override SuggestionQuery GetSuggestion()
        {
            return new(SuggestionType.Member, CurrentInput, _queryType);
        }

        /// <summary> To string. </summary>
        public override string ToString() => $"<Member: {CurrentInput}>";
    }
}
