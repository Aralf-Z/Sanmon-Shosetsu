namespace Sanmon.Utility.FSM
{
    public abstract class StatusBase
    {
        public abstract void Init<T>(T host) where T : IStatusHost;
        public virtual void OnEnter(){}
        public virtual void OnUpdate(float dt){}
        public virtual void OnFixedUpdate(float dt){}
        public virtual void OnExit(){}
    }
}