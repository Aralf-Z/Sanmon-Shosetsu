using System;

namespace GameConsole.Extension
{
    /// <summary> Param info. </summary>
    public class ParamInfo
    {
        /// <summary> Name. </summary>
        public readonly string name;

        /// <summary> Alias. </summary>
        public readonly string alias;

        /// <summary> Default value. </summary>
        public readonly string defaultValue;

        /// <summary> Type. </summary>
        public readonly Type type;

        /// <summary> Create a ParamInfo instance. </summary>
        public ParamInfo(string name, string alias, Type type, string defaultValue)
        {
            this.name = name;
            this.alias = alias;
            this.type = type;
            this.defaultValue = defaultValue;
        }
    }
}
