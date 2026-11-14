using System;
using System.Reflection;

namespace RedSaw.CommandLineInterface
{
    /// <summary> character automaton state. </summary>
    abstract class InputBehaviourState
    {
        /// <summary> Should collapse. </summary>
        public bool ShouldCollapse { get; protected set; } = false;

        /// <summary> mark if current state is after an assign operation. </summary>
        public bool AfterAssign { get; protected set; } = false;

        protected readonly InputBehaviourStateMachine _stateMachine; // State machine.

        /// <summary> Create a InputBehaviourState instance. </summary>
        public InputBehaviourState(InputBehaviourStateMachine stateMachine, bool afterAssign = false)
        {
            this._stateMachine = stateMachine;
            this.AfterAssign = afterAssign;
        }

        /// <summary> input new character and check if. c: new input character. </summary>
        public abstract InputBehaviourState StepForward(char c);

        /// <summary> step backward, return to last state. </summary>
        public abstract bool StepBackward();

        /// <summary> get suggestion query. </summary>
        public virtual SuggestionQuery GetSuggestion() => SuggestionQuery.NONE;

        // Wait.
        protected InputBehaviourWaitState Wait() => new(_stateMachine);

        // Unknown.
        protected InputBehaviourUnknownState Unknown() => new(_stateMachine);

        // Variable.
        protected InputBehaviourVariableState Variable() => new(_stateMachine);

        // Variable member.
        protected InputBehaviourState VariableMember(string typeName) => _stateMachine.TryGetVariableMemberState(typeName);

        // Command member.
        protected InputBehaviourState CommandMember(string commandName) => _stateMachine.TryGetCallableMemberState(commandName);

        // Member.
        protected InputBehaviourState Member(Type type, string name)
        {
            MemberInfo member = type.GetDefaultMember(name);
            if (member == null)
            {
                return new InputBehaviourUnknownState(_stateMachine);
            }

            return member.MemberType switch
            {
                MemberTypes.Field => new InputBehaviourMemberState(_stateMachine, ((FieldInfo)member).FieldType),
                MemberTypes.Property => new InputBehaviourMemberState(_stateMachine, ((PropertyInfo)member).PropertyType),
                _ => new InputBehaviourUnknownState(_stateMachine),
            };
        }

        // Callable.
        protected InputBehaviourCommandState Callable(char firstChar, bool shouldCollapse = false, bool afterAssign = false)
        {
            return new InputBehaviourCommandState(_stateMachine, firstChar, shouldCollapse, afterAssign);
        }

        /// <summary> To string. </summary>
        public override string ToString() => "<State>";
    }
}
