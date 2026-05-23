using System;
using System.Collections.Generic;

namespace Sanmon.Utility.Value
{
    /// <summary>
    /// 总值
    /// </summary>
    public class SumValue
    {
        public static readonly SumValue Default = new SumValue(0f); 
        public float Value { get; private set; }
        public float Ratio => 1 + mSourceRatiosSum;
        
        public float BaseValue { get;}

        private float mSourceValuesSum;
        private readonly HashSet<SourceValue> mSourceValues = new ();
        public IReadOnlyCollection<SourceValue> SourceValues => mSourceValues;
        
        private float mSourceRatiosSum;
        private readonly HashSet<SourceValue> mSourceRatios = new ();
        public IReadOnlyCollection<SourceValue> SourceRatios => mSourceRatios;

        /// <summary> float: preValue, float: preRatio </summary>
        public event Action<SumValue, float, float> Evt_ValueChanged;
        
        public SumValue(float value)
        {
            BaseValue = Value = value;
        }
        
        public void AddValue(SourceValue value)
        {
            var preValue = Value;
            var preRatio = Ratio;
            mSourceValues.Add(value);
            mSourceValuesSum += value.Value;
            Value = mSourceValuesSum * Ratio;
            Evt_ValueChanged?.Invoke(this, preValue, preRatio);
        }

        public void RemoveValue(SourceValue value)
        {
            var preValue = Value;
            var preRatio = Ratio;
            mSourceValues.Remove(value);
            mSourceValuesSum -= value.Value;
            Value = mSourceValuesSum * Ratio;
            Evt_ValueChanged?.Invoke(this, preValue, preRatio);
        }
        
        public void AddRatio(SourceValue value)
        {
            var preValue = Value;
            var preRatio = Ratio;
            mSourceRatios.Add(value);
            mSourceRatiosSum += value.Value;
            Value = mSourceRatiosSum * Ratio;
            Evt_ValueChanged?.Invoke(this, preValue, preRatio);
        }

        public void RemoveRatio(SourceValue value)
        {
            var preValue = Value;
            var preRatio = Ratio;
            mSourceRatios.Remove(value);
            mSourceRatiosSum -= value.Value;
            Value = mSourceRatiosSum * Ratio;
            Evt_ValueChanged?.Invoke(this, preValue, preRatio);
        }
    }
}