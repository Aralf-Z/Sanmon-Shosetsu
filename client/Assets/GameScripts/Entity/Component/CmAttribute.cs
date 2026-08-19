using System.Collections.Generic;
using Sanmon.GameEntity;
using Sanmon.Utility.Value;

namespace GameScripts
{
    /// <summary>
    /// 属性组件，string key, 如果存在性能瓶颈可修改枚举key
    /// </summary>
    public class CmAttribute: ComponentBase
    {
        public IReadOnlyDictionary<string, SumValue> Attri => mAttri;
        
        private readonly Dictionary<string, SumValue> mAttri = new ();
        
        public SumValue this[string name] => mAttri.GetValueOrDefault(name, SumValue.DEFAULT);

        public SumValue AddValue(string name, float value)
        {
            var sum = new SumValue(value);
            mAttri.Add(name, sum);
            return sum;
        }
        
        public void RemoveValue(string name)
        {
            mAttri.Remove(name);
        }
    }
}