using System.Collections.Generic;

namespace Sanmon.Utility.ObjectPool
{
    public class ObjectPool<T> : IObjectPool<T> where T : class, IObject<T>, new()
    {
        private readonly Queue<T> mPool = new ();
        private readonly HashSet<T> mCached = new ();
        
        public T Require()
        {
            if (mPool.Count <= 0)
            {
                var newObj = new T();
                newObj.OnNew();
                mPool.Enqueue(newObj);
            }
            var obj = mPool.Dequeue();
            obj.IsCollected = false;
            obj.OnRequire();
            mCached.Add(obj);
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
            mPool.Enqueue(obj);
            mCached.Remove(obj);
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
            foreach (var obj in mCached)
            {
                obj.IsCollected = true;
                obj.OnRecycle();
                mPool.Enqueue(obj);
            }
            mCached.Clear();
        }
        
        /// <summary>
        /// 清空池
        /// </summary>
        public void Clear()
        {
            Recycle();
            mPool.Clear();
        }
    }
}