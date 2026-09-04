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
            public readonly int attribute;
            public readonly SumValue maxValue;
            public float value;

            public ResourceInfo(int attribute, SumValue maxValue)
            {
                this.attribute = attribute;
                this.maxValue = maxValue;
                this.value = maxValue.Value;
            }
        }

        public IReadOnlyCollection<int> Name => _res.Keys;
        
        private readonly Dictionary<int, ResourceInfo> _res = new ();

        public float this[int attribute] => _res.GetValueOrDefault(attribute)?.value ?? 0f;
        public float this[Attribute attribute] => this[(int)attribute];
        public float Get(int attribute) => this[attribute];
        
        public void Add(int attribute, SumValue maxValue)
        {
            _res.Add(attribute, new ResourceInfo(attribute, maxValue));
        }
        
        public void Add(Attribute attribute, SumValue maxValue)
        {
            Add((int)attribute, maxValue);
        }

        public void Remove(int attribute)
        {
            //var info = _res[attribute];
            _res.Remove(attribute);
        }
        
        public void Remove(Attribute attribute)
        {
            Remove((int)attribute);
        }

        public void ChangeValue(int attribute, float value)
        {
            _res[attribute].value += value;
        }
        
        public void ChangeValue(Attribute attribute, float value)
        {
            ChangeValue((int)attribute, value);
        }
    }
}