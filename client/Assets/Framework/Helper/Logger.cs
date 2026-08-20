using System;
using UnityEngine;

namespace Sanmon.Helper
{
    /// <summary>
    /// Unity的潜规则：Logger结尾的类 + Log开头的方法控制台双击跳转会到调用该方法的路径
    /// </summary>
    public static class Logger
    {
        //todo Warning和Error不走Log
        private static LoggerConfig Config => LoggerConfig.Ins;
        
        private static void Log(string message, LoggerConfig.Level level, string title = null, Color color = default)
        {
            Log(message, level, title, color == default ? "#FFFFFF" : color.ColorToHex());
        }

        private static void Log(string message, LoggerConfig.Level level, string title = null, string color = "#FFFFFF")
        {
            var timeStamp = Config.logTimeStamp ? $"[{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00}:{DateTime.Now.Second:00}:{DateTime.Now.Millisecond:000}] " : "";
            var pre = level switch
            {
                LoggerConfig.Level.Info => $"<color={Config.colors[(int)LoggerConfig.Level.Info]}>[INFO]</color>",
                LoggerConfig.Level.Warning => $"<color={Config.colors[(int)LoggerConfig.Level.Warning]}>[WARN]</color>",
                LoggerConfig.Level.Error => $"<color={Config.colors[(int)LoggerConfig.Level.Error]}>[ERROR]</color>",
                LoggerConfig.Level.Debug => $"<color={Config.colors[(int)LoggerConfig.Level.Debug]}>[DEBUG]</color>",
                _ =>  $"<color={Config.colors[(int)LoggerConfig.Level.Info]}>[LOG]</color>",
            };
            Debug.Log($"{timeStamp}{pre}<color={color}>{(title == null ? "" : $"<{title}>  ")}{message}</color>");
        }
        
        /// <summary>
        /// 运行日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="color"></param>
        public static void LogInfo(string message, string title = null, Color color = default)
        {
            if (Config.level >=　LoggerConfig.Level.Info)
            {
                if (color == default)
                {
                    Log(message, LoggerConfig.Level.Info, title, Config.colors[(int)LoggerConfig.Level.Info]);
                }
                else
                {
                    Log(message, LoggerConfig.Level.Info, title, color);
                }
            };
        }

        /// <summary>
        /// 警告信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="color"></param>
        public static void LogWarning(string message, string title = null, Color color = default)
        {
            if(Config.level >= LoggerConfig.Level.Warning) 
            {
                if (color == default)
                {
                    Log(message, LoggerConfig.Level.Warning, title, Config.colors[(int)LoggerConfig.Level.Warning]);
                }
                else
                {
                    Log(message, LoggerConfig.Level.Warning, title, color);
                }
            };
        }

        /// <summary>
        /// 错误信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="color"></param>
        public static void LogError(string message, string title = null, Color color = default)
        {
            if(Config.level >= LoggerConfig.Level.Error) 
            {
                if (color == default)
                {
                    Log(message, LoggerConfig.Level.Error, title, Config.colors[(int)LoggerConfig.Level.Error]);
                }
                else
                {
                    Log(message, LoggerConfig.Level.Error, title, color);
                }
            };
        }
        
        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="color"></param>
        public static void LogDebug(string message, string title = null, Color color = default)
        {
            if(Config.level >=　LoggerConfig.Level.Debug) 
            {
                if (color == default)
                {
                    Log(message, LoggerConfig.Level.Debug, title, Config.colors[(int)LoggerConfig.Level.Debug]);
                }
                else
                {
                    Log(message, LoggerConfig.Level.Debug, title, color);
                }
            };
        }

        /// <summary>
        /// 包含帧信息的调试日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="color"></param>
        public static void LogFrame(string message, string title = null, Color color = default)
        {
            if(Config.level >=　LoggerConfig.Level.Debug) 
                LogDebug(message,$"[f:{Time.frameCount}] {(title == null ? "" : $"<{title}>")}", color);
        }

        /// <summary>
        /// 包含运行时间的调试日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="color"></param>
        public static void LogTime(string message, string title = null, Color color = default)
        {
            if(Config.level >=　LoggerConfig.Level.Debug) 
                LogDebug(message,$"[t:{Time.realtimeSinceStartup:F3}] {(title == null ? "" : $"<{title}>")}", color);
        }
    }
}