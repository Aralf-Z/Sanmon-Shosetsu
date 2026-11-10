using System;
using System.Collections.Generic;
using System.IO;
using Sanmon.Helper;
using ZLua;

namespace Sanmon.Battle
{
    public class EffectEvent
    {
        public int order;

        public string name;

        public Effect effect;

        public EventType eventType;

        public readonly HashSet<Unit> unitHolder = new ();

        public readonly HashSet<Buff> buffHolder = new ();
        
        public Action<DamageInfo> damageAction;
        
        public Action<HealInfo> healAction;

        public Action<Buff> buffAction;
        
        public Action<Buff, DamageInfo> buffDamageAction;
        
        public Action<Buff, HealInfo> buffHealAction;
        
        public Action<DamageInfo, Unit> globalDamageAction;
        
        public void LoadLua()
        {
            var luaModule = Path.Combine(EffectManager.EFFECT_PATH, effect.name);
            try
            {
                switch (eventType)
                {
                    case EventType.DamageInfo:
                        damageAction = LuaAppDomain.GetFunction<Action<DamageInfo>>(luaModule, name);
                        break;
                    case EventType.HealInfo:
                        healAction = LuaAppDomain.GetFunction<Action<HealInfo>>(luaModule, name);
                        break;
                    case EventType.Buff:
                        buffAction = LuaAppDomain.GetFunction<Action<Buff>>(luaModule, name);
                        break;
                    case EventType.BuffDamageInfo:
                        buffDamageAction = LuaAppDomain.GetFunction<Action<Buff, DamageInfo>>(luaModule, name);
                        break;
                    case EventType.BuffHealInfo:
                        buffHealAction = LuaAppDomain.GetFunction<Action<Buff, HealInfo>>(luaModule, name);
                        break;
                    case EventType.DamageInfoUnit:
                        globalDamageAction = LuaAppDomain.GetFunction<Action<DamageInfo, Unit>>(luaModule, name);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception e)
            {
                SanmonLogger.LogError("Error load lua file: " + e.Message + $". luaModule = {luaModule}, name = {name}.");
                throw;
            }
            
        }
    }
}