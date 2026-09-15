using System.Collections.Generic;
using Game.Config.Battle;
using UnityEngine;
using Logger = Sanmon.Helper.Logger;

namespace Sanmon.Battle
{
    internal static partial class EffectBuiltin
    {
        public static List<Effect> effects = new ()
        {
            new Effect()
            {
                name = "default_damage_pipeline",
                events = new EffectEvent[]
                {
                    new EffectEvent()
                    {
                        name = DealDamageEvent.HIT_ATTACKER_BEFORE_HIT,
                        damageAction = d =>
                        {
                            if(d.defender.tag.Check(Tag.Dead))
                                d.isHit = true;
                        }
                    },
                    new EffectEvent()
                    {
                        name = DealDamageEvent.HIT_ATTACKER_CHECK_HIT,
                        damageAction = d =>
                        {
                            d.isHit = Random.Range(1, 21) != 1;
                        }
                    }
                    ,
                    new EffectEvent()
                    {
                        name = DealDamageEvent.CAL_ATTACKER_CHECK_CRIT,
                        damageAction = d =>
                        {
                            d.isCrit = Random.Range(1, 21) == 20;
                        }
                    }
                    ,
                    new EffectEvent()
                    {
                        name = DealDamageEvent.CAL_ATTACKER_CHECK_EXTRA_DAMAGE,
                        damageAction = d =>
                        {
                            
                        }
                    }
                    ,
                    new EffectEvent()
                    {
                        name = DealDamageEvent.CAL_DEFENDER_CHECK_DEFENCE,
                        damageAction = d =>
                        {
                            foreach (var dp in d.damage)
                            {
                               Logger.LogDebug(dp.ToString());
                            }
                        }
                    }
                    ,
                    new EffectEvent()
                    {
                        name = DealDamageEvent.FINAL_DEFENDER_EVALUATION,
                        damageAction = d =>
                        {
                            var deRes = d.defender.resource;
                            foreach (var dp in d.damage)
                            {
                                var v = (dp.value * dp.mulValue + dp.addValue) * (1 - dp.deductionRatio) - dp.deductionValue;
                                deRes.ChangeValue(Attribute.Health, -v);
                            }
                        }
                    }
                    ,
                    new EffectEvent()
                    {
                        name = DealDamageEvent.HIT_ATTACKER_BEFORE_HIT,
                        damageAction = d =>
                        {
                            if(d.defender.resource[Attribute.Health] <= 0)
                                d.defender.tag.Add(Tag.Dead);
                        }
                    }
                }
            }
        };
    }
}