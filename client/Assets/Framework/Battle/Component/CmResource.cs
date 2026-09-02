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
            public readonly Attribute attribute;
            public readonly SumValue maxValue;
            public float value;

            public ResourceInfo(Attribute attribute, SumValue maxValue)
            {
                this.attribute = attribute;
                this.maxValue = maxValue;
                this.value = maxValue.Value;
            }
        }

        public IReadOnlyCollection<Attribute> Name => _res.Keys;
        
        private readonly Dictionary<Attribute, ResourceInfo> _res = new ();

        public float this[Attribute attribute] => _res.GetValueOrDefault(attribute)?.value ?? 0f;
        public float this[int attribute] => this[(Attribute)attribute];
        
        public void Add(Attribute attribute, SumValue maxValue)
        {
            _res.Add(attribute, new ResourceInfo(attribute, maxValue));
        }

        public void Remove(Attribute attribute)
        {
            //var info = _res[attribute];
            _res.Remove(attribute);
        }
    }
}