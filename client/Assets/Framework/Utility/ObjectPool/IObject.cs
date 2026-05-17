using System.Collections.Generic;

namespace Sanmon.Utility.ObjectPool
{
    /// <summary>
    /// 对象接口
    /// </summary>
    public interface IObject<T>
    {
        bool IsCollected { get; set; }

        void OnNew();
        void OnRequire();
        void OnRecycle();
    }
}