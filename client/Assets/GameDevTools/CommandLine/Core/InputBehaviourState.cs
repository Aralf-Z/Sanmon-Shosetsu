using System;
using System.Reflection;

namespace RedSaw.CommandLineInterface
{
    /// <summary> character automaton state. </summary>
    abstract class InputBehaviourState
    {
        public bool ShouldCollapse { get; protected set; } = false;

        public bool AfterAssign { get; protected set; } = false;

        protected readonly InputBehaviourStateMachine _stateMachine;

        public InputBehaviourState(InputBehaviourStateMachine stateMachine, bool afterAssign = false)
        {
            this._stateMachine = stateMachine;
            this.AfterAssign = afterAssign;
        }

        public abstract InputBehaviourState StepForward(char c);

        public abstract bool StepBackward();

        public virtual SuggestionQuery GetSuggestion() => SuggestionQuery.NONE;

        protected InputBehaviourWaitState Wait() => new(_stateMachine);

        protected InputBehaviourUnknownState Unknown() => new(_stateMachine);

        protected InputBehaviourVariableState Variable() => new(_stateMachine);

        protected InputBehaviourState VariableMember(string typeName) => _stateMachine.TryGetVariableMemberState(typeName);

        protected InputBehaviourState CommandMember(string commandName) => _stateMachine.TryGetCallableMemberState(commandName);

        protected InputBehaviourState Member(Type type, string name)
        {
            MemberInfo member = type.GetDefaultMember(name);

            if (member == null) return new InputBehaviourUnknownState(_stateMachine);

            return member.MemberType switch
            {
                MemberTypes.Field => new InputBehaviourMemberState(_stateMachine, ((FieldInfo)member).FieldType),
                MemberTypes.Property => new InputBehaviourMemberState(_stateMachine, ((PropertyInfo)member).PropertyType),
                _ => new InputBehaviourUnknownState(_stateMachine),
            };
        }

        protected InputBehaviourCommandState Callable(char firstChar, bool shouldCollapse = false, bool afterAssign = false)
        {
            return new InputBehaviourCommandState(_stateMachine, firstChar, shouldCollapse, afterAssign);
        }

        public override string ToString() => "<State>";
    }
}
