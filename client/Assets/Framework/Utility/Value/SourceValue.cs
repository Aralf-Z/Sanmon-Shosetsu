using System;

namespace Sanmon.Utility.Value
{
    public struct SourceValue : IEquatable<SourceValue>
    {
        public string Name { get; private set; }
        public float Value { get; private set; }

        public SourceValue(string name, int value)
        {
            Name = name;
            Value = value;
        }

        public SourceValue(string name, float value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            return $"{(Value >= 0 ? "+" : "-")}{Value} [{Name}]";
        }
        
        public string ToPercentString()
        {
            return $"{(Value >= 0 ? "+" : "-")}{Value * 100}% [{Name}]";
        }

        public string ToIntString()
        {
            var value = (int)Value;
            return $"{(value >= 0 ? "+" : "-")}{value} [{Name}]";
        }

        public override bool Equals(object obj)
        {
            if (obj is SourceValue other)
            {
                var delta = Value - other.Value;
                return Name == other.Name && delta is < .00001f and > -.00001f;
            }
            return false;
        }

        public override int GetHashCode()
        {
            var value = MathF.Round(Value, 5);
            return HashCode.Combine(Name, value);
        }

        public bool Equals(SourceValue other)
        {
            var delta = Value - other.Value;
            return Name == other.Name && delta is < .00001f and > -.00001f;
        }
    }
}