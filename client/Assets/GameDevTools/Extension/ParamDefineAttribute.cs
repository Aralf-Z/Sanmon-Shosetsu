using System;

namespace GameConsole.Extension
{
    /// <summary> 指定为命令提供预设参数的静态收集方法. </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ParamDefineAttribute : Attribute
    {
        public readonly Type collectClass;

        public readonly string collectMethod;

        public ParamDefineAttribute(Type collectClass, string collectMethod)
        {
            this.collectClass = collectClass;
            this.collectMethod = collectMethod;
        }
    }
}
