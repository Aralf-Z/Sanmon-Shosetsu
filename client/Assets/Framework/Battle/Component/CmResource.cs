using Game.Config.Battle;
using System.Collections.Generic;
using Sanmon.GameEntity;
using Sanmon.Utility.Value;

namespace Sanmon.Battle
{
    /// <summary>
    /// 资源属性组件
    /// </summary>
    public class CmResource: ComponentBase
    {
        private class ResourceInfo
        {
            public readonly Attribute key;
            public readonly SumValue maxValue;
            public float value;

            public ResourceInfo(Attribute key, SumValue maxValue, float value)
            {
                this.key = key;
                this.maxValue = maxValue;
                this.value = value;
            }
        }

        public IReadOnlyCollection<Attribute> Name => mRes.Keys;
        
        private readonly Dictionary<Attribute, ResourceInfo> mRes = new ();

        public float this[Attribute name] => mRes.GetValueOrDefault(name)?.value ?? 0f;
        
        public void Add(Attribute key, SumValue maxValue, float value)
        {
            mRes.Add(key, new ResourceInfo(key, maxValue, value));
        }

        public void Remove(Attribute key)
        {
            //var info = mRes[key];
            mRes.Remove(key);
        }
    }
}