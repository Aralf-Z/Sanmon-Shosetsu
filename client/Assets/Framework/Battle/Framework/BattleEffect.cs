using System;
using Sanmon.GameEntity;

namespace Sanmon.Battle
{
    internal class BattleEffect: Effect
    {
        public int order;

        public string methodName;

        public Action<DamageInfo> method;

        internal BattleEffect(int id) : base(id) { }
    }
}