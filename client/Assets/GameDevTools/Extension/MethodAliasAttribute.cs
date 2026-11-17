using System;
using System.Collections.Generic;

namespace GameConsole.Extension
{
    /// <summary> 定义命令和参数在作弊面板中的显示别名. </summary>
    public class MethodAliasAttribute : Attribute
    {
        public readonly string methodAlias;

        public IReadOnlyList<string> ParamAlias => _paramAlias;

        private readonly string[] _paramAlias;

        public MethodAliasAttribute(string methodAlias, params string[] paramAlias)
        {
            this.methodAlias = methodAlias;
            _paramAlias = paramAlias;
        }
    }
}
