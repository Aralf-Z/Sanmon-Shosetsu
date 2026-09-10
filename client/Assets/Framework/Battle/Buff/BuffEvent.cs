using System.Collections.Generic;

namespace Sanmon.Battle
{
    internal static class BuffEvent
    {
        public const string TIMER_TICK = "buff_timer_tick";
        public const string STACK_TICK = "buff_stack_tick";
        public const string ATTACKER_AFTER_HIT = "buff_attacker_after_hit";
        public const string DEFENDER_AFTER_HIT = "buff_defender_after_hit";

        internal static readonly Dictionary<string, EventType> BUFF_EVENT = new()
        {
            [TIMER_TICK] = EventType.Buff,
            [STACK_TICK] = EventType.Buff,
            [ATTACKER_AFTER_HIT] = EventType.BuffDamageInfo,
            [DEFENDER_AFTER_HIT] = EventType.BuffDamageInfo,
        };
    }
}