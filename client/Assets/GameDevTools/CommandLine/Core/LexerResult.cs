using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> lexer result, contains a group of LexerResult. </summary>
    public readonly struct LexerResult
    {
        /// <summary> Source. </summary>
        public readonly string source;

        /// <summary> Tokens. </summary>
        public readonly Token[] tokens;

        /// <summary> Source buffer. </summary>
        public readonly string[] sourceBuffer;

        /// <summary> Value at the specified index. </summary>
        public readonly Token this[int index] => tokens[index];

        /// <summary> Last token. </summary>
        public readonly Token LastToken => tokens[^1];

        /// <summary> Create a LexerResult instance. </summary>
        public LexerResult(string source, Token[] tokens, string[] sourceBuffer)
        {
            this.source = source;
            this.tokens = tokens;
            this.sourceBuffer = sourceBuffer;
        }

        /// <summary> Debug. </summary>
        public readonly void Debug(Action<string> log)
        {
            foreach (var tk in tokens)
            {
                var src = tk.sourceIdx >= 0 ? sourceBuffer[tk.sourceIdx] : string.Empty;
                log($"<{tk.type}> {src}");
            }
        }

        /// <summary> Near info. </summary>
        public readonly string NearInfo(Token token, int radius = 7)
        {
            int left = Math.Max(0, token.startIdx - radius);
            int right = Math.Min(token.endIdx + radius, source.Length - 1);
            return source[left..right];
        }

        /// <summary> Near info. </summary>
        public readonly string NearInfo(int lb, int rb)
        {
            int left = Math.Max(0, lb);
            int right = Math.Min(rb, source.Length - 1);
            return source[left..right];
        }
    }
}
