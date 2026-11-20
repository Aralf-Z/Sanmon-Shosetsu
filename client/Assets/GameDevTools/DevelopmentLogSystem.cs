using UnityEngine;
using RedSaw.CommandLineInterface;

namespace GameConsole
{
    /// <summary> 独立于总控和界面的主线程日志采集. </summary>
    internal static class DevelopmentLogSystem
    {
        internal static ConsoleLogStore Logs { get; } = new();

        private static bool _initialized; // 重复初始化不重复订阅, 不清空当前运行的日志.

        /// <summary> 在主线程建立会话订阅, 总控可重复调用以确保采集已启动. </summary>
        internal static void Initialize()
        {
            if (_initialized) return;

            Application.logMessageReceived += OnUnityLog;
            Application.quitting += Shutdown;
            _initialized = true;
        }

        /// <summary> 沿用原始结构化输出, 停止采集后不再写入已释放的会话缓存. </summary>
        internal static void Append(ConsoleLogRecord record)
        {
            if (_initialized)
            {
                Logs.Append(record);
            }
        }

        internal static void Clear() => Logs.Clear();

        internal static void Shutdown()
        {
            if (!_initialized) return;

            _initialized = false;
            Application.logMessageReceived -= OnUnityLog;
            Application.quitting -= Shutdown;
            Logs.Release();
        }

        internal static string GetHexColor(LogType type) => type switch
        {
            LogType.Error or LogType.Exception or LogType.Assert => "#b13c45",
            LogType.Warning => "yellow",
            _ => "#fffde3"
        };

        // 关闭 Domain Reload 时也重置上次会话, 在首场景和组件 Awake 之前建立订阅.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void BeginSession()
        {
            Shutdown();
            Initialize();
        }

        // 只接收 Unity 主线程事件, 不经过命令执行器或界面, 不回写 Unity Console.
        private static void OnUnityLog(string message, string stack, LogType type) => Append(new ConsoleLogRecord(message, message, GetHexColor(type), GetLogType(type), stack));

        // 异常和断言保留原始类型, 筛选分类仍由现有日志缓存决定.
        private static ConsoleLogType GetLogType(LogType type) => type switch
        {
            LogType.Warning => ConsoleLogType.Warning,
            LogType.Error => ConsoleLogType.Error,
            LogType.Exception => ConsoleLogType.Exception,
            LogType.Assert => ConsoleLogType.Assert,
            _ => ConsoleLogType.Log
        };
    }
}
