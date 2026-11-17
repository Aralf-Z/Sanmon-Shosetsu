using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> lexer result, contains a group of LexerResult. </summary>
    public readonly struct LexerResult
    {
        public readonly string source;

        public readonly Token[] tokens;

        public readonly string[] sourceBuffer;

        public readonly Token this[int index] => tokens[index];

        public readonly Token LastToken => tokens[^1];

        public LexerResult(string source, Token[] tokens, string[] sourceBuffer)
        {
            this.source = source;
            this.tokens = tokens;
            this.sourceBuffer = sourceBuffer;
        }

        public readonly void Debug(Action<string> log)
        {
            foreach (var tk in tokens)
            {
                var src = tk.sourceIdx >= 0
                    ? sourceBuffer[tk.sourceIdx]
                    : string.Empty;

                log($"<{tk.type}> {src}");
            }
        }

        public readonly string NearInfo(Token token, int radius = 7)
        {
            int left = Math.Max(0, token.startIdx - radius);
            int right = Math.Min(token.endIdx + radius, source.Length - 1);

            return source[left..right];
        }

        public readonly string NearInfo(int lb, int rb)
        {
            int left = Math.Max(0, lb);
            int right = Math.Min(rb, source.Length - 1);

            return source[left..right];
        }
    }
}
