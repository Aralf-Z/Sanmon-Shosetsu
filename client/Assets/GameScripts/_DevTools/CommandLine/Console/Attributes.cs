using System;

namespace RedSaw.CommandLineInterface{

    /// <summary>
    /// command attribute
    /// <para>any static method has attach this attribute would be treated
    /// as a command method, and it woule be collected by command system automatically</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CommandAttribute : Attribute
    {

        /// <summary> name of command </summary>
        public readonly string name;
        
        /// <summary> description of command </summary>
        public readonly string desc;

        /// <summary> tag of command, for you can query commands by tag, or constraint commands by tag </summary>
        public readonly string tag;

        public CommandAttribute(string name = null, string desc =null,  string tag= null){

            this.name = name;
            this.tag = tag;
            this.desc = desc ?? "command has no description";
        }
    }

    /// <summary>
    /// a static method which defined this attribute would be treated as
    /// parameter parser.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CommandValueParserAttribute : Attribute{

        public readonly Type type;
        public string Alias { get; set; }

        public CommandValueParserAttribute(Type type){

            this.type = type;
            this.Alias = null;
        }
    }

    /// <summary>
    /// any static property or field marked by CommandPropertyAttribute could be visit directly by @ in command line
    /// </summary>
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class CommandPropertyAttribute : Attribute{

        public readonly string Name;
        public readonly string Tag;
        public readonly string Desc;

        public CommandPropertyAttribute(string name){

            this.Name = name;
            this.Desc = "property has no description";
            this.Tag = null;
        }
        public CommandPropertyAttribute(){

            this.Name = null;
            this.Desc = "property has no description";
            this.Tag = null;
        }
    }


}