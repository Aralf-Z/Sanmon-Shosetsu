using System;
using System.Collections.Generic;
using Sanmon.GameEntity;
using Sanmon.Utility.Value;
using UnityEngine;

namespace Framework.Battle
{
    /// <summary>
    /// 资源属性组件
    /// </summary>
    public class CmResource: ComponentBase
    {
        private class ResourceInfo
        {
            public readonly string key;
            public readonly SumValue maxValue;
            public float value;

            public ResourceInfo(string key, SumValue maxValue, float value)
            {
                this.key = key;
                this.maxValue = maxValue;
                this.value = value;
            }
        }

        public IReadOnlyCollection<string> Name => mRes.Keys;
        
        private readonly Dictionary<string, ResourceInfo> mRes = new ();

        public float this[string name] => mRes.GetValueOrDefault(name)?.value ?? 0f;
        
        public void Add(string key, SumValue maxValue, float value)
        {
            mRes.Add(key, new ResourceInfo(key, maxValue, value));
        }

        public void Remove(string key)
        {
            var info = mRes[key];
            mRes.Remove(key);
        }

        public void Change(string key, float value)
        {
            var info = mRes[key];
            info.value = Mathf.Clamp(value, 0, info.maxValue.Value);
        }
    }
}