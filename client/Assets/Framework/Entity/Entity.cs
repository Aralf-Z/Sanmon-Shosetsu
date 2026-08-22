using System;
using System.Collections.Generic;
using Sanmon.Utility.Set;

namespace Sanmon.GameEntity
{
    public class Entity
    {
        internal readonly Dictionary<Type, ComponentBase> mComponents = new();
        internal readonly Dictionary<Type, FunctionBase> mFunctions = new();
        
        public IReadOnlyCollection<ComponentBase> Components => mComponents.Values;
        public IReadOnlyCollection<FunctionBase> Functions => mFunctions.Values;
        
        protected internal void LogicUpdate(float dt)
        {
            foreach (var (_, func) in mFunctions)
            {
                func.OnLogicUpdate(dt);
            }
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
            return mComponents.GetValueOrDefault(typeof(T)) as T;
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

            if (!mComponents.Remove(key, out var component))
            {
                component.OnRemoved();
            }
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
        /// 添加功能
        /// </summary>
        public T AddFunction<T>() where T : FunctionBase, new()
        {
            var key = typeof(T);

            if (mFunctions.TryGetValue(key, out var add))
            {
                return (T)add;
            }
            
            var func = new T { Host = this };
            mFunctions.Add(typeof(T), func);
            func.OnAdded();
            return func;
        }

        /// <summary>
        /// 移出功能
        /// </summary>
        public T GetFunction<T>() where T : FunctionBase
        {
            return mFunctions.GetValueOrDefault(typeof(T)) as T;
        }

        /// <summary>
        /// 获取功能，如果获取不到则添加一个功能
        /// </summary>
        public T GetOrAddFunction<T>() where T : FunctionBase, new()
        {
            if (mFunctions.TryGetValue(typeof(T), out var func))
            {
                return (T)func;
            }

            return AddFunction<T>();
        }

        /// <summary>
        /// 移出功能
        /// </summary>
        public void RemoveFunction<T>() where T : FunctionBase
        {
            var key = typeof(T);

            if (mFunctions.Remove(key, out var func))
            {
                func.OnRemoved();
            }
        }
        
        public void Clear()
        {
            foreach (var component in mComponents.Values)
            { component.OnRemoved(); }
            
            foreach (var func in mFunctions.Values)
            { func.OnRemoved(); }
            
            mComponents.Clear();
            mFunctions.Clear();
        }
    }
}