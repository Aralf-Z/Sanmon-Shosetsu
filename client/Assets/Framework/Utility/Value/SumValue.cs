using System.Collections.Generic;

namespace Sanmon.Utility.Value
{
    /// <summary>
    /// 总值
    /// </summary>
    public class SumValue
    {
        public float Value { get; private set; }
        public float Ratio => mSourceRatiosSum;
        
        public float BaseValue { get;}

        private float mSourceValuesSum;
        public IEnumerable<SourceValue> SourceValues => mSourceValues;
        private HashSet<SourceValue> mSourceValues;
        
        private float mSourceRatiosSum;
        public IEnumerable<SourceValue> SourceRatios => mSourceRatios;
        private HashSet<SourceValue> mSourceRatios;

        public SumValue(float value)
        {
            BaseValue = Value = value;
        }
        
        public void Add(SourceValue value)
        {
            mSourceValues.Add(value);
            mSourceValuesSum += value.Value;
            Value = mSourceValuesSum * (1 + mSourceRatiosSum);
        }

        public void Remove(SourceValue value)
        {
            mSourceValues.Remove(value);
            mSourceValuesSum -= value.Value;
            Value = mSourceValuesSum * (1 + mSourceRatiosSum);
        }
        
        public void AddRatio(SourceValue value)
        {
            mSourceRatios.Add(value);
            mSourceRatiosSum += value.Value;
            Value = mSourceRatiosSum * (1 + mSourceRatiosSum);
        }

        public void RemoveRatio(SourceValue value)
        {
            mSourceRatios.Remove(value);
            mSourceRatiosSum -= value.Value;
            Value = mSourceRatiosSum * (1 + mSourceRatiosSum);
        }
    }
}