using Game.Config.Logic;
using Sanmon.Core;

namespace Sanmon.Entity
{
    /// <summary>
    /// 这是实体运行时的逻辑类，可以做到逻辑的拆除和添加
    /// </summary>
    public abstract class EffectBase: 
        IGetModule
    {
        public abstract int ConfigId { get; protected set; }
        
        public Effect Config => this.Module().Config.Tables.TbEffect[ConfigId];
        
        public EmEffectStatus Status { get; protected set; } = EmEffectStatus.None;
        
        protected internal virtual void Update(float dt)
        {
            
        }
    }
}