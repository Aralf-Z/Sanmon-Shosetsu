using System.Collections.Generic;
using Game.Config.Battle;
using Sanmon.Utility.Math;
using Logger = Sanmon.Helper.Logger;

namespace Sanmon.Battle
{
    internal static partial class EffectBuiltin
    {
        public const string DAMAGE_PIPELINE = "default_damage_pipeline";
        public const string HEAL_PIPELINE = "default_heal_pipeline";

        private static readonly Dice _dice = new Dice();
        private static readonly Dices _d20 = new Dices(20);

        private static readonly Effect damageEffect = new Effect()
        {
            name = DAMAGE_PIPELINE,
            script = Script.CSharp,
            events = new EffectEvent[]
            {
                new()
                {
                    name = DealDamageEvent.HIT_ATTACKER_BEFORE_HIT,
                    damageAction = d =>
                    {
                        if (d.defender.tag.Check(Tag.Dead))
                            d.isAbort = true;
                    }
                },
                new()
                {
                    name = DealDamageEvent.HIT_ATTACKER_CHECK_HIT,
                    damageAction = d => { d.isHit = _dice.RollSum(_d20) != 1; }
                },
                new()
                {
                    name = DealDamageEvent.CAL_ATTACKER_CHECK_CRIT,
                    damageAction = d => { d.isCrit = _dice.RollSum(_d20) == 20; }
                },
                new()
                {
                    name = DealDamageEvent.CAL_DEFENDER_CHECK_DEFENCE,
                    damageAction = d =>
                    {
                        foreach (var dp in d.damage)
                        {
                            dp.deductionRatio = d.defender.attri[Attribute.Defence].Value;
                        }
                    }
                },
                new()
                {
                    name = DealDamageEvent.FINAL_DEFENDER_EVALUATION,
                    damageAction = d =>
                    {
                        var deRes = d.defender.resource;
                        foreach (var dp in d.damage)
                        {
                            var v = (dp.value * (1f + dp.mulValue) + dp.addValue) * (1f - dp.deductionRatio) - dp.deductionValue;
                            deRes.ChangeValue(Attribute.Health, -v);
                        }
                    }
                },
                new()
                {
                    name = DealDamageEvent.HIT_ATTACKER_BEFORE_HIT,
                    damageAction = d =>
                    {
                        if (d.defender.resource[Attribute.Health] <= 0)
                            d.defender.tag.Add(Tag.Dead);
                    }
                }
            }
        };
    }
}