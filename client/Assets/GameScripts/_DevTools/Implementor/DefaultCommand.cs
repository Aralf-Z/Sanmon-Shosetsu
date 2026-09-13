using GameConsole.Extension;
using GameConsole.GameUI;
using RedSaw.CommandLineInterface;
using UnityEngine;
using GameScripts;
using Logger = Sanmon.Helper.Logger;

namespace GameConsole.Implementor
{
    public static class DefaultCommand
    {
        [Command]
        public static void LogTalentTree()
        {
            foreach (var td in GameScripts.Game.Tables.TbTalentData.DataList)
            {
                Logger.LogInfo(FormatDesc(td.Desc, td.DescParams));
            }
        }
        
        private static string FormatDesc(string desc, Game.Config.Character.ParamDefine[] defines)
        {
            var args = new object[defines.Length];

            for (var i = 0; i < args.Length; i++)
            {
                args[i] = defines[i].BaseValue[0].ToString(defines[i].Format);
            }
            
            return string.Format(desc, args);
        }
    }
}