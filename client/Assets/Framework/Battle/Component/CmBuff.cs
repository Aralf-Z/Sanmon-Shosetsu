using System.Collections.Generic;
using Sanmon.GameEntity;
using Sanmon.Utility.Set;

namespace Sanmon.Battle
{
    public class CmBuff: ComponentBase
    {
        public DualBufferSet<Buff> buffs = new DualBufferSet<Buff>();
        
        public Dictionary<string, List<Buff>> buffOnEvent = new Dictionary<string, List<Buff>>();
    }
}