using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> command attribute any static method has attach this attribute would be treated as a command method, and it woule be collected by command system automatically. </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CommandAttribute : Attribute
    {
        /// <summary> name of command. </summary>
        public readonly string name;

        /// <summary> description of command. </summary>
        public readonly string desc;

        /// <summary> tag of command, for you can query commands by tag, or constraint commands by tag. </summary>
        public readonly string tag;

        /// <summary> Create a CommandAttribute instance. </summary>
        public CommandAttribute(string name = null, string desc = null, string tag = null)
        {
            this.name = name;
            this.tag = tag;
            this.desc = desc ?? "command has no description";
        }
    }
}
