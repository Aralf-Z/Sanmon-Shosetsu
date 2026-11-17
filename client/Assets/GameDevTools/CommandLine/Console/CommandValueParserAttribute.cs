using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> a static method which defined this attribute would be treated as parameter parser. </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CommandValueParserAttribute : Attribute
    {
        public readonly Type type;

        public string Alias { get; set; }

        public CommandValueParserAttribute(Type type)
        {
            this.type = type;
            this.Alias = null;
        }
    }
}
