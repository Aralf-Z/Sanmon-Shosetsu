using System.Collections.Generic;

namespace Sanmon.Helper
{
    public static partial class Extension
    {
        public static T Random<T>(this List<T> list)
        {
            if (list == null || list.Count == 0) return default;
            return list[UnityEngine.Random.Range(0, list.Count)];
        }
    }
}