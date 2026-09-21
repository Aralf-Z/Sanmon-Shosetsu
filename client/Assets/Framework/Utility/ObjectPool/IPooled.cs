using System.Collections.Generic;

namespace Sanmon.Utility.ObjectPool
{
    /// <summary>
    /// 对象接口
    /// </summary>
    public interface IPooled
    {
        bool IsCollected { get; set; }

        void OnNew();
        void OnRequire();
        void OnRecycle();
    }
}