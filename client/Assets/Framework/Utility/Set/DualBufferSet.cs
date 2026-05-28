using System.Collections.Generic;
using System.Linq;
using Sanmon.Helper;

namespace Sanmon.Utility.Set
{
    /*
     * 优化方案: 减少Dictionary和HashSet，
     * 1.将Item缓存在一个类的List集合上，类持有order，对order排序
     * 2.将item的位序也缓存下来，通过移出为序的方式来移出，在remove的时候利用字典获取order封装类
     * 3.以上通过将遍历hash改为遍历List，将hash压力转嫁给remove
     * 4.加入新的不要用排序，要用0(1)的插入？或者分长度实现
     */
    
    
    /// <summary>
    /// 双缓冲容器，可以排序遍历
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DualBufferSet<T> where T : IBufferItem
    {
        private readonly Dictionary<int, HashSet<T>> mMainSet = new ();
        private readonly List<int> mOrder = new ();
        private readonly List<T> mAddCache = new ();
        private readonly List<T> mRemoveCache = new ();
        
        public void Add(T item)
        {
            item.SetStatus(EmBufferStatus.PendingAdd);
            mAddCache.Add(item);
            item.OnAdd();
        }
        
        public void Remove(T item)
        {
            item.SetStatus(EmBufferStatus.PendingRemove);
            mRemoveCache.Add(item);
            item.OnRemove();
        }

        public void Clear()
        {
            mMainSet.Clear();
            mOrder.Clear();
            mAddCache.Clear();
            mRemoveCache.Clear();
        }
        
        public void Update(float dt)
        {
            var needOrder = false;
            
            foreach (var order in mOrder)
            {
                var items = mMainSet[order];

                foreach (var item in items)
                {
                    if(item.Status is EmBufferStatus.Running)
                        item.OnUpdate(dt);
                
                    if(item.Status is EmBufferStatus.Dirty)
                        Remove(item);
                }
            }

            foreach (var item in mAddCache)
            {
                if (mMainSet.TryGetValue(item.Order, out var set))
                    set.Add(item);
                else
                {
                    item.SetStatus(EmBufferStatus.Running);
                    mMainSet.Add(item.Order, new HashSet<T> { item });
                    mOrder.Add(item.Order);
                    needOrder =  true;
                }
            }

            foreach (var item in mRemoveCache)
            {
                mMainSet[item.Order].Remove(item);
            }
            
            if(needOrder) mOrder.Sort();
            
            mAddCache.Clear();
            mRemoveCache.Clear();
        }
    }
}