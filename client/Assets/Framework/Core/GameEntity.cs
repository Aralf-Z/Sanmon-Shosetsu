using System.Collections.Generic;
using Sanmon.Entities;
using UnityEngine;

namespace Sanmon.Core
{
    /// <summary>
    /// 框架实体管理器，所有的实体都必须注册在这里
    /// </summary>
    public class GameEntity : MonoBehaviour,
        IGetModule
    {
        private readonly HashSet<Entity> mEntities = new HashSet<Entity>();
        private readonly List<Entity> mRemoveCache = new List<Entity>();
        private readonly List<Entity> mAddCache = new List<Entity>();
        
        internal void Init()
        {

        }

        internal void Destroy()
        {
            
        }
        
        public Entity Require()
        {
            var en = new Entity();
            Register(en);
            return en;
        }

        public void Register(Entity entity)
        {
            mAddCache.Add(entity);
        }
        
        public void Recycle(Entity entity)
        {
            entity.Clear();
            mRemoveCache.Add(entity);
        }
        
        public void Recycle(IEnumerable<Entity> entity)
        {
            foreach (var e in entity)
            {
                mRemoveCache.Add(e);
            }
        }

        internal void OnLogicUpdate(float dt)
        {
            foreach (var e in mEntities)
                e.LogicUpdate(dt);
            
            foreach (var e in mAddCache)
                mEntities.Add(e);
            
            foreach (var e in mRemoveCache)
                mEntities.Remove(e);
            
            mAddCache.Clear();
            mRemoveCache.Clear();
        }
    }
}