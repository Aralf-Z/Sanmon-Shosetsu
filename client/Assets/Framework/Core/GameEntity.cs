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
        private readonly HashSet<Entity> _entities = new HashSet<Entity>();
        private readonly List<Entity> _removeCache = new List<Entity>();
        private readonly List<Entity> _addCache = new List<Entity>();
        
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
            _addCache.Add(entity);
        }
        
        public void Recycle(Entity entity)
        {
            entity.Clear();
            _removeCache.Add(entity);
        }
        
        public void Recycle(IEnumerable<Entity> entity)
        {
            foreach (var e in entity)
            {
                _removeCache.Add(e);
            }
        }

        internal void OnLogicUpdate(float dt)
        {
            foreach (var e in _entities)
                e.LogicUpdate(dt);
            
            foreach (var e in _addCache)
                _entities.Add(e);
            
            foreach (var e in _removeCache)
                _entities.Remove(e);
            
            _addCache.Clear();
            _removeCache.Clear();
        }
    }
}