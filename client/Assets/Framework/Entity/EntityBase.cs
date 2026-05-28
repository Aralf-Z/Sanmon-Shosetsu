using System;
using System.Collections.Generic;
using Sanmon.Utility.Set;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Sanmon.Entity
{
    public abstract class EntityBase
    {
        internal readonly Dictionary<Type, ComponentBase> mComponents = new();
        internal readonly DualBufferSet<EffectBase> mEffects = new();
        
        public IReadOnlyCollection<ComponentBase> Components => mComponents.Values;
        
        protected internal abstract string Tag { get; set; }

        protected internal virtual void Update(float dt)
        {
            
        }
        
        protected internal virtual void LateUpdate(float dt)
        {
            
        }
        
        protected internal virtual void FixedUpdate(float dt)
        {
            mEffects.Update(dt);
        }
        
        /// <summary>
        /// 添加组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddComponent<T>() where T: ComponentBase, new()
        {
            var key = typeof(T);
            
            if (mComponents.TryGetValue(key, out var add))
            {
                return (T)add;
            }
            
            var component = new T { Host = this };
            mComponents.Add(key, component);
            component.OnAdded();
            
            return component;
        }
        
        /// <summary>
        /// 获得组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponent<T>() where T : ComponentBase
        {
            return (T)mComponents.GetValueOrDefault(typeof(T));
        }

        /// <summary>
        /// 获得组件，获取不到则添加一个组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetOrAddComponent<T>() where T : ComponentBase, new()
        {
            if (mComponents.TryGetValue(typeof(T), out var tar))
            {
                return (T)tar;
            }
           
            return AddComponent<T>();
        }
        
        /// <summary>
        /// 移出组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RemoveComponent<T>() where T : ComponentBase
        {
            var key = typeof(T);
            var component = mComponents[key];
            mComponents.Remove(key);
            component.OnRemoved();
        }

        /// <summary>
        /// 是否拥有组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool HasComponent<T>()
        {
            return mComponents.ContainsKey(typeof(T));
        }
        
        /// <summary>
        /// 添加效果
        /// </summary>
        /// <param name="effect"></param>
        public void AddEffect(EffectBase effect) => mEffects.Add(effect);
        
        /// <summary>
        /// 一处效果
        /// </summary>
        /// <param name="effect"></param>
        public void RemoveEffect(EffectBase effect) => mEffects.Remove(effect);
        
        public void Clear()
        {
            foreach (var component in mComponents.Values)
            {
                component.OnRemoved();
            }
            mComponents.Clear();
            mEffects.Clear();
        }
    }
}