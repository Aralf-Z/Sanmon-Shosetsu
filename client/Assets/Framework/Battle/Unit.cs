using Sanmon.GameEntity;

namespace Sanmon.Battle
{
    /// <summary>
    /// 战斗单位
    /// </summary>
    public class Unit
    {
        public Entity unit;
        public CmAttribute attri;
        public CmResource resource;
        public CmBlackboard blackboard;
        public CmTag tag;
        public CmGroup group;

        public Unit(Entity unit)
        {
            this.unit = unit;
            attri = unit.GetComponent<CmAttribute>();
            resource = unit.GetComponent<CmResource>();
            blackboard = unit.GetComponent<CmBlackboard>();
            tag = unit.GetComponent<CmTag>();
            group = unit.GetComponent<CmGroup>();
        }
    }
}