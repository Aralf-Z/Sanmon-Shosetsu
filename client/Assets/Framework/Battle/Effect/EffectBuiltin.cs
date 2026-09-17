using System.Collections.Generic;
using Game.Config.Battle;
using Sanmon.Utility.Math;
using Logger = Sanmon.Helper.Logger;

namespace Sanmon.Battle
{
    internal static partial class EffectBuiltin
    {
        public static readonly List<Effect> effects = new ()
        {
            damageEffect,
        };
    }
}