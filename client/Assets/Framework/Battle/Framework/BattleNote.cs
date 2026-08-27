using System.Collections.Generic;
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
    }
}