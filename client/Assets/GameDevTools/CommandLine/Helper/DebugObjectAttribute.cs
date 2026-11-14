/*
    provide an attribute to mark debug info, when we use CommandHelper.Debug(), we can get all debug info
    include parent object's debug info
*/
using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> mark one element is container of debug info. </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public class DebugObjectAttribute : Attribute
    {
        /// <summary> Title. </summary>
        public string Title { get; protected set; }

        /// <summary> Create a DebugObjectAttribute instance. </summary>
        public DebugObjectAttribute(string title)
        {
            this.Title = title;
        }

        /// <summary> Create a DebugObjectAttribute instance. </summary>
        public DebugObjectAttribute()
        {
            this.Title = null;
        }
    }
}
