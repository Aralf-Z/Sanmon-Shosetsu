using System;
using System.Collections.Generic;
using System.IO;
using ZLua;

namespace Sanmon.Battle
{
    public class Effect
    {
        public string name;
        
        public Script script;

        public bool IsOverride { get; internal set; }

        public EffectEvent[] events;

        public EffectEvent GetEvent(string eventName)
        {
            //一个Effect有且有1个对应的event
            foreach (var e in events)
                if (e.name == eventName) return e;
            throw new EffectException($"effect '{name}' 未包含 '{eventName}' 事件回调");
        }
    }

    public enum EventType
    {
        DamageInfo,
        HealInfo,
        Buff,
        BuffDamageInfo,
        BuffHealInfo,
        DamageInfoUnit,
    }

    public enum Script
    {
        None,
        CSharp,
        Lua,
    }
    
    
}