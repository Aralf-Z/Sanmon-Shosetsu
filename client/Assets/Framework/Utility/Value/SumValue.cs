using System;
using System.Collections.Generic;

namespace Sanmon.Utility.Value
{
    /// <summary>
    /// 总值
    /// </summary>
    public class SumValue
    {
        public static readonly SumValue DEFAULT = new SumValue(0f); 
        public float Value { get; private set; }
        public float Ratio => 1 + _sourceRatiosSum;
        
        public float BaseValue { get;}

        private float _sourceValuesSum;
        private readonly HashSet<SourceValue> _sourceValues = new ();
        public IReadOnlyCollection<SourceValue> SourceValues => _sourceValues;
        
        private float _sourceRatiosSum;
        private readonly HashSet<SourceValue> _sourceRatios = new ();
        public IReadOnlyCollection<SourceValue> SourceRatios => _sourceRatios;

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
            _sourceValues.Add(value);
            _sourceValuesSum += value.Value;
            Value = _sourceValuesSum * Ratio;
            Evt_ValueChanged?.Invoke(this, preValue, preRatio);
        }

        public void RemoveValue(SourceValue value)
        {
            var preValue = Value;
            var preRatio = Ratio;
            _sourceValues.Remove(value);
            _sourceValuesSum -= value.Value;
            Value = _sourceValuesSum * Ratio;
            Evt_ValueChanged?.Invoke(this, preValue, preRatio);
        }
        
        public void AddRatio(SourceValue value)
        {
            var preValue = Value;
            var preRatio = Ratio;
            _sourceRatios.Add(value);
            _sourceRatiosSum += value.Value;
            Value = _sourceRatiosSum * Ratio;
            Evt_ValueChanged?.Invoke(this, preValue, preRatio);
        }

        public void RemoveRatio(SourceValue value)
        {
            var preValue = Value;
            var preRatio = Ratio;
            _sourceRatios.Remove(value);
            _sourceRatiosSum -= value.Value;
            Value = _sourceRatiosSum * Ratio;
            Evt_ValueChanged?.Invoke(this, preValue, preRatio);
        }
    }
}