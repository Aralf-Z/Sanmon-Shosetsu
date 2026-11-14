using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> a static method which defined this attribute would be treated as parameter parser. </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CommandValueParserAttribute : Attribute
    {
        /// <summary> Type. </summary>
        public readonly Type type;

        /// <summary> Alias. </summary>
        public string Alias { get; set; }

        /// <summary> Create a CommandValueParserAttribute instance. </summary>
        public CommandValueParserAttribute(Type type)
        {
            this.type = type;
            this.Alias = null;
        }
    }
}
