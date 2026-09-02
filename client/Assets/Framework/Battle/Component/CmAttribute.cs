using System.Collections.Generic;
using Game.Config.Battle;
using Sanmon.GameEntity;
using Sanmon.Utility.Value;

namespace Sanmon.Battle
{
    /// <summary>
    /// 属性组件
    /// </summary>
    public class CmAttribute: ComponentBase
    {
        public IReadOnlyDictionary<Attribute, SumValue> Attri => mAttri;
        
        private readonly Dictionary<Attribute, SumValue> mAttri = new ();
        
        public SumValue this[Attribute attribute] => mAttri.GetValueOrDefault(attribute, SumValue.DEFAULT);
        public SumValue this[int attribute] => this[(Attribute)attribute];

        public SumValue AddValue(Attribute attribute, float value)
        {
            var sum = new SumValue(value);
            mAttri.Add(attribute, sum);
            return sum;
        }
        
        public void RemoveValue(Attribute attribute)
        {
            mAttri.Remove(attribute);
        }
    }
}