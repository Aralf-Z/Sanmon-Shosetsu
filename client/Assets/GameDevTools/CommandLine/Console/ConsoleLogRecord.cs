using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> Unity 日志保留实际类型, 工具输出统一为 Dev. </summary>
    public enum ConsoleLogType
    {
        Log,
        Warning,
        Error,
        Exception,
        Assert,
        Dev = 5
    }

    /// <summary> 独立于 UI 的原始日志, 显示文本与用于折叠的正文分别保存. </summary>
    public sealed class ConsoleLogRecord
    {
        public long Id { get; internal set; }

        public DateTime Time { get; }

        public string Message { get; }

        public string DisplayMessage { get; }

        public string Color { get; }

        public ConsoleLogType Type { get; }

        public string StackTrace { get; }

        public bool IsUserInput { get; internal set; }

        public ConsoleLogRecord(string message, string displayMessage, string color, ConsoleLogType type = ConsoleLogType.Dev, string stackTrace = null)
        {
            Time = DateTime.Now;
            Message = message ?? string.Empty;
            DisplayMessage = displayMessage ?? string.Empty;
            Color = color;
            Type = type;
            StackTrace = stackTrace ?? string.Empty;
        }
    }

    /// <summary> 可选的结构化输出适配, 旧渲染器继续使用原文本接口. </summary>
    public interface IStructuredConsoleRenderer
    {
        bool AcceptsStructuredLogs { get; }

        void OutputLog(ConsoleLogRecord record);
    }
}
