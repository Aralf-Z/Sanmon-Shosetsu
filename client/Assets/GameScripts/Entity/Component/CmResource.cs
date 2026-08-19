using System;
using System.Collections.Generic;
using Sanmon.GameEntity;
using Sanmon.Utility.Value;
using UnityEngine;

namespace GameScripts
{
    /// <summary>
    /// 资源型属性组件，string key, 如果存在性能瓶颈可修改枚举key. 
    /// 依赖于Attribute组件, 初始化顺序应低于Attribute. 
    /// 默认随着上限变化, 上限变小则不会超过上限，上限变大时，增加上限变化值
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
        private readonly Dictionary<SumValue, string> mValueMap = new ();

        public float this[string name] => mRes.GetValueOrDefault(name)?.value ?? 0f;
        
        /// <summary> float: preValue, float: nowValue </summary>
        public event Action<string, float, float> e_ValueChanged;
        
        public void Add(string key, SumValue maxValue, float value)
        {
            mRes.Add(key, new ResourceInfo(key, maxValue, value));
            mValueMap.Add(maxValue, key);
        }

        public void Remove(string key)
        {
            var info = mRes[key];
            mRes.Remove(key);
            mValueMap.Remove(info.maxValue);
        }

        public void Change(string key, float value)
        {
            var info = mRes[key];
            var preValue = info.value;
            info.value = Mathf.Clamp(value, 0, info.maxValue.Value);
            e_ValueChanged?.Invoke(key, preValue, info.value);
        }
    }
}