namespace RedSaw.CommandLineInterface
{
    /// <summary> bad state only use to mark the input is bad, and wait for user delete all and reinput. </summary>
    class InputBehaviourUnknownState : InputBehaviourState
    {
        private int _count; // Count.

        /// <summary> Create a InputBehaviourUnknownState instance. </summary>
        public InputBehaviourUnknownState(InputBehaviourStateMachine stateMachine) : base(stateMachine)
        {
            this._count = 0;
        }

        /// <summary> Step forward. </summary>
        public override InputBehaviourState StepForward(char c)
        {
            _count++;
            return this;
        }

        /// <summary> Step backward. </summary>
        public override bool StepBackward()
        {
            return --_count < 0;
        }

        /// <summary> To string. </summary>
        public override string ToString() => "<Unknown>";
    }
}
