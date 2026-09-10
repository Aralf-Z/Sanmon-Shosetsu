using UnityEngine;

namespace Sanmon.Helper
{
    public partial class Extension
    {
        /// <summary>
        /// 小数进一取整
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int Ceil(this float value) => Mathf.CeilToInt(value);
        
        /// <summary>
        /// 小数去尾取整
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int Floor(this float value) => Mathf.FloorToInt(value);
        
        /// <summary>
        /// 小数四舍五入取整
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int Round(this float value) => Mathf.RoundToInt(value);
    }
}