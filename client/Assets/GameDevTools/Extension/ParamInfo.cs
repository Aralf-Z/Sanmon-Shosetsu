using System;

namespace GameConsole.Extension
{
    /// <summary> Param info. </summary>
    public class ParamInfo
    {
        public readonly string name;

        public readonly string alias;

        public readonly string defaultValue;

        public readonly Type type;

        public ParamInfo(string name, string alias, Type type, string defaultValue)
        {
            this.name = name;
            this.alias = alias;
            this.type = type;
            this.defaultValue = defaultValue;
        }
    }
}
