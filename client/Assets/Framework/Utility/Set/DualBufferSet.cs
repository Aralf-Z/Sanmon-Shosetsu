using System.Collections.Generic;
using Unity.Android.Types;

namespace Sanmon.Utility.Set
{
    /// <summary>
    /// 双缓冲容器，可以排序遍历
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DualBufferSet<T>
    {
        private readonly List<T> mMainSet = new List<T>();
        private readonly List<T> mAddCache = new List<T>();
        private readonly List<T> mRemoveCache = new List<T>();

        public void asd()
        {
            mAddCache.Sort();
        }
    }
}