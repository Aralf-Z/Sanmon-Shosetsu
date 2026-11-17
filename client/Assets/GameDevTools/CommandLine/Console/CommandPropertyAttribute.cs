using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> any static property or field marked by CommandPropertyAttribute could be visit directly by @ in command line. </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class CommandPropertyAttribute : Attribute
    {
        public readonly string name;

        public readonly string tag;

        public readonly string desc;

        public CommandPropertyAttribute(string name)
        {
            this.name = name;
            this.desc = "property has no description";
            this.tag = null;
        }

        public CommandPropertyAttribute()
        {
            this.name = null;
            this.desc = "property has no description";
            this.tag = null;
        }
    }
}
