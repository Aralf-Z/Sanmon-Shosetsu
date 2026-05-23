using GameConsole.Extension;
using GameConsole.GameUI;
using RedSaw.CommandLineInterface;
using UnityEngine;

namespace GameConsole.Implementor
{
    public static class DefaultCommand
    {
        private const string kBattleTag = "战斗";
        private const string kChaTag = "玩家";

        private static GameDevTools sGameDevTools;

        private static GameDevTools Gdt => sGameDevTools ??= Object.FindFirstObjectByType<GameDevTools>();

        [Command(desc: "打印")]
        //[CheatIgnore]
        private static void Print(string str, LogType logType = LogType.Log)
        {
            Gdt.Print(str, logType);
        }
    }
}