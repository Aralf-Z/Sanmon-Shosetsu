using Sanmon.Utility.Set;

namespace Sanmon.GameEntity
{
    /// <summary>
    /// 这是实体运行时的功能类，Fc做前缀
    /// </summary>
    public abstract class FunctionBase
    {
        public Entity Host { get; internal set; }
        
        public abstract void OnAdded();
        public abstract void OnLogicUpdate(float dt);
        public abstract void OnRemoved();
    }
}