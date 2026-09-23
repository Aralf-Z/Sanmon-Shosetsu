using System;

namespace Sanmon.Utility.Monitor
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class MonitorAttribute : Attribute
    {
        public string Tag { get; }

        public MonitorAttribute(string tag = null)
        {
            Tag = tag;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field/* | AttributeTargets.Property*/)]
    public class MonitorInfoAttribute : Attribute
    {
        public string Name { get; }
        public int Order { get; }

        public MonitorInfoAttribute(string name = null, int order = 0)
        {
            Name = name;
            Order = order;
        }
    }
}