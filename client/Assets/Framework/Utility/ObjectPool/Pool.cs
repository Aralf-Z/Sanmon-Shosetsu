using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sanmon.Utility.ObjectPool
{
    public class Pool<T> : IPool<T> where T : class, IPooled
    {
        protected Func<T> _create;
        
        private readonly Queue<T> _pool = new ();
        private readonly HashSet<T> _cached = new ();
        
        protected Pool()
        {
        }

        public Pool(Func<T> create)
        {
            _create = create ?? throw new ArgumentNullException(nameof(create));
        }

        /// <summary>
        /// 请求对象
        /// </summary>
        /// <returns></returns>
        public T Require()
        {
            if (_pool.Count <= 0)
            {
                var newObj = _create.Invoke();
                newObj.OnNew();
                _pool.Enqueue(newObj);
            }
            var obj = _pool.Dequeue();
            obj.IsCollected = false;
            obj.OnRequire();
            _cached.Add(obj);
            return obj;
        }

        /// <summary>
        /// 回收单个
        /// </summary>
        /// <param name="obj"></param>
        public void Recycle(T obj)
        {
            if (obj.IsCollected) return;
            obj.IsCollected = true;
            obj.OnRecycle();
            _pool.Enqueue(obj);
            _cached.Remove(obj);
        }

        /// <summary>
        /// 回收复数个
        /// </summary>
        /// <param name="objs"></param>
        public void Recycle(IEnumerable<T> objs)
        {
            foreach (var obj in objs) Recycle(obj);
        }

        /// <summary>
        /// 回收所有
        /// </summary>
        public void Recycle()
        {
            foreach (var obj in _cached)
            {
                obj.IsCollected = true;
                obj.OnRecycle();
                _pool.Enqueue(obj);
            }
            _cached.Clear();
        }
        
        /// <summary>
        /// 清空池
        /// </summary>
        public void Clear()
        {
            Recycle();
            _pool.Clear();
        }
    }

    public class SimplePool<T> : Pool<T> where T : class, IPooled, new()
    {
        public SimplePool()
        {
            _create = () => new T();
        }
    }

    public class MonoPool<T> : Pool<T> where T : MonoBehaviour, IPooled
    {
        private readonly GameObject _template;
        private readonly Transform _parent;
        
        public MonoPool(GameObject template, Transform parent = null)
        {
            _template = template;
            _parent = parent ?? template.transform.parent;

            _template.SetActive(false);
            
            _create = Create;
        }

        private T Create()
        {
            var go = Object.Instantiate(_template, _parent);
            go.SetActive(true);
            return go.GetComponent<T>();
        }
    }
}