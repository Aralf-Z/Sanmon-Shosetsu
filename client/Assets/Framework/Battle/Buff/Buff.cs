using Game.Config.Battle;
using Sanmon.Core;
using Sanmon.Module;
using Sanmon.Utility.Set;
using UnityEngine;

namespace Sanmon.Battle
{
    public class Buff: IBufferItem
    , IGetModule
    {
        internal Buff(int id)
        {
            data = this.Module().Config.Tables.TbBuffData[id];
            stack = data.StartStack;
        }

        public Unit caster;
        public Unit carrier;

        public BuffData data;

        /// <summary> 添加时的时间戳 </summary>
        public float AddTimeStamp { get;  private set; }
        /// <summary> 当前层数 </summary>
        public float stack;
        /// <summary> 计时器 </summary>
        public float timer;
        /// <summary> 计时器总时长 </summary>
        public float timerDuration;
        /// <summary> 携带时间 </summary>
        public float carryTime;

        #region IBufferItem

        private BufferStatus _status;
        
        public BufferStatus Status => _status;
        
        public int Order => 0;
        
        
        void IBufferItem.OnAdd()
        {
            
        }

        void IBufferItem.OnUpdate(float dt)
        {
            carryTime += dt;
            timer += dt;
        }

        void IBufferItem.OnRemove()
        {
            
        }

        void IBufferItem.SetStatus(BufferStatus status)
        {
            _status = status;
        }

        #endregion
    }
}