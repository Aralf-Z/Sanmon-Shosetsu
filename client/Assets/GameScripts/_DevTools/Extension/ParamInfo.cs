using System;
using System.Collections.Generic;

namespace GameConsole.Extension
{
    public class ParamAliasAttribute: Attribute
    {
        public IReadOnlyList<string> alias;

        private string[] mAlias;

        /// <summary>
        /// 字段信息
        /// </summary>
        /// <param name="alias">
        /// 别名，按顺序以"|"分隔 <para> 注意：如果一定要填，就要对齐所有的参数，不然会解析错位 </para></param>
        public ParamAliasAttribute(string alias)
        {
            mAlias = string.IsNullOrEmpty(alias) ? new string[]{} : alias.Split('|');
        }
    }
    
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