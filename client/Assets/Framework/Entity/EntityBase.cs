using System;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;

namespace Sanmon.Entity
{
    public abstract class EntityBase
    {
        internal readonly Dictionary<Type, ComponentBase> mComponents = new();
        
        public IReadOnlyCollection<ComponentBase> Components => mComponents.Values;
        
        protected internal abstract string Tag { get; set; }

        protected internal virtual void PreUpdate(float dt)
        {
            
        }
        
        /// <summary>
        /// 添加组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Add<T>() where T: ComponentBase, new()
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
        public T Get<T>() where T : ComponentBase
        {
            return (T)mComponents.GetValueOrDefault(typeof(T));
        }

        /// <summary>
        /// 获得组件，获取不到则添加一个组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetOrAdd<T>() where T : ComponentBase, new()
        {
            if (mComponents.TryGetValue(typeof(T), out var tar))
            {
                return (T)tar;
            }
           
            return Add<T>();
        }
        
        /// <summary>
        /// 移出组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Remove<T>() where T : ComponentBase
        {
            var key = typeof(T);
            var component = mComponents[key];
            mComponents.Remove(key);
            component.OnRemoved();
        }

        public void Clear()
        {
            foreach (var component in mComponents.Values)
            {
                component.OnRemoved();
            }
            mComponents.Clear();
        }
        
        /// <summary>
        /// 是否拥有组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Has<T>()
        {
            return mComponents.ContainsKey(typeof(T));
        }
    }
}