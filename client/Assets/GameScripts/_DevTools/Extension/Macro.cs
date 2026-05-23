using System;

namespace GameConsole.Extension
{
    public class MacroAttribute: Attribute
    {
        public const int CUSTOM_MACRO_ORDER = 100;
        public const int METHOD_DEFINE_ORDER = 500;
        public const int NO_PARAM_METHOD_ORDER = 1000;
        
        public readonly string name;
        public readonly string command;
        public readonly string value;
        public readonly int order;

        public MacroAttribute(string name, string command, string value = null, int order = METHOD_DEFINE_ORDER)
        {
            this.name = name;
            this.command = command;
            this.value = value;
            this.order = order;
        }
    }
}