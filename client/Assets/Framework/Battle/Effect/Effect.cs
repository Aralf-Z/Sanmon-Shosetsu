using System;
using System.IO;
using ZLua;

namespace Sanmon.Battle
{
    internal class Effect
    {
        public string name;
        
        public EffectEvent[] events;

        public EffectEvent GetEvent(string eventName)
        {
            foreach (var e in events)
                if (e.name == eventName) return e;
            throw new EffectException($"effect '{name}' 未包含 '{eventName}' 事件回调");
        }
    }

    internal enum EventType
    {
        DamageInfo,
        HealInfo,
        Buff,
        BuffDamageInfo,
        BuffHealInfo,
    }

    internal class EffectEvent
    {
        public int order;

        public string name;

        public Effect effect;

        public EventType eventType;
        
        public Action<DamageInfo> damageAction;
        
        public Action<HealInfo> healAction;

        public Action<Buff> buffAction;
        
        public Action<Buff, DamageInfo> buffDamageAction;
        
        public Action<Buff, HealInfo> buffHealAction;
        
        public void LoadLua()
        {
            var luaModule = Path.Combine(EffectManager.EFFECT_PATH, effect.name);
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
                default:
                    throw new NotImplementedException();
            }
        }
    }
}