using System;
using UnityEngine;
using GameConsole.Extension;
using RedSaw.CommandLineInterface;

namespace GameConsole.Implementor
{
    /// <summary> Default command. </summary>
    public static class DefaultCommand
    {
        private static GameDevToolsController Gdt => GameDevToolsController.Instance;

        [Command]
        [MethodAlias("打印")]
        [CheatIgnore]
        private static void Print(string str, LogType logType = LogType.Log)
        {
            Gdt.Print(str, logType);
        }
    }
}
