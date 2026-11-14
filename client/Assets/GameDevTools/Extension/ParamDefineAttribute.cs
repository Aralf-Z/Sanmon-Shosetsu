using System;

namespace GameConsole.Extension
{
    /// <summary> 指定为命令提供预设参数的静态收集方法. </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ParamDefineAttribute : Attribute
    {
        /// <summary> Collect class. </summary>
        public readonly Type collectClass;

        /// <summary> Collect method. </summary>
        public readonly string collectMethod;

        /// <summary> Create a ParamDefineAttribute instance. </summary>
        public ParamDefineAttribute(Type collectClass, string collectMethod)
        {
            this.collectClass = collectClass;
            this.collectMethod = collectMethod;
        }
    }
}
