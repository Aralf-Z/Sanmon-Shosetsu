using System;

namespace Sanmon.Utility.Inspector
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class InspectableAttribute : Attribute
    {
        public string Tag { get; }

        public InspectableAttribute(string tag = null)
        {
            Tag = tag;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field/* | AttributeTargets.Property*/)]
    public class InspectInfoAttribute : Attribute
    {
        public string Name { get; }
        public int Order { get; }

        public InspectInfoAttribute(string name = null, int order = 0)
        {
            Name = name;
            Order = order;
        }
    }
}