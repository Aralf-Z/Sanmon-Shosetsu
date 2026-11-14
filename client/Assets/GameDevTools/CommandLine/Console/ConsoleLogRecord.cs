using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> Unity 日志保留实际类型, 工具输出统一为 Dev. </summary>
    public enum ConsoleLogType { Log, Warning, Error, Exception, Assert, Dev = 5 }

    /// <summary> 独立于 UI 的原始日志, 显示文本与用于折叠的正文分别保存. </summary>
    public sealed class ConsoleLogRecord
    {
        /// <summary> 所属缓存分配的稳定编号. </summary>
        public long Id { get; internal set; }
        /// <summary> 实际接收时间. </summary>
        public DateTime Time { get; }
        /// <summary> 不含控制台自动时间戳的正文. </summary>
        public string Message { get; }
        /// <summary> 沿用控制台时间戳设置的显示文本. </summary>
        public string DisplayMessage { get; }
        /// <summary> 原始显示颜色. </summary>
        public string Color { get; }
        /// <summary> 接收端提供的日志类型. </summary>
        public ConsoleLogType Type { get; }
        /// <summary> 接收端提供的调用栈或说明, 构造时不按类型覆盖. </summary>
        public string StackTrace { get; }
        /// <summary> 提交回显的说明是界面占位, 与真实堆栈区别处理. </summary>
        public bool IsUserInput { get; internal set; }

        /// <summary> 创建日志快照, 缓存接收时再分配编号. </summary>
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
        /// <summary> 当前模式是否接收结构化日志. </summary>
        bool AcceptsStructuredLogs { get; }
        /// <summary> 接收已格式化时间戳的单条日志, 不执行任何命令. </summary>
        void OutputLog(ConsoleLogRecord record);
    }
}
