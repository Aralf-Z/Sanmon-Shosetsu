using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace GameConsole.Extension
{
    /// <summary> 宏参数与文本的无副作用处理. </summary>
    internal static class MacroText
    {
        /// <summary> 生成控制台兼容的字符串字面量. </summary>
        internal static string Quote(string value) => "\"" + (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t") + "\"";

        /// <summary> 解码显式引号, 未加引号的参数保持原文. </summary>
        internal static string Unquote(string value)
        {
            value = (value ?? string.Empty).Trim();
            if (value.Length < 2 || (value[0] != '"' && value[0] != '\'') || value[value.Length - 1] != value[0]) return value;
            var result = new StringBuilder();
            for (var i = 1; i < value.Length - 1; i++)
            {
                var c = value[i];
                if (c == '\\' && i + 1 < value.Length - 1)
                {
                    var next = value[++i];
                    c = next switch { 'n' => '\n', 'r' => '\r', 't' => '\t', _ => next };
                }
                result.Append(c);
            }
            return result.ToString();
        }

        /// <summary> 检查成对符号并拆分不在引号和括号中的分隔符. </summary>
        internal static List<string> Split(string text, char separator)
        {
            var parts = new List<string>();
            if (string.IsNullOrWhiteSpace(text)) return parts;
            var quote = '\0';
            var brackets = new Stack<char>();
            var start = 0;
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (quote != '\0')
                {
                    if (c == '\\') { i++; continue; }
                    if (c == quote) quote = '\0';
                    continue;
                }
                if (c == '"' || c == '\'') { quote = c; continue; }
                if (c == '(' || c == '[') brackets.Push(c);
                else if (c == ')' || c == ']')
                {
                    if (brackets.Count == 0 || brackets.Pop() != (c == ')' ? '(' : '[')) throw new FormatException(ToolkitLocalization.Get("ui.MacroText.Split"));
                }
                if (c == separator && brackets.Count == 0)
                {
                    parts.Add(text.Substring(start, i - start).Trim());
                    start = i + 1;
                }
            }
            if (quote != '\0') throw new FormatException(ToolkitLocalization.Get("ui.MacroText.Split.2"));
            if (brackets.Count != 0) throw new FormatException(ToolkitLocalization.Get("ui.MacroText.Split.3"));
            parts.Add(text.Substring(start).Trim());
            return parts;
        }

        /// <summary> 自定义类型解析器留到运行时处理, 避免校验触发用户代码. </summary>
        internal static bool IsSimple(Type type) => type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) || Nullable.GetUnderlyingType(type) != null;

        /// <summary> 按目标参数类型检查静态字面量. </summary>
        internal static object ConvertLiteral(string text, Type type, bool isNull = false)
        {
            if (isNull)
            {
                if (!type.IsValueType || Nullable.GetUnderlyingType(type) != null) return null;
                throw new FormatException(ToolkitLocalization.Format("ui.MacroText.ConvertLiteral", ("p0", (object)(type.Name))));
            }
            type = Nullable.GetUnderlyingType(type) ?? type;
            if (type == typeof(string)) return text;
            if (type.IsEnum) return Enum.Parse(type, text, true);
            return Convert.ChangeType(text, type, CultureInfo.InvariantCulture);
        }

        /// <summary> 返回字符串外的注释起点, 未找到时返回文本长度. </summary>
        internal static int CommentIndex(string text)
        {
            var quote = '\0';
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (quote != '\0')
                {
                    if (c == '\\') i++;
                    else if (c == quote) quote = '\0';
                }
                else if (c == '"' || c == '\'') quote = c;
                else if (c == '/' && i + 1 < text.Length && text[i + 1] == '/') return i;
            }
            return text.Length;
        }

        /// <summary> 移除行尾注释, 供编译与展示共用. </summary>
        internal static string Code(string text) => text.Substring(0, CommentIndex(text)).Trim();

        /// <summary> 仅识别行首调用名称, 不将字符串或注释中的关键字着色. </summary>
        internal static string Keyword(string text)
        {
            var code = Code(text);
            var bracket = code.IndexOf('(');
            return bracket < 0 ? code : code.Substring(0, bracket).TrimEnd();
        }

        /// <summary> 整理一行空白, 引号中的内容和注释含义保持不变. </summary>
        internal static string FormatLine(string text)
        {
            var commentIndex = CommentIndex(text);
            var comment = text.Substring(commentIndex);
            text = text.Substring(0, commentIndex);
            Split(text, '\0');
            var result = new StringBuilder();
            var quote = '\0';
            var pendingSpace = false;
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (quote != '\0')
                {
                    result.Append(c);
                    if (c == '\\' && i + 1 < text.Length) result.Append(text[++i]);
                    else if (c == quote) quote = '\0';
                    continue;
                }
                if (char.IsWhiteSpace(c)) { pendingSpace = true; continue; }
                if (pendingSpace && result.Length > 0 && result[result.Length - 1] != '(' && result[result.Length - 1] != ' ' && c != ')' && c != ',' && c != '(') result.Append(' ');
                pendingSpace = false;
                result.Append(c);
                if (c == ',') result.Append(' ');
                if (c == '"' || c == '\'') quote = c;
            }
            var code = result.ToString().Trim();
            return code + (code.Length > 0 && comment.Length > 0 ? " " : string.Empty) + comment;
        }
    }
}
