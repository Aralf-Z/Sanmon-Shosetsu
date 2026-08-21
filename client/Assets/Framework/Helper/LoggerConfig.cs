using System.Collections.Generic;
using Sanmon.Core;

namespace Sanmon.Helper
{
    internal class LoggerConfig: AppConfig<LoggerConfig>
    {
        public enum Level
        {
            None    = 0,
            Info    = 1,
            Warning = 2,
            Error   = 3,
            Debug   = 4,
        }

        public List<string> colors = new ()
        {
            "#FFFFFF",//白色
            "#FFFFFF",//白色
            "#FFFF00",//黄色
            "#FF0000",//红色
            "#00FFFF",//蓝绿色
        };
        
        public bool logTimeStamp = false;
        public Level level = Level.Debug;
        
        
    }
}