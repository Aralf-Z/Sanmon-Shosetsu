using System;
using System.Collections.Generic;

namespace GameConsole.Extension
{
    /// <summary> 定义命令和参数在作弊面板中的显示别名. </summary>
    public class MethodAliasAttribute : Attribute
    {
        /// <summary> Method alias. </summary>
        public readonly string methodAlias;

        /// <summary> Param alias. </summary>
        public IReadOnlyList<string> ParamAlias => _paramAlias;

        private readonly string[] _paramAlias; // Param alias.

        /// <summary> 方法和参数显示别名. 参数以竖线按位置分隔, 空项或缺失项使用参数声明名称. </summary>
        public MethodAliasAttribute(string methodAlias, params string[] paramAlias)
        {
            this.methodAlias = methodAlias;
            _paramAlias = paramAlias;
        }
    }
}
