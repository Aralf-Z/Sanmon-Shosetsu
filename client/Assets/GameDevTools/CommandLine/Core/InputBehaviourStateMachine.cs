using System;
using System.Collections.Generic;

namespace RedSaw.CommandLineInterface
{
    /// <summary> use to analyze input string and provide suggestions type. </summary>
    class InputBehaviourStateMachine
    {
        /// <summary> State stack. </summary>
        public readonly Stack<InputBehaviourState> stateStack;

        /// <summary> Current status. </summary>
        public string CurrentStatus
        {
            get
            {
                if (_currentState == null)
                    return "<No State>";
                return _currentState.ToString();
            }
        }

        /// <summary> current suggestion. </summary>
        public SuggestionQuery CurrentSuggestionQuery => _currentState.GetSuggestion();

        private readonly Func<string, Type> _getVariableType; // Get variable type.

        private readonly Func<string, Type> _getCallableType; // Get callable type.

        private InputBehaviourState _currentState; // Current state.

        /// <summary> Create a InputBehaviourStateMachine instance. </summary>
        public InputBehaviourStateMachine(Func<string, Type> getVariableType, Func<string, Type> getCallableType)
        {
            this._getVariableType = getVariableType;
            this._getCallableType = getCallableType;
            stateStack = new Stack<InputBehaviourState>();
            _currentState = new InputBehaviourReadyState(this);
        }

        /// <summary> Try get variable member state. </summary>
        public InputBehaviourState TryGetVariableMemberState(string variableName)
        {
            var type = _getVariableType(variableName);
            if (type == null)
                return new InputBehaviourUnknownState(this);
            return new InputBehaviourMemberState(this, type);
        }

        /// <summary> Try get callable member state. </summary>
        public InputBehaviourState TryGetCallableMemberState(string callableName)
        {
            var type = _getCallableType(callableName);
            if (type == null)
                return new InputBehaviourUnknownState(this);
            return new InputBehaviourMemberState(this, type);
        }

        /// <summary> Step forward. </summary>
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

        /// <summary> Step backward. </summary>
        public void StepBackward()
        {
            if (_currentState.StepBackward())
            {
                _currentState = stateStack.Pop();
            }
        }

        /// <summary> Reset. </summary>
        public void Reset()
        {
            stateStack.Clear();
            _currentState = new InputBehaviourReadyState(this);
        }

        /// <summary> Reset. </summary>
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
