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
        public CmEffect effect;
        public CmTransform transform;

        public Unit(Entity unit)
        {
            this.unit = unit;
            attri = unit.GetComponent<CmAttribute>();
            resource = unit.GetComponent<CmResource>();
            blackboard = unit.GetComponent<CmBlackboard>();
            tag = unit.GetComponent<CmTag>();
            group = unit.GetComponent<CmGroup>();
            effect = unit.GetComponent<CmEffect>();
            transform = unit.GetComponent<CmTransform>();
        }
    }
}