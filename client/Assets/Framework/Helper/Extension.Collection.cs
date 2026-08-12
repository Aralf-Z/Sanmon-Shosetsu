using System;
using System.Collections.Generic;

namespace Sanmon.Helper
{
    public static partial class Extension
    {
        #region List

        public static T Random<T>(this List<T> list)
        {
            if (list == null || list.Count == 0) return default;
            return list[UnityEngine.Random.Range(0, list.Count)];
        }
        
        public static TValue TryGet<TValue>(this List<TValue> list, int index)
        {
            if(index < list.Count && index >= 0) return list[index];
            return default;
        }

        #endregion

        #region Dictionary

        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) where TValue : new()
        {
            if (dict.TryGetValue(key, out var value)) return value;

            value = new TValue();
            dict.Add(key, value);
            return value;
        }

        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> factory)
        {
            if (dict.TryGetValue(key, out var value)) return value;

            value = factory.Invoke();
            dict.Add(key, value);
            return value;
        }

        #endregion
    }
}