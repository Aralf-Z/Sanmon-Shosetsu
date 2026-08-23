using System.Collections.Generic;

namespace Framework.Battle
{
    public class DamageInfo
    {
        public BattleUnit attacker;
        public BattleUnit defender;
        public BattleBox box;
        public DamageSource source;
        public List<DamagePair> damage;

        public bool isCrit;
        public bool isHit;
        
        public List<Buff> buffsOnHitForDefender;
        public List<Buff> buffsOnHitForAttacker;
    }
}