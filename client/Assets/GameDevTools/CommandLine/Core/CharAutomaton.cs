using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> lexer use to parse input realtime. </summary>
    public class CharAutomaton
    {
        /// <summary> Current status. </summary>
        public string CurrentStatus => _stateMachine.CurrentStatus;

        private readonly InputBehaviourStateMachine _stateMachine; // State machine.

        private string _lastInput = string.Empty; // Last input.

        /// <summary> Create a CharAutomaton instance. </summary>
        public CharAutomaton(Func<string, Type> getVariableType, Func<string, Type> getCallableType)
        {
            _stateMachine = new InputBehaviourStateMachine(getVariableType, getCallableType);
        }

        /// <summary> input current input text and get suggestion query. </summary>
        public SuggestionQuery Input(string newInput)
        {
            /* input string is empty */
            if (newInput == null || newInput.Length == 0)
            {
                _stateMachine.Reset();
                _lastInput = string.Empty;
                return SuggestionQuery.NONE;
            }

            /* input string is equal to last input, it maybe nothing change 
            or an totally new input */
            if (newInput.Length == _lastInput.Length)
            {
                if (newInput == _lastInput)
                {
                    return _stateMachine.CurrentSuggestionQuery;
                }

                Rehandle(newInput);
                return _stateMachine.CurrentSuggestionQuery;
            }

            /* input some new characters */
            if (newInput.Length > _lastInput.Length)
            {
                /* add some new characters */
                if (newInput.StartsWith(_lastInput))
                {
                    foreach (char c in newInput[_lastInput.Length..])
                    {
                        _stateMachine.StepForward(c);
                    }

                    _lastInput = newInput;
                    return _stateMachine.CurrentSuggestionQuery;
                }

                /* maybe an totally new input */
                Rehandle(newInput);
                return _stateMachine.CurrentSuggestionQuery;
            }

            /* delete some characaters */
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

            /* code never run below */
            return _stateMachine.CurrentSuggestionQuery;
        }

        // Rehandle.
        private void Rehandle(string input)
        {
            _lastInput = input;
            _stateMachine.Reset(input);
        }
    }
}
