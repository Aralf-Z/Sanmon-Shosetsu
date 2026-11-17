using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> mark one element is container of debug info. </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public class DebugObjectAttribute : Attribute
    {
        public string Title { get; protected set; }

        public DebugObjectAttribute(string title)
        {
            this.Title = title;
        }

        public DebugObjectAttribute()
        {
            this.Title = null;
        }
    }
}
