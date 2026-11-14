using System;
using System.Text;

namespace GameConsole.Extension
{
    /// <summary> 宏的元数据与逐行正文. </summary>
    public sealed class Macro
    {
        /// <summary> 自定义宏正文最多支持的实际行数, 包含空行和注释. </summary>
        public const int MAX_BODY_LINES = Config.MAX_MACRO_BODY_LINES;

        private static Action<Macro> _runner; // 当前宿主登记的执行入口, 不依赖具体 MonoBehaviour 总控.

        /// <summary> 持久化标识, 自定义宏改名时保持不变. </summary>
        public string Id { get; internal set; }
        /// <summary> 内置宏只读. </summary>
        public bool IsBuiltIn { get; internal set; }
        /// <summary> 宏名称. </summary>
        public string name;
        /// <summary> 宏描述. </summary>
        public string desc;
        /// <summary> 每行一个命令的正文. </summary>
        public string macro;

        /// <summary> 解析名称, 描述和正文组成的完整文本. </summary>
        public Macro(string source)
        {
            name = ToolkitLocalization.Get("ui.Macro.labels");
            desc = string.Empty;
            var body = new StringBuilder();
            var inBody = false;
            foreach (var line in (source ?? string.Empty).Replace("\r\n", "\n").Split('\n'))
            {
                var trimmed = line.Trim();
                if (!inBody && (trimmed.StartsWith("name=", StringComparison.Ordinal) || trimmed.StartsWith("desc=", StringComparison.Ordinal)))
                {
                    var value = MacroText.Unquote(trimmed.Substring(5));
                    if (trimmed.StartsWith("name=", StringComparison.Ordinal)) name = value;
                    else desc = value;
                    continue;
                }
                if (!inBody && trimmed.Length == 0) continue;
                inBody = true;
                if (body.Length > 0) body.Append('\n');
                body.Append(line);
            }
            macro = body.ToString();
        }

        /// <summary> 名称和描述使用统一转义后与正文拼接. </summary>
        public static string Compose(string name, string desc, string body) => $"name={MacroText.Quote(name)}\ndesc={MacroText.Quote(desc)}\n\n{body ?? string.Empty}";

        /// <summary> 返回可保存的完整文本. </summary>
        public string ToSource() => Compose(name, desc, macro);

        /// <summary> 通过当前工具执行, 宏系统持有执行快照. </summary>
        public void Invoke() => _runner?.Invoke(this);

        /// <summary> 宏服务建立时登记通用入口, 返回仅释放该次登记的清理操作. </summary>
        internal static Action BindRunner(Action<Macro> runner)
        {
            _runner = runner;
            return () => { if (ReferenceEquals(_runner, runner)) _runner = null; };
        }
    }
}
