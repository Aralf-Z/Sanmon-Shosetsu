using Game.Config.Battle;

namespace Sanmon.Battle
{
    public class DamagePair
    {
        public DamageType type;
        /// <summary> 基础数值 </summary>
        public float value;
        /// <summary> 加区 </summary>
        public float addValue;
        /// <summary> 乘区 </summary>
        public float mulValue;

        public float deductionRatio;

        public float deductionValue;

        public override string ToString()
        {
            return $"[{type}={value}, 加区={addValue}, 乘区={mulValue}]";
        }
    }
}