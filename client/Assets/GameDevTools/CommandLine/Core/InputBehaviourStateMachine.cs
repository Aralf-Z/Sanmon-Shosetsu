using System;
using System.Collections.Generic;

namespace RedSaw.CommandLineInterface
{
    /// <summary> use to analyze input string and provide suggestions type. </summary>
    class InputBehaviourStateMachine
    {
        public readonly Stack<InputBehaviourState> stateStack;

        public string CurrentStatus
        {
            get
            {
                if (_currentState == null) return "<No State>";

                return _currentState.ToString();
            }
        }

        public SuggestionQuery CurrentSuggestionQuery => _currentState.GetSuggestion();

        private readonly Func<string, Type> _getVariableType;

        private readonly Func<string, Type> _getCallableType;

        private InputBehaviourState _currentState;

        public InputBehaviourStateMachine(Func<string, Type> getVariableType, Func<string, Type> getCallableType)
        {
            this._getVariableType = getVariableType;
            this._getCallableType = getCallableType;
            stateStack = new Stack<InputBehaviourState>();
            _currentState = new InputBehaviourReadyState(this);
        }

        public InputBehaviourState TryGetVariableMemberState(string variableName)
        {
            var type = _getVariableType(variableName);

            if (type == null) return new InputBehaviourUnknownState(this);

            return new InputBehaviourMemberState(this, type);
        }

        public InputBehaviourState TryGetCallableMemberState(string callableName)
        {
            var type = _getCallableType(callableName);

            if (type == null) return new InputBehaviourUnknownState(this);

            return new InputBehaviourMemberState(this, type);
        }

        public void StepForward(char c)
        {
            var nextState = _currentState.StepForward(c);

            if (nextState != _currentState)
            {
                if (nextState.ShouldCollapse)
                {
                    stateStack.Pop();
                }

                stateStack.Push(_currentState);
                _currentState = nextState;
            }
        }

        public void StepBackward()
        {
            if (_currentState.StepBackward())
            {
                _currentState = stateStack.Pop();
            }
        }

        public void Reset()
        {
            stateStack.Clear();
            _currentState = new InputBehaviourReadyState(this);
        }

        public void Reset(string input)
        {
            Reset();
            foreach (char c in input)
            {
                StepForward(c);
            }
        }
    }
}
