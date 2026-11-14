using System;

namespace GameConsole.Extension
{
    /// <summary> 使命令不出现在作弊面板中. </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CheatIgnoreAttribute : Attribute
    {
    }
}
