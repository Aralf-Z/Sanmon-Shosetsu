using Game.Config.Battle;
using Sanmon.Utility.Set;

namespace Framework.Battle
{
    public class Buff: IBufferItem
    {
        public BattleUnit caster;
        public BattleUnit carrier;

        public BuffData data;

        /// <summary>
        /// 当前层数
        /// </summary>
        public int stack;
        /// <summary>
        /// 每层计时时间
        /// </summary>
        public float duration;
        /// <summary>
        /// 计时器
        /// </summary>
        public float timer;
        /// <summary>
        /// 携带时间
        /// </summary>
        public float carryTime;
        
        private BufferStatus _status;
        
        public BufferStatus Status => _status;
        
        public int Order => data.Order;
        
        
        public void OnAdd()
        {
            
        }

        public void OnUpdate(float dt)
        {
            
        }

        public void OnRemove()
        {
            
        }

        public void SetStatus(BufferStatus status)
        {
            _status = status;
        }
    }
}