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
            mEntities.Add(entity);
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
        
        internal void OnUpdate(float dt)
        {
            foreach (var e in mEntities)
            {
                e.Update(dt);
            }
        }

        internal void OnLateUpdate(float dt)
        {
            foreach (var e in mRemoveCache)
            {
                mEntities.Remove(e);
            }
            
            mRemoveCache.Clear();
        }

        internal void OnFixedUpdate(float dt)
        {
            
        }
    }
}