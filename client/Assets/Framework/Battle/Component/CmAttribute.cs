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
        public IReadOnlyDictionary<int, SumValue> Attri => mAttri;
        
        private readonly Dictionary<int, SumValue> mAttri = new ();
        
        public SumValue this[int attribute] => mAttri.GetValueOrDefault(attribute, SumValue.DEFAULT);
        public SumValue this[Attribute attribute] => this[(int)attribute];
        public SumValue Get(int attribute) => this[attribute];
        public SumValue Get(Attribute attribute) => this[attribute];
        
        public SumValue AddValue(int attribute, float value)
        {
            var sum = new SumValue(value);
            mAttri.Add(attribute, sum);
            return sum;
        }
        
        public SumValue AddValue(Attribute attribute, float value)
        {
            return AddValue((int)attribute, value);
        }
        
        public void RemoveValue(int attribute)
        {
            mAttri.Remove(attribute);
        }
        
        public void RemoveValue(Attribute attribute)
        {
            mAttri.Remove((int)attribute);
        }
    }
}