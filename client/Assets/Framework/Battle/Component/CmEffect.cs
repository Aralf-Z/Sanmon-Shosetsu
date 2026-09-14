using System.Collections.Generic;
using Sanmon.Core;
using Sanmon.GameEntity;
using Sanmon.Helper;

namespace Sanmon.Battle
{
    public class CmEffect : ComponentBase
        , IGetNote
    {
        private static readonly List<EffectEvent> DEFAULT_EVENTS = new();

        private Dictionary<string, List<EffectEvent>> _events = new();
        private Dictionary<string, EffectInfo> _effects = new();

        protected internal override void OnAdded()
        {
            base.OnAdded();

            //todo default_damage_pipeline
            Add("default_damage_pipeline");
        }

        internal IReadOnlyList<EffectEvent> FindEvent(string eventName)
        {
            var r = _events.GetValueOrDefault(eventName, DEFAULT_EVENTS);
            return r;
        }

        public void Add(string effect)
        {
            if (_effects.TryGetValue(effect, out var info))
            {
                info.count++;
            }
            else
            {
                var be = EffectManager.Ins.Require(effect);
                info = new EffectInfo() { effect = be, count = 1 };
                _effects[effect] = info;

                foreach (var @event in be.events)
                {
                    var events = _events.GetOrAdd(@event.name);

                    //二分插入
                    var left = 0;
                    var right = events.Count;

                    while (left < right)
                    {
                        var mid = (left + right) >> 1;
                        if (events[mid].order <= @event.order) left = mid + 1;
                        else right = mid;
                    }

                    events.Insert(left, @event);
                }
            }
            
            foreach (var @event in info.effect.events)
            {
                var unit = this.Note().Get<BattleNote>().allUnits[Owner];
                @event.unitHolder.Add(unit);
            }
        }

        public void Remove(string effect)
        {
            if (_effects.TryGetValue(effect, out var info))
            {
                info.count--;
                
                foreach (var @event in info.effect.events)
                {
                    var unit = this.Note().Get<BattleNote>().allUnits[Owner];
                    @event.unitHolder.Remove(unit);
                }
                
                if (info.count == 0)
                {
                    _effects.Remove(effect);
                    foreach (var @event in info.effect.events)
                    {
                        var events = _events[@event.name];
                        events.Remove(@event);
                    }
                }
            }
            else
            {
                throw new EffectException($"entity'{Owner.Info}]' 未包含effect'{effect}'");
            }
        }

        private class EffectInfo
        {
            public Effect effect;
            public int count;
        }
    }
}