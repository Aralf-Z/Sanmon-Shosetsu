namespace RedSaw.CommandLineInterface
{
    /// <summary> 命令词法分析器. </summary>
    public partial class Lexer
    {
        // SPECIAL CHARACTERS
        /// <summary> 换行字符. </summary>
        public const char EOL = '\n';

        /// <summary> 输入结束标记. </summary>
        public const char EOF = '\0';

        /// <summary> 制表字符. </summary>
        public const char TAB = '\t';

        /// <summary> 回车字符. </summary>
        public const char CR = '\r';

        /// <summary> White space. </summary>
        public const char WHITE_SPACE = ' ';

        // SYMBOLS
        /// <summary> 命令变量的前缀字符. </summary>
        public const char VAR = '@';

        /// <summary> 成员访问分隔符. </summary>
        public const char DOT = '.';

        /// <summary> 类型转换分隔符. </summary>
        public const char COLON = ':';

        /// <summary> 参数分隔符. </summary>
        public const char COMMA = ',';

        /// <summary> 赋值运算符. </summary>
        public const char ASSIGN = '=';

        /// <summary> 参数列表的左圆括号. </summary>
        public const char L_PAREN = '(';

        /// <summary> 参数列表的右圆括号. </summary>
        public const char R_PAREN = ')';

        /// <summary> 索引访问的左方括号. </summary>
        public const char L_BRACKET = '[';

        /// <summary> 索引访问的右方括号. </summary>
        public const char R_BRACKET = ']';

        /// <summary> Single quote. </summary>
        public const char SINGLE_QUOTE = '\'';

        /// <summary> Double quote. </summary>
        public const char DOUBLE_QUOTE = '"';

        /// <summary> Underline. </summary>
        public const char UNDERLINE = '_';

        /// <summary> Negative. </summary>
        public const char NEGATIVE = '-';

        /// <summary> 浮点数指数部分的前缀字符. </summary>
        public const char E = 'e';

        // KEYWORDS
        /// <summary> True keyword. </summary>
        public const string trueKeyword = "true";

        /// <summary> False keyword. </summary>
        public const string falseKeyword = "false";

        /// <summary> Null keyword. </summary>
        public const string nullKeyword = "null";

        /// <summary> use to preprocess the source input, the source input could only contains &apos;EOF&apos; and &apos;WHITE_SPACE&apos;. </summary>
        public static char[] whiteSpaceChars =
        {
            EOL,
            EOF,
            TAB,
            CR
        };

        /// <summary> Symbols. </summary>
        public static char[] symbols =
        {
            VAR,
            DOT,
            COLON,
            COMMA,
            ASSIGN,
            L_PAREN,
            R_PAREN,
            L_BRACKET,
            R_BRACKET,
            SINGLE_QUOTE,
            DOUBLE_QUOTE
        };

        /// <summary> Id terminators. </summary>
        public static char[] idTerminators =
        {
            WHITE_SPACE,
            EOF,
            DOT,
            COLON,
            COMMA,
            ASSIGN,
            L_PAREN,
            R_PAREN,
            L_BRACKET,
            R_BRACKET,
            SINGLE_QUOTE,
            DOUBLE_QUOTE
        };

        /// <summary> check if a string value is an id which starts with &apos;_&apos; or letter and contains only &apos;_&apos; or letter or digit. </summary>
        public static bool IsId(string value)
        {
            if (value == null || value.Length == 0)
                return false;
            char head = value[0];
            if (head == UNDERLINE || char.IsLetter(head))
            {
                for (int i = 1; i < value.Length; i++)
                {
                    char c = value[i];
                    if (c == UNDERLINE || char.IsLetterOrDigit(c))
                        continue;
                }

                return true;
            }

            return false;
        }

        /// <summary> Is white space. </summary>
        public static bool IsWhiteSpace(char c)
        {
            foreach (char cc in whiteSpaceChars)
            {
                if (c == cc)
                    return true;
            }

            return char.IsWhiteSpace(c);
        }

        // Is input terminator.
        private bool IsInputTerminator(char c)
        {
            foreach (char t in idTerminators)
                if (c == t)
                    return true;
            return false;
        }

        // Pre process.
        private 
        /// <summary>remove all white space in target source input</summary>
        string PreProcess(string input)
        {
            var result = new System.Text.StringBuilder();
            var quote = '\0';
            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];
                if (quote != '\0')
                {
                    result.Append(c);
                    if (c == '\\' && i + 1 < input.Length) result.Append(input[++i]);
                    else if (c == quote) quote = '\0';
                }
                else
                {
                    if (c == SINGLE_QUOTE || c == DOUBLE_QUOTE) quote = c;
                    result.Append(char.IsWhiteSpace(c) ? WHITE_SPACE : c);
                }
            }
            return result.Append(EOF).ToString();
        }
    }
}
