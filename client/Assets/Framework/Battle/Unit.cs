using Sanmon.GameEntity;

namespace Sanmon.Battle
{
    /// <summary>
    /// 战斗单位
    /// </summary>
    public class Unit
    {
        public Entity entity;
        public CmAttribute attri;
        public CmResource resource;
        public CmBlackboard blackboard;
        public CmBuff buff;
        public CmTag tag;
        public CmGroup group;
        public CmEffect effect;
        public CmTransform transform;
        public CmCollider collider;

        internal Unit(Entity entity)
        {
            this.entity = entity;
            attri = entity.GetComponent<CmAttribute>();
            resource = entity.GetComponent<CmResource>();
            blackboard = entity.GetComponent<CmBlackboard>();
            buff = entity.GetComponent<CmBuff>();
            tag = entity.GetComponent<CmTag>();
            group = entity.GetComponent<CmGroup>();
            effect = entity.GetComponent<CmEffect>();
            transform = entity.GetComponent<CmTransform>();
            collider = entity.GetComponent<CmCollider>();
        }

        internal Unit(Entity entity, CmAttribute attri, CmResource resource, CmBlackboard blackboard, CmBuff buff, CmTag tag, CmGroup group, CmEffect effect, CmTransform transform, CmCollider collider)
        {
            this.entity = entity;
            this.attri = attri;
            this.resource = resource;
            this.blackboard = blackboard;
            this.buff = buff;
            this.tag = tag;
            this.group = group;
            this.effect = effect;
            this.transform = transform;
            this.collider = collider;
        }
    }
}