using System;

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

    internal class EffectEvent
    {
        public int order;

        public string name;

        public Effect effect;

        public Action<DamageInfo> action;
    }
}