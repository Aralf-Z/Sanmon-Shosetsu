using System;

namespace GameConsole.Extension
{
    /// <summary> 为当前注册命令定义内置宏, 参数原文交给现有命令系统解析. </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class MacroAttribute : Attribute
    {
        public readonly string name;

        public readonly string parameters;

        public readonly string desc;

        public MacroAttribute(string name, string parameters, string desc = "")
        {
            this.name = name;
            this.parameters = parameters;
            this.desc = desc;
        }
    }
}
