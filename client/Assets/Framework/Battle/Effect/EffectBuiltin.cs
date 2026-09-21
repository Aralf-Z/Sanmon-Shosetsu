using System.Collections.Generic;
using Game.Config.Battle;
using Sanmon.Utility.Math;

namespace Sanmon.Battle
{
    internal static partial class EffectBuiltin
    {
        public static readonly List<Effect> effects;

        static EffectBuiltin()
        {
            effects = new ()
            {
                damageEffect,
            };
        }
    }
}