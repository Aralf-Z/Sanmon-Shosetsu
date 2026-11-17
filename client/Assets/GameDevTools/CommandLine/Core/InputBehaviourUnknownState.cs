namespace RedSaw.CommandLineInterface
{
    /// <summary> bad state only use to mark the input is bad, and wait for user delete all and reinput. </summary>
    class InputBehaviourUnknownState : InputBehaviourState
    {
        private int _count;

        public InputBehaviourUnknownState(InputBehaviourStateMachine stateMachine) : base(stateMachine)
        {
            this._count = 0;
        }

        public override InputBehaviourState StepForward(char c)
        {
            _count++;
            return this;
        }

        public override bool StepBackward()
        {
            return --_count < 0;
        }

        public override string ToString() => "<Unknown>";
    }
}
