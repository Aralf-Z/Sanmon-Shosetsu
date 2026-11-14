using System;

namespace GameConsole.Extension
{
    /// <summary> 为当前注册命令定义内置宏, 参数原文交给现有命令系统解析. </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class MacroAttribute : Attribute
    {
        /// <summary> Name. </summary>
        public readonly string name;

        /// <summary> 调用括号内的原始命令参数文本, 不拆分或自动补引号. </summary>
        public readonly string parameters;

        /// <summary> 显示描述. </summary>
        public readonly string desc;

        /// <summary> Create a MacroAttribute instance. </summary>
        public MacroAttribute(string name, string parameters, string desc = "")
        {
            this.name = name;
            this.parameters = parameters;
            this.desc = desc;
        }
    }
}
