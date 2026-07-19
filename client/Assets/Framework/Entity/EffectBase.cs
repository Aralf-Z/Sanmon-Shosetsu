using Game.Config.Logic;
using Sanmon.Core;
using Sanmon.Utility.Set;

namespace Sanmon.Entities
{
    /// <summary>
    /// 这是实体运行时的逻辑类，可以做到逻辑的拆除和添加，Ef前缀
    /// </summary>
    public abstract class EffectBase: 
        IGetModule
        , IBufferItem
    {
        public Entity Host { get; internal set; }
        public abstract int ConfigId { get; }
        public Effect Config => this.Module().Config.Tables.TbEffect[ConfigId];
        public BufferStatus Status { get; private set; }
        public int Order => Config.Order;
        
        public abstract void OnAdd();
        public abstract void OnUpdate(float dt);
        public abstract void OnRemove();

        public void SetStatus(BufferStatus status) => Status = status;
    }
}