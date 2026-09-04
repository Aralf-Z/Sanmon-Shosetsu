using Game.Config.Battle;
using Sanmon.Utility.Set;
using UnityEngine;

namespace Sanmon.Battle
{
    public class Buff: IBufferItem
    {
        public Unit caster;
        public Unit carrier;

        public BuffData data;

        /// <summary> 添加时的时间戳 </summary>
        public float addTimeStamp;
        /// <summary> 当前层数 </summary>
        public int stack;
        /// <summary> 每层计时时间 </summary>
        public float duration;
        /// <summary> 计时器 </summary>
        public float timer;
        /// <summary> 携带时间 </summary>
        public float carryTime;
        
        private BufferStatus _status;
        
        public BufferStatus Status => _status;
        
        public int Order => data.Order;
        
        
        public void OnAdd()
        {
            addTimeStamp = Time.realtimeSinceStartup;
        }

        public void OnUpdate(float dt)
        {
            carryTime += dt;
            timer += dt;
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