using Sanmon.GameEntity;

namespace Framework.Battle
{
    /// <summary>
    /// 战斗单位
    /// </summary>
    public class BattleUnit
    {
        public Entity unit;
        public CmAttribute attri;
        public CmResource resource;
        public CmTag tag;
        public CmGroup group;

        public BattleUnit(Entity unit)
        {
            this.unit = unit;
            attri = unit.GetComponent<CmAttribute>();
            resource = unit.GetComponent<CmResource>();
            tag = unit.GetComponent<CmTag>();
            group = unit.GetComponent<CmGroup>();
        }
    }
}