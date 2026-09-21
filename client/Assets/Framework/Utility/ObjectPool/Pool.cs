using System.Collections.Generic;

namespace Sanmon.Utility.ObjectPool
{
    public class Pool<T> : IPool<T> where T : class, IPooled, new()
    {
        private readonly Queue<T> _pool = new ();
        private readonly HashSet<T> _cached = new ();
        
        public T Require()
        {
            if (_pool.Count <= 0)
            {
                var newObj = new T();
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
}