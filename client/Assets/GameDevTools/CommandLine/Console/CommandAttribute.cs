using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> command attribute any static method has attach this attribute would be treated as a command method, and it woule be collected by command system automatically. </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CommandAttribute : Attribute
    {
        public readonly string name;

        public readonly string desc;

        public readonly string tag;

        public CommandAttribute(string name = null, string desc = null, string tag = null)
        {
            this.name = name;
            this.tag = tag;
            this.desc = desc ?? "command has no description";
        }
    }
}
