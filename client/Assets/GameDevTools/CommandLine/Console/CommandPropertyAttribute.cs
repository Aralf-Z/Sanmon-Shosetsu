using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> any static property or field marked by CommandPropertyAttribute could be visit directly by @ in command line. </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class CommandPropertyAttribute : Attribute
    {
        /// <summary> Name. </summary>
        public readonly string name;

        /// <summary> Tag. </summary>
        public readonly string tag;

        /// <summary> Desc. </summary>
        public readonly string desc;

        /// <summary> Create a CommandPropertyAttribute instance. </summary>
        public CommandPropertyAttribute(string name)
        {
            this.name = name;
            this.desc = "property has no description";
            this.tag = null;
        }

        /// <summary> Create a CommandPropertyAttribute instance. </summary>
        public CommandPropertyAttribute()
        {
            this.name = null;
            this.desc = "property has no description";
            this.tag = null;
        }
    }
}
