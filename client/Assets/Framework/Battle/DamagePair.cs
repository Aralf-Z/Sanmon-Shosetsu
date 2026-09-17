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
        /// <summary> 伤害减免率 </summary>
        public float deductionRatio;
        /// <summary> 伤害抵抗值 </summary>
        public float deductionValue;

        public override string ToString()
        {
            return $"[{type}={value}, 加区={addValue:#0}, 乘区={mulValue:#0}, 伤害减免率={deductionRatio:##0.00%}, 伤害抵抗值={deductionValue:#0}]";
        }
    }
}