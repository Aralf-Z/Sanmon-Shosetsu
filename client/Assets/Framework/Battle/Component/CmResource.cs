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

            public ResourceInfo(Attribute key, SumValue maxValue)
            {
                this.key = key;
                this.maxValue = maxValue;
                this.value = maxValue.Value;
            }
        }

        public IReadOnlyCollection<Attribute> Name => _res.Keys;
        
        private readonly Dictionary<Attribute, ResourceInfo> _res = new ();

        public float this[Attribute name] => _res.GetValueOrDefault(name)?.value ?? 0f;
        
        public void Add(Attribute key, SumValue maxValue)
        {
            _res.Add(key, new ResourceInfo(key, maxValue));
        }

        public void Remove(Attribute key)
        {
            //var info = _res[key];
            _res.Remove(key);
        }
    }
}