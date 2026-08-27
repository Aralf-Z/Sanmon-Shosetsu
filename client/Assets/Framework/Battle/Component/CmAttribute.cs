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
        
        public SumValue this[Attribute name] => mAttri.GetValueOrDefault(name, SumValue.DEFAULT);

        public SumValue AddValue(Attribute name, float value)
        {
            var sum = new SumValue(value);
            mAttri.Add(name, sum);
            return sum;
        }
        
        public void RemoveValue(Attribute name)
        {
            mAttri.Remove(name);
        }
    }
}