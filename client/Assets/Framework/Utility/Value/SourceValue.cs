using System;

namespace Sanmon.Utility.Value
{
    public struct SourceValue
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
                return Name == other.Name && Value.Equals(other.Value);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Value);
        }
    }
}