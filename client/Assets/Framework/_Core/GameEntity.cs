using System;
using System.Collections.Generic;
using Sanmon.GameEntity;
using UnityEngine;

namespace Sanmon.Core
{
    /// <summary>
    /// 框架实体管理器，所有的实体都必须注册在这里
    /// </summary>
    public class GameEntity : MonoBehaviour,
        IGetModule
    {
        internal bool IsInit { get; private set; }
        
        private readonly HashSet<Entity> _entities = new HashSet<Entity>();
        private readonly HashSet<Entity> _pendingAdd = new HashSet<Entity>();
        private readonly HashSet<Entity> _pendingRemove = new HashSet<Entity>();
        
        private int _entityInstanceId;
        
        internal void Init()
        {
            IsInit = true;
        }

        internal void Destroy()
        {
            IsInit = false;
        }
        
        public Entity Require(string entityName)
        {
            var en = new Entity();
            var info = en.AddComponent<CmInfo>();
            info.Name = entityName;
            info.InstanceId = _entityInstanceId++;
            en.Info = info;
            _pendingAdd.Add(en);
            return en;
        }
        
        public void Recycle(Entity entity)
        {
            entity.Clear();
            _pendingRemove.Add(entity);
        }
        
        public void Recycle(IEnumerable<Entity> entity)
        {
            foreach (var e in entity)
            {
                _pendingRemove.Add(e);
            }
        }

        internal void OnLogicUpdate(float dt)
        {
            foreach (var e in _pendingAdd)
                _entities.Add(e);
            
            foreach (var e in _pendingRemove)
                _entities.Remove(e);
            
            foreach (var e in _entities)
                e.LogicUpdate(dt);
            
            _pendingAdd.Clear();
            _pendingRemove.Clear();
        }
    }
}