using System;
using System.Text;

namespace GameConsole.Extension
{
    /// <summary> 宏的元数据与逐行正文. </summary>
    public sealed class Macro
    {
        public const int MAX_BODY_LINES = Config.MAX_MACRO_BODY_LINES;

        private static Action<Macro> _runner;

        public string Id { get; internal set; }

        public bool IsBuiltIn { get; internal set; }

        public string name;

        public string desc;

        public string macro;

        public Macro(string source)
        {
            name = ToolkitLocalization.Get("ui.Macro.labels");
            desc = string.Empty;
            var body = new StringBuilder();
            var inBody = false;

            foreach (var line in (source ?? string.Empty).Replace("\r\n", "\n")
                .Split('\n'))
            {
                var trimmed = line.Trim();

                if (!inBody && (trimmed.StartsWith("name=", StringComparison.Ordinal) || trimmed.StartsWith("desc=", StringComparison.Ordinal)))
                {
                    var value = MacroText.Unquote(trimmed.Substring(5));

                    if (trimmed.StartsWith("name=", StringComparison.Ordinal))
                    {
                        name = value;
                    }
                    else
                    {
                        desc = value;
                    }

                    continue;
                }

                if (!inBody && trimmed.Length == 0)
                {
                    continue;
                }

                inBody = true;
                if (body.Length > 0)
                {
                    body.Append('\n');
                }

                body.Append(line);
            }

            macro = body.ToString();
        }

        public static string Compose(string name, string desc, string body) => $"name={MacroText.Quote(name)}\ndesc={MacroText.Quote(desc)}\n\n{body ?? string.Empty}";

        public string ToSource() => Compose(name, desc, macro);

        public void Invoke() => _runner?.Invoke(this);

        /// <summary> 宏服务建立时登记通用入口, 返回仅释放该次登记的清理操作. </summary>
        internal static Action BindRunner(Action<Macro> runner)
        {
            _runner = runner;
            return () =>
            {
                if (ReferenceEquals(_runner, runner))
                {
                    _runner = null;
                }
            };
        }
    }
}
