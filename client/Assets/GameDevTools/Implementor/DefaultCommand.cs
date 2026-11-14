using System;
using RedSaw.CommandLineInterface;
using UnityEngine;
using GameConsole.Extension;

namespace GameConsole.Implementor
{
    /// <summary> Default command. </summary>
    public static class DefaultCommand
    {
        private static GameDevToolsController Gdt => GameDevToolsController.Instance;

        // 按日志类型对应的颜色输出消息.
        [Command]
        [MethodAlias("打印")]
        [CheatIgnore]
        private static void Print(string str, LogType logType = LogType.Log)
        {
            Gdt.Print(str, logType);
        }
    }
}
