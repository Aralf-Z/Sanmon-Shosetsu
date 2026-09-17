using System;
using Game.Config.Battle;
using Sanmon.Battle;
using Sanmon.Helper;
using Attribute = Game.Config.Battle.Attribute;

namespace GameScripts
{
    public class DealDamageEffect
    {
        public const string DAMAGE_PIPELINE = "default_damage_pipeline";

        private Random _random = new Random();

        public static readonly Effect effect = new Effect()
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
                        
                        Logger.LogDebug($"{d}", "测试");
                    }
                },
                new()
                {
                    name = DealDamageEvent.HIT_ATTACKER_CHECK_HIT,
                },
                new()
                {
                    name = DealDamageEvent.CAL_ATTACKER_CHECK_CRIT,
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