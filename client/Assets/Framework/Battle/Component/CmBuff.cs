using System.Collections.Generic;
using Sanmon.GameEntity;
using Sanmon.Utility.Set;

namespace Sanmon.Battle
{
    public class CmBuff: ComponentBase
    {
        private readonly BuffSet _buffs = new();
        private readonly Dictionary<string, List<Buff>> _buffOnEvent = new ();

        public IReadOnlyDictionary<string, List<Buff>> BuffOnEvent => _buffOnEvent;
        public IReadOnlyCollection<Buff> AllBuff => _buffs.AllItem;
        
        internal DualBufferSet<Buff> Buffs => _buffs;

        protected internal override void OnAdded()
        {
            _buffs.cm = this;
        }

        public void Add(Buff buff)
        {
            _buffs.Add(buff);
        }

        private class BuffSet : DualBufferSet<Buff>
        {
            public CmBuff cm;

            public override void Add(Buff item)
            {
                base.Add(item);

                foreach (var effName in item.data.Effects)
                {
                    foreach (var ee in EffectManager.Ins.events[effName])
                    {
                        if(!cm._buffOnEvent.TryGetValue(ee.name, out var list))
                            list = new List<Buff>();
                        list.Add(item);
                    }
                }
            }

            protected override void Remove(Buff item)
            {
                base.Remove(item);
                
                foreach (var effName in item.data.Effects)
                {
                    foreach (var ee in EffectManager.Ins.events[effName])
                    {
                        if(cm._buffOnEvent.TryGetValue(ee.name, out var list))
                            list.Remove(item);
                    }
                }
            }
        }
    }
    
    
}