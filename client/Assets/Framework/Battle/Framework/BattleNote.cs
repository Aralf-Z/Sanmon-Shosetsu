using System.Collections.Generic;
using Sanmon.GameEntity;
using Sanmon.Note;

namespace Sanmon.Battle
{
    public class BattleNote: NoteBase
    {
        protected internal override void Init()
        {
            
        }
        
        public readonly Queue<DamageInfo> damageInfos = new Queue<DamageInfo>();
        public readonly Queue<HealInfo> healInfos = new Queue<HealInfo>();
        
        public readonly Dictionary<Entity, Unit> allUnits = new ();
        public IReadOnlyCollection<Unit> AllUnits => allUnits.Values;
    }
}