using System;
using System.Collections.Generic;
using System.IO;
using Sanmon.Core;
using Sanmon.Helper;
using Sanmon.Utility.Singleton;
using ZLua;

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
            DealDamageEvent.CAL_ATTACKER_CHECK_RATIO,
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
            var events = new List<EffectEvent>();
            
            //todo zlua找不到function会报错
            foreach (var luaModule in this.Module().Lua.GetLuaFileName(EFFECT_PATH))
            {
                var effect = new Effect
                {
                    name = luaModule
                };
                
                var lua = Path.Combine(EFFECT_PATH, luaModule);
                
                foreach (var method in LUA_EVENT_NAME)
                {
                    var @event = LuaAppDomain.GetFunction<Action<DamageInfo>>(lua, method);
                    if (@event != null)
                    {
                        events.Add(new EffectEvent()
                        {
                            order = LuaAppDomain.GetFunction<Func<int>>(lua, $"{method}_order")?.Invoke() 
                                    ?? (luaModule.StartsWith("default") ? 0 : 1),
                            name = method,
                            effect = effect,
                            action = @event
                        });
                    }
                }
                
                effect.events = events.ToArray();
                effects.Add(luaModule, effect);
                
                events.Clear();
            }
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
    }
}