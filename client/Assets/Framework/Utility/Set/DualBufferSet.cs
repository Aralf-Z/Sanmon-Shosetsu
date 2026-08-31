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
        private readonly Dictionary<int, HashSet<T>> _mainSet = new ();
        private readonly List<int> _order = new ();
        private readonly List<T> _pendingAdd = new ();
        private readonly List<T> _pendingRemove = new ();
        
        public void Add(T item)
        {
            item.SetStatus(BufferStatus.PendingAdd);
            _pendingAdd.Add(item);
            item.OnAdd();
        }
        
        private void Remove(T item)
        {
            item.SetStatus(BufferStatus.PendingRemove);
            _pendingRemove.Add(item);
            item.OnRemove();
        }

        public void Clear()
        {
            foreach (var items in _mainSet.Values)
            {
                foreach (var item in items)
                {
                    item.OnRemove();
                    item.SetStatus(BufferStatus.None);
                }
            }
            
            _mainSet.Clear();
            _order.Clear();
            _pendingAdd.Clear();
            _pendingRemove.Clear();
        }
        
        public void Update(float dt)
        {
            var needOrder = false;
            
            //更新
            foreach (var order in _order)
            {
                var items = _mainSet[order];

                foreach (var item in items)
                {
                    if(item.Status is BufferStatus.Running)
                        item.OnUpdate(dt);
                
                    if(item.Status is BufferStatus.Dirty)
                        Remove(item);
                }
            }

            //添加
            foreach (var item in _pendingAdd)
            {
                item.SetStatus(BufferStatus.Running);
                item.OnAdd();
                if (_mainSet.TryGetValue(item.Order, out var set))
                {
                    set.Add(item);
                }
                else
                {
                    _mainSet.Add(item.Order, new HashSet<T> { item });
                    _order.Add(item.Order);
                    needOrder =  true;
                }
            }

            //移出
            foreach (var item in _pendingRemove)
            {
                item.OnRemove();
                item.SetStatus(BufferStatus.None);
                _mainSet[item.Order].Remove(item);
            }
            
            //排序
            if(needOrder) _order.Sort();
            
            _pendingAdd.Clear();
            _pendingRemove.Clear();
        }
    }
}