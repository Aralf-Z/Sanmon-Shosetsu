using System;
using System.Collections.Generic;

namespace GameConsole.Extension
{
    public class MethodAliasAttribute: Attribute
    {
        public readonly string methodAlias;
        public IReadOnlyList<string> ParamAlias => mParamAlias;

        private readonly string[] mParamAlias;

        /// <summary>
        /// 字段信息
        /// </summary>
        /// <param name="methodAlias"> 方法别名 </param>
        /// <param name="paramAlias"> 参数别名，按顺序以"|"分隔，要对齐所有的参数，不然会解析错位 </param>
        public MethodAliasAttribute(string methodAlias, string paramAlias = null)
        {
            this.methodAlias = methodAlias;
            mParamAlias = string.IsNullOrEmpty(paramAlias) ? null : paramAlias.Split('|');
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