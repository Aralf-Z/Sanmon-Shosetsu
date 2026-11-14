/*
    provide an attribute to mark debug info, when we use CommandHelper.Debug(), we can get all debug info
    include parent object's debug info
*/
using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> mark one field or property is debug info, when we use CommandHelper.Debug(), we can get all debug info include parent object&apos;s debug info. </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DebugInfoAttribute : Attribute
    {
        /// <summary> Key. </summary>
        public string Key { get; protected set; }

        /// <summary> you must provide an color key, and any invalid colors would be changed to white. </summary>
        public string Color { get; set; }

        /// <summary> Create a DebugInfoAttribute instance. </summary>
        public DebugInfoAttribute(string key)
        {
            this.Key = key;
            this.Color = "#ffffff";
        }

        /// <summary> Create a DebugInfoAttribute instance. </summary>
        public DebugInfoAttribute()
        {
            this.Key = string.Empty;
            this.Color = "#ffffff";
        }
    }
}
