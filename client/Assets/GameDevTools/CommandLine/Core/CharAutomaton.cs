using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> lexer use to parse input realtime. </summary>
    public class CharAutomaton
    {
        public string CurrentStatus => _stateMachine.CurrentStatus;

        private readonly InputBehaviourStateMachine _stateMachine;

        private string _lastInput = string.Empty;

        public CharAutomaton(Func<string, Type> getVariableType, Func<string, Type> getCallableType)
        {
            _stateMachine = new InputBehaviourStateMachine(getVariableType, getCallableType);
        }

        public SuggestionQuery Input(string newInput)
        {
            if (newInput == null || newInput.Length == 0)
            {
                _stateMachine.Reset();
                _lastInput = string.Empty;
                return SuggestionQuery.NONE;
            }

            if (newInput.Length == _lastInput.Length)
            {
                if (newInput == _lastInput) return _stateMachine.CurrentSuggestionQuery;

                Rehandle(newInput);
                return _stateMachine.CurrentSuggestionQuery;
            }

            if (newInput.Length > _lastInput.Length)
            {
                if (newInput.StartsWith(_lastInput))
                {
                    foreach (char c in newInput[_lastInput.Length..])
                    {
                        _stateMachine.StepForward(c);
                    }

                    _lastInput = newInput;
                    return _stateMachine.CurrentSuggestionQuery;
                }

                Rehandle(newInput);
                return _stateMachine.CurrentSuggestionQuery;
            }

            if (newInput.Length < _lastInput.Length)
            {
                if (_lastInput.StartsWith(newInput))
                {
                    foreach (char _ in _lastInput[newInput.Length..])
                    {
                        _stateMachine.StepBackward();
                    }

                    _lastInput = newInput;
                    return _stateMachine.CurrentSuggestionQuery;
                }

                Rehandle(newInput);
                return _stateMachine.CurrentSuggestionQuery;
            }

            // code never run below
            return _stateMachine.CurrentSuggestionQuery;
        }

        private void Rehandle(string input)
        {
            _lastInput = input;
            _stateMachine.Reset(input);
        }
    }
}
