using System.Collections.Generic;
using System.Linq;
using Game.Config.Battle;

namespace Sanmon.Battle
{
    //todo 池化？
    public class DamageInfo
    {
        public IDamageMaker maker;
        public Unit attacker;
        public Unit defender;
        public ColliderBox box;
        public DamageSource source;
        public List<DamagePair> damage;
        
        public bool isCrit;
        public bool isHit;
        
        public List<Buff> buffsOnHitForDefender;
        public List<Buff> buffsOnHitForAttacker;

        public override string ToString()
        {
            return $"maker: {maker.Name} | attacker: [{attacker.unit.Info}] | [defender: {defender.unit.Info}]"
                + $"\nsource = {source} | isCrit = {isCrit} | isHit = {isHit}"
                + $"\ndamage = {string.Join(", ", damage)}"
                + $"\nbuffsOnHitForDefender = {string.Join("\n", buffsOnHitForDefender.Select(x => x.data))}"
                + $"\nbuffsOnHitForAttacker = {string.Join("\n", buffsOnHitForAttacker.Select(x => x.data))}";
        }
    }
}