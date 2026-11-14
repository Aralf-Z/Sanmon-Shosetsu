using System.Collections.Generic;
using System.Text;

namespace RedSaw.CommandLineInterface
{
    /// <summary> lexer main logic. </summary>
    public partial class Lexer
    {
        private int _index; // Index.

        private string _src; // Src.

        private readonly List<string> _tokenValues = new(); // Token values.

        private readonly List<Token> _tokens = new(); // Tokens.

        private bool HasMore => _index < _src.Length; // Has more.

        /// <summary> parse input string to tokens as LexerResult. input: input string. result: LexerResult. </summary>
        public LexerResult Parse(string input)
        {
            // check if target input is null or empty
            if (input == null || input.Length == 0)
                throw new CommandLexerException("Input string is empty");
            // preprocess to remove all white space, and recheck if target input is empty
            var pp = PreProcess(input);
            if (pp.Length == 0)
                throw new CommandLexerException("Input string is invalid");
            // start to parse
            return Walk(pp);
        }

        // Walk.
        private LexerResult Walk(string input)
        {
            _src = input;
            _index = 0;
            _tokens.Clear();
            _tokenValues.Clear();
            while (HasMore)
            {
                int startIdx = _index;
                char c = _src[_index++];
                switch (c)
                {
                    // skip white space
                    case WHITE_SPACE:
                        break;
                    // stop at the end of file
                    case EOF:
                        _tokens.Add(new Token(TokenType.Eof, -1, startIdx, _index));
                        break;
                    /* 
                        while start with '@', it would be treated as a property which 
                        could be visit directly by '@' in command line
                    */
                    case VAR:
                        string varName = NextInput(_index);
                        _tokens.Add(new Token(TokenType.Var, _tokenValues.Count, startIdx, _index));
                        _tokenValues.Add(varName);
                        break;
                    // string of double quote
                    case DOUBLE_QUOTE:
                    case SINGLE_QUOTE:
                        string str = NextString(_index, c);
                        _tokens.Add(new Token(TokenType.String, _tokenValues.Count, startIdx, _index));
                        _tokenValues.Add(str);
                        break;
                    // :
                    case COLON:
                        _tokens.Add(new Token(TokenType.Colon, -1, startIdx, _index));
                        break;
                    // ,
                    case COMMA:
                        startIdx = _index - 1;
                        _tokens.Add(new Token(TokenType.Comma, -1, startIdx, _index));
                        break;
                    // .
                    case DOT:
                        _tokens.Add(new Token(TokenType.Dot, -1, startIdx, _index));
                        break;
                    // =
                    case ASSIGN:
                        _tokens.Add(new Token(TokenType.Assign, -1, startIdx, _index));
                        break;
                    // (
                    case L_PAREN:
                        _tokens.Add(new Token(TokenType.LParen, -1, startIdx, _index));
                        break;
                    // )
                    case R_PAREN:
                        _tokens.Add(new Token(TokenType.RParen, -1, startIdx, _index));
                        break;
                    // [
                    case L_BRACKET:
                        _tokens.Add(new Token(TokenType.LBracket, -1, startIdx, _index));
                        break;
                    // ]
                    case R_BRACKET:
                        _tokens.Add(new Token(TokenType.RBracket, -1, startIdx, _index));
                        break;
                    // input string
                    default:
                        // number
                        if (c == NEGATIVE || char.IsDigit(c))
                        {
                            string number = NextNumber(_index, out bool isFloat);
                            _tokens.Add(new Token(isFloat ? TokenType.Float : TokenType.Int, _tokenValues.Count, startIdx, _index));
                            _tokenValues.Add(number);
                            break;
                        }

                        var nextInput = NextInput(_index - 1);
                        // keywords
                        switch (nextInput)
                        {
                            case trueKeyword:
                                _tokens.Add(new Token(TokenType.True, -2, startIdx, _index));
                                break;
                            case falseKeyword:
                                _tokens.Add(new Token(TokenType.False, -3, startIdx, _index));
                                break;
                            case nullKeyword:
                                _tokens.Add(new Token(TokenType.Null, -4, startIdx, _index));
                                break;
                            default:
                                _tokens.Add(new Token(TokenType.Input, _tokenValues.Count, startIdx, _index));
                                _tokenValues.Add(nextInput);
                                break;
                        }

                        break;
                }
            }

            return new LexerResult(input, _tokens.ToArray(), _tokenValues.ToArray());
        }

        // Next input.
        private string NextInput(int startIdx)
        {
            while (HasMore)
            {
                char c = _src[_index++];
                if (IsInputTerminator(c))
                {
                    _index--;
                    break;
                }
            }

            return _src[startIdx.._index];
        }

        // Next string.
        private string NextString(int startIdx, char quoteChar)
        {
            var value = new StringBuilder();
            while (HasMore)
            {
                char c = _src[_index++];
                if (c == quoteChar)
                {
                    return value.ToString();
                }
                if (c == '\\' && HasMore)
                {
                    var escaped = _src[_index];
                    if (escaped == quoteChar || escaped == '\\' || escaped == 'n' || escaped == 'r' || escaped == 't')
                    {
                        _index++;
                        c = escaped switch { 'n' => '\n', 'r' => '\r', 't' => '\t', _ => escaped };
                    }
                }
                value.Append(c);
            }

            var nearStr = _src[startIdx.._index];
            throw new CommandLexerException($"String not terminated near <{nearStr}>");
        }

        // Next number.
        private string NextNumber(int startIdx, out bool isFloat)
        {
            while (HasMore)
            {
                char c = _src[_index++];
                if (c == DOT)
                {
                    isFloat = true;
                    return NextFloat(startIdx);
                }

                if (char.IsDigit(c))
                    continue;
                _index--;
                break;
            }

            isFloat = false;
            return _src[(startIdx - 1).._index];
        }

        // Next float.
        private string NextFloat(int startIdx)
        {
            while (HasMore)
            {
                char c = _src[_index++];
                if (char.IsDigit(c))
                    continue;
                if (c == E)
                    return NextExponent(startIdx);
                _index--;
                break;
            }

            return _src[(startIdx - 1).._index];
        }

        // Next exponent.
        private string NextExponent(int startIdx)
        {
            while (HasMore)
            {
                char c = _src[_index++];
                if (char.IsDigit(c))
                    continue;
                _index--;
                break;
            }

            return _src[(startIdx - 1).._index];
        }
    }
}
