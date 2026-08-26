using Game.Config.Battle;

namespace Sanmon.Battle
{
    public struct DamagePair
    {
        public DamageType type;
        public int value;

        public override string ToString()
        {
            return $"[{type}: {value}]";
        }
    }
}