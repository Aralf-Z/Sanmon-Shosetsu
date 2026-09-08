using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Framework.Module;
using Game.Config.Battle;
using Sanmon.Core;
using Sanmon.Utility.Singleton;
using UnityEngine;
using ZLua;
using Attribute = Game.Config.Battle.Attribute;
using Logger = Sanmon.Helper.Logger;
using Random = UnityEngine.Random;

namespace Sanmon.Battle
{
    internal class EffectManager: Singleton<EffectManager>
        , IGetModule
    { 
        private const string EFFECT_PATH = "effect";
        
        private static readonly string[] LUA_EVENT_NAME =
        {
            // 命中阶段
            DealDamageEvent.HIT_ATTACKER_BEFORE_HIT,
            DealDamageEvent.HIT_DEFENDER_BEFORE_HIT,
            DealDamageEvent.HIT_ATTACKER_CHECK_HIT,
            DealDamageEvent.HIT_ATTACKER_AFTER_HIT,
            DealDamageEvent.HIT_DEFENDER_AFTER_HIT,
            
            // 计算阶段
            DealDamageEvent.CAL_ATTACKER_BEFORE_CAL,
            DealDamageEvent.CAL_DEFENDER_BEFORE_CAL,
            DealDamageEvent.CAL_ATTACKER_CHECK_CRIT,
            DealDamageEvent.CAL_ATTACKER_CHECK_EXTRA_DAMAGE,
            DealDamageEvent.CAL_DEFENDER_CHECK_DEFENCE,
            DealDamageEvent.CAL_ATTACKER_CHECK_DERIVE,
            DealDamageEvent.CAL_DEFENDER_CHECK_DERIVE,
            DealDamageEvent.CAL_ATTACKER_AFTER_CAL,
            DealDamageEvent.CAL_DEFENDER_AFTER_CAL,
            
            // 结算阶段
            DealDamageEvent.FINAL_ATTACKER_BEFORE_FINAL,
            DealDamageEvent.FINAL_DEFENDER_BEFORE_FINAL,
            DealDamageEvent.FINAL_DEFENDER_EVALUATION,
            DealDamageEvent.FINAL_DEFENDER_CHECK_STATE,
            DealDamageEvent.FINAL_ATTACKER_DERIVE,
            DealDamageEvent.FINAL_DEFENDER_DERIVE,
            DealDamageEvent.FINAL_ATTACKER_AFTER_FINAL,
            DealDamageEvent.FINAL_DEFENDER_AFTER_FINAL,
        };
        
        public readonly Dictionary<string, Effect> effects = new ();
        
        public EffectManager()
        {
#if UNITY_EDITOR
            var time = UnityEngine.Time.realtimeSinceStartup;
#endif
            foreach (var luaModule in this.Module().Lua.GetLuaFileName(EFFECT_PATH))
            {
                var effect = NewEffect(luaModule);
                effects.Add(luaModule, effect);
            }

            effects["default_damage_pipeline"] = new Effect
            {
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
                            foreach (var dp in d.damage)
                            {
                                dp.addValue += dp.type is DamageType.Physical ? 5 : 0;
                                dp.mulValue += dp.type is DamageType.Magical ? 1.2f : 0;
                            }
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
                                dp.deductionRatio += dp.type is DamageType.Physical ? .2f : 0;
                                dp.deductionValue += dp.type is DamageType.Magical ? 5f : 0;
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
            };
            
#if UNITY_EDITOR
            Logger.LogInfo($"EffectManager 初始化花销 '{UnityEngine.Time.realtimeSinceStartup - time}s'", "战斗");
#endif
        }

        public Effect Require(string effect)
        {
            if(effects.TryGetValue(effect, out var effectInstance))
                return  effectInstance;
            
            throw new EffectException($"申请错误的Effect名: {effect}");
        }

        
        private Effect NewEffect(string effectName)
        {
            var effect = new Effect
            {
                name = effectName
            };
                
            var luaFullPath = Path.Combine(LuaModule.RootPath, EFFECT_PATH, effectName + ".lua");
            var luaText = File.ReadAllText(luaFullPath, Encoding.UTF8);
            var returnText = ExtractLastBracedContent(luaText);
            var luaModule = Path.Combine(EFFECT_PATH, effectName);
            var events =  new List<EffectEvent>();
            
            foreach (var method in LUA_EVENT_NAME)
            {
                if (returnText.Contains(method))
                {
                    var orderFunc = $"{method}_order";
                    var @event = LuaAppDomain.GetFunction<Action<DamageInfo>>(luaModule, method);
                    
                    if (@event != null)
                    {
                        events.Add(new EffectEvent()
                        {
                            order = returnText.Contains(orderFunc) 
                                ? LuaAppDomain.GetFunction<Func<int>>(luaModule, orderFunc).Invoke()
                                : effectName.StartsWith("default") ? 0 : 1,
                            name = method,
                            effect = effect,
                            damageAction = @event
                        });
                    }
                }
            }
                
            effect.events = events.ToArray();
            
            return effect;
        }
        
        private static string ExtractLastBracedContent(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            var end = text.LastIndexOf('}');
            if (end == -1) return null;
            
            var braceCount = 0;
            var start = -1;
            for (var i = end; i >= 0; i--)
            {
                var c = text[i];
                if (c == '}') braceCount++;
                else if (c == '{')
                {
                    braceCount--;
                    if (braceCount == 0)
                    {
                        start = i;
                        break;
                    }
                }
            }

            if (start == -1) return null;
            
            var inner = text.Substring(start + 1, end - start - 1).Trim();
            if (string.IsNullOrEmpty(inner)) return null;
            
            return inner;
        }
    }
}