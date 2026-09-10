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
        public bool isAbort;

        public override string ToString()
        {
            return $"maker: {maker.Name} | attacker: [{attacker.entity.Info}] | [defender: {defender.entity.Info}]"
                   + $"\nsource = {source} | isCrit = {isCrit} | isHit = {isHit} | isAbort {isAbort}"
                   + $"\ndamage = {string.Join(", ", damage)}";
        }
    }
}