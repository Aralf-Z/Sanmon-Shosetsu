using System.Collections.Generic;

namespace Sanmon.Battle
{
    internal static class BuffEvent
    {
        public const string TIMER_TICK = "buff_timer_tick";
        public const string STACK_TICK = "buff_stack_tick";

        internal static readonly Dictionary<string, EventType> BUFF_EVENT = new()
        {

        };
    }
}