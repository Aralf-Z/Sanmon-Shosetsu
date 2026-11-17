namespace RedSaw.CommandLineInterface
{
    /// <summary> 命令词法分析器. </summary>
    public partial class Lexer
    {
        public const char EOL = '\n';

        public const char EOF = '\0';

        public const char TAB = '\t';

        public const char CR = '\r';

        public const char WHITE_SPACE = ' ';

        public const char VAR = '@';

        public const char DOT = '.';

        public const char COLON = ':';

        public const char COMMA = ',';

        public const char ASSIGN = '=';

        public const char L_PAREN = '(';

        public const char R_PAREN = ')';

        public const char L_BRACKET = '[';

        public const char R_BRACKET = ']';

        public const char SINGLE_QUOTE = '\'';

        public const char DOUBLE_QUOTE = '"';

        public const char UNDERLINE = '_';

        public const char NEGATIVE = '-';

        public const char E = 'e';

        public const string trueKeyword = "true";

        public const string falseKeyword = "false";

        /// <summary> Null keyword. </summary>
        public const string nullKeyword = "null";

        /// <summary> use to preprocess the source input, the source input could only contains &apos;EOF&apos; and &apos;WHITE_SPACE&apos;. </summary>
        public static char[] whiteSpaceChars =
        { EOL, EOF, TAB, CR };

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
            if (value == null || value.Length == 0) return false;

            char head = value[0];

            if (head == UNDERLINE || char.IsLetter(head))
            {
                for (int i = 1; i < value.Length; i++)
                {
                    char c = value[i];

                    if (c == UNDERLINE || char.IsLetterOrDigit(c))
                    {
                        continue;
                    }
                }

                return true;
            }

            return false;
        }

        public static bool IsWhiteSpace(char c)
        {
            foreach (char cc in whiteSpaceChars)
            {
                if (c == cc) return true;
            }

            return char.IsWhiteSpace(c);
        }

        private bool IsInputTerminator(char c)
        {
            foreach (char t in idTerminators)
            {
                if (c == t) return true;
            }

            return false;
        }

        private string PreProcess(string input)
        {
            var result = new System.Text.StringBuilder();
            var quote = '\0';

            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];

                if (quote != '\0')
                {
                    result.Append(c);
                    if (c == '\\' && i + 1 < input.Length)
                    {
                        result.Append(input[++i]);
                    }
                    else if (c == quote)
                    {
                        quote = '\0';
                    }
                }
                else
                {
                    if (c == SINGLE_QUOTE || c == DOUBLE_QUOTE)
                    {
                        quote = c;
                    }

                    result.Append(char.IsWhiteSpace(c) ? WHITE_SPACE : c);
                }
            }

            return result.Append(EOF)
                .ToString();
        }
    }
}
