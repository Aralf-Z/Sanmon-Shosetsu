using System;
using System.Collections.Generic;
using Sanmon.Utility.Set;

namespace Sanmon.GameEntity
{
    public class Entity
    {
        internal readonly Dictionary<Type, ComponentBase> _components = new();
        internal readonly Dictionary<Type, FunctionBase> _functions = new();
        internal readonly Dictionary<int, List<Effect>> _effects = new();
        
        public IReadOnlyCollection<ComponentBase> Components => _components.Values;
        public IReadOnlyCollection<FunctionBase> Functions => _functions.Values;

        internal Entity()
        {
        }

        protected internal void LogicUpdate(float dt)
        {
            foreach (var (_, func) in _functions)
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
            
            if (_components.TryGetValue(key, out var add))
            {
                return (T)add;
            }
            
            var component = new T { Host = this };
            _components.Add(key, component);
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
            return _components.GetValueOrDefault(typeof(T)) as T;
        }

        /// <summary>
        /// 获得组件，获取不到则添加一个组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetOrAddComponent<T>() where T : ComponentBase, new()
        {
            if (_components.TryGetValue(typeof(T), out var tar))
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

            if (!_components.Remove(key, out var component))
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
            return _components.ContainsKey(typeof(T));
        }

        /// <summary>
        /// 添加功能
        /// </summary>
        public T AddFunction<T>() where T : FunctionBase, new()
        {
            var key = typeof(T);

            if (_functions.TryGetValue(key, out var add))
            {
                return (T)add;
            }
            
            var func = new T { Host = this };
            _functions.Add(typeof(T), func);
            func.OnAdded();
            return func;
        }

        /// <summary>
        /// 移出功能
        /// </summary>
        public T GetFunction<T>() where T : FunctionBase
        {
            return _functions.GetValueOrDefault(typeof(T)) as T;
        }

        /// <summary>
        /// 获取功能，如果获取不到则添加一个功能
        /// </summary>
        public T GetOrAddFunction<T>() where T : FunctionBase, new()
        {
            if (_functions.TryGetValue(typeof(T), out var func))
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

            if (_functions.Remove(key, out var func))
            {
                func.OnRemoved();
            }
        }

        public void AddEffect(int id)
        {
            if (_effects.Count > 0)
            {
                for (var i = _effects.Count - 1; i > 0; i--)
                {
                    
                }
            }
            else
            {
                //_effects.Add(BattleEffect.Ins.Require(id));
            }
        }

        private void RemoveEffect(int id)
        {
            
        }
        
        public void Clear()
        {
            foreach (var component in _components.Values)
            { component.OnRemoved(); }
            
            foreach (var func in _functions.Values)
            { func.OnRemoved(); }
            
            _components.Clear();
            _functions.Clear();
            _effects.Clear();
        }
    }
}