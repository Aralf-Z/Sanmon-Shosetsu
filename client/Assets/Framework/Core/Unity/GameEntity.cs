using System.Collections.Generic;
using Sanmon.Entity;
using UnityEngine;

namespace Sanmon.Core
{
    /// <summary>
    /// 框架实体管理器，所有的实体都必须注册在这里
    /// </summary>
    public class GameEntity : MonoBehaviour,
        IGetModule
    {
        private readonly HashSet<EntityBase> mEntities = new HashSet<EntityBase>();
        private readonly List<EntityBase> mRemoveCache = new List<EntityBase>();
        
        internal void Init()
        {

        }

        internal void Destroy()
        {
            
        }
        
        //todo 场景中被标记为Entity时可能需要一个注册接口
        
        public T Require<T>() where T : EntityBase, new ()
        {
            var en = new T();
            mEntities.Add(en);
            return en;
        }

        public void Recycle<T>(T entity) where T : EntityBase
        {
            entity.Clear();
            mRemoveCache.Add(entity);
        }
        
        public void Recycle<T>(IEnumerable<T> entity) where T : EntityBase
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
            
        }

        internal void OnFixedUpdate(float dt)
        {
            
        }
    }
}