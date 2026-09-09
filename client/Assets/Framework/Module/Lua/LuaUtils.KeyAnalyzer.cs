using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Module
{
    public static partial class LuaUtils
    {
        /// <summary>
        /// Lua Script Key Analyzer.
        /// </summary>
        private static class LuaKeyAnalyzer
        {
            public static string[] Analyze(string lua)
            {
                if (string.IsNullOrEmpty(lua))
                    throw new LuaAnalyzeException("Lua script is empty.");

                var lexer = new Lexer(lua);
                var tokens = lexer.Tokenize();

                // 找最后一个 return。
                int returnIndex = -1;

                for (int i = 0; i < tokens.Count; i++)
                {
                    if (tokens[i].Type == TokenType.Return)
                        returnIndex = i;
                }

                if (returnIndex < 0)
                    throw new LuaAnalyzeException(
                        "Cannot find 'return' statement.");

                var parser = new Parser(tokens, lua);
                return parser.ParseReturnTable(returnIndex);
            }

            // ============================================================
            // Lexer
            // ============================================================

            private sealed class Lexer
            {
                private readonly string _source;
                private int _index;
                private int _line = 1;
                private int _column = 1;

                public Lexer(string source)
                {
                    _source = source;
                }

                public List<Token> Tokenize()
                {
                    var result = new List<Token>();

                    while (!IsEnd)
                    {
                        SkipWhiteSpaceAndComments();

                        if (IsEnd)
                            break;

                        int line = _line;
                        int column = _column;

                        char c = Current;

                        // Identifier / keyword
                        if (IsIdentifierStart(c))
                        {
                            string value = ReadIdentifier();
                            result.Add(new Token(
                                GetKeywordType(value),
                                value,
                                line,
                                column));

                            continue;
                        }

                        // Number
                        if (char.IsDigit(c))
                        {
                            string value = ReadNumber();

                            result.Add(new Token(
                                TokenType.Number,
                                value,
                                line,
                                column));

                            continue;
                        }

                        // String
                        if (c == '"' || c == '\'')
                        {
                            string value = ReadString();

                            result.Add(new Token(
                                TokenType.String,
                                value,
                                line,
                                column));

                            continue;
                        }

                        // Long string [[ ... ]]
                        if (c == '[' && Peek(1) == '[')
                        {
                            string value = ReadLongString();

                            result.Add(new Token(
                                TokenType.String,
                                value,
                                line,
                                column));

                            continue;
                        }

                        switch (c)
                        {
                            case '{':
                                result.Add(Simple(TokenType.LeftBrace, "{"));
                                Advance();
                                break;

                            case '}':
                                result.Add(Simple(TokenType.RightBrace, "}"));
                                Advance();
                                break;

                            case '(':
                                result.Add(Simple(TokenType.LeftParen, "("));
                                Advance();
                                break;

                            case ')':
                                result.Add(Simple(TokenType.RightParen, ")"));
                                Advance();
                                break;

                            case '[':
                                result.Add(Simple(TokenType.LeftBracket, "["));
                                Advance();
                                break;

                            case ']':
                                result.Add(Simple(TokenType.RightBracket, "]"));
                                Advance();
                                break;

                            case ',':
                                result.Add(Simple(TokenType.Comma, ","));
                                Advance();
                                break;

                            case ';':
                                result.Add(Simple(TokenType.Semicolon, ";"));
                                Advance();
                                break;

                            case '=':
                                if (Peek(1) == '=')
                                {
                                    result.Add(Simple(TokenType.Operator, "=="));
                                    Advance();
                                    Advance();
                                }
                                else
                                {
                                    result.Add(Simple(TokenType.Assign, "="));
                                    Advance();
                                }

                                break;

                            case '.':
                                if (Peek(1) == '.' && Peek(2) == '.')
                                {
                                    result.Add(Simple(TokenType.Operator, "..."));
                                    Advance();
                                    Advance();
                                    Advance();
                                }
                                else if (Peek(1) == '.')
                                {
                                    result.Add(Simple(TokenType.Operator, ".."));
                                    Advance();
                                    Advance();
                                }
                                else
                                {
                                    result.Add(Simple(TokenType.Operator, "."));
                                    Advance();
                                }

                                break;

                            case ':':
                                if (Peek(1) == ':')
                                {
                                    result.Add(Simple(TokenType.Operator, "::"));
                                    Advance();
                                    Advance();
                                }
                                else
                                {
                                    result.Add(Simple(TokenType.Operator, ":"));
                                    Advance();
                                }

                                break;

                            default:
                                // Lua 运算符。
                                if (IsOperatorChar(c))
                                {
                                    string op = ReadOperator();

                                    result.Add(new Token(
                                        TokenType.Operator,
                                        op,
                                        line,
                                        column));
                                }
                                else
                                {
                                    throw Error(
                                        $"Unexpected character '{c}'.",
                                        line,
                                        column);
                                }

                                break;
                        }
                    }

                    result.Add(new Token(
                        TokenType.Eof,
                        string.Empty,
                        _line,
                        _column));

                    return result;
                }

                private Token Simple(TokenType type, string value)
                {
                    return new Token(type, value, _line, _column);
                }

                private TokenType GetKeywordType(string value)
                {
                    switch (value)
                    {
                        case "return": return TokenType.Return;
                        case "function": return TokenType.Function;
                        case "end": return TokenType.End;
                        case "if": return TokenType.If;
                        case "then": return TokenType.Then;
                        case "elseif": return TokenType.ElseIf;
                        case "else": return TokenType.Else;
                        case "for": return TokenType.For;
                        case "while": return TokenType.While;
                        case "repeat": return TokenType.Repeat;
                        case "until": return TokenType.Until;
                        case "do": return TokenType.Do;
                        case "local": return TokenType.Local;
                        case "and": return TokenType.And;
                        case "or": return TokenType.Or;
                        case "not": return TokenType.Not;
                        case "true": return TokenType.True;
                        case "false": return TokenType.False;
                        case "nil": return TokenType.Nil;
                        case "break": return TokenType.Break;
                        case "goto": return TokenType.Goto;
                        default: return TokenType.Identifier;
                    }
                }

                private string ReadIdentifier()
                {
                    int start = _index;

                    while (!IsEnd && IsIdentifierPart(Current))
                        Advance();

                    return _source.Substring(start, _index - start);
                }

                private string ReadNumber()
                {
                    int start = _index;

                    while (!IsEnd)
                    {
                        char c = Current;

                        if (char.IsLetterOrDigit(c) ||
                            c == '.' ||
                            c == '_')
                        {
                            Advance();
                        }
                        else
                        {
                            break;
                        }
                    }

                    return _source.Substring(start, _index - start);
                }

                private string ReadString()
                {
                    char quote = Current;
                    Advance();

                    var builder = new StringBuilder();

                    while (!IsEnd)
                    {
                        char c = Current;

                        if (c == quote)
                        {
                            Advance();
                            return builder.ToString();
                        }

                        if (c == '\\')
                        {
                            Advance();

                            if (IsEnd)
                                throw Error("Unterminated string.");

                            char escaped = Current;
                            Advance();

                            switch (escaped)
                            {
                                case 'n':
                                    builder.Append('\n');
                                    break;

                                case 'r':
                                    builder.Append('\r');
                                    break;

                                case 't':
                                    builder.Append('\t');
                                    break;

                                case '\\':
                                    builder.Append('\\');
                                    break;

                                case '\'':
                                    builder.Append('\'');
                                    break;

                                case '"':
                                    builder.Append('"');
                                    break;

                                default:
                                    // Lua 支持很多 escape。
                                    // 对 Key 分析而言保留转义后的字符即可。
                                    builder.Append(escaped);
                                    break;
                            }

                            continue;
                        }

                        builder.Append(c);
                        Advance();
                    }

                    throw Error("Unterminated string.");
                }

                private string ReadLongString()
                {
                    // 简化支持 [[ ... ]]
                    Advance();
                    Advance();

                    int start = _index;

                    while (!IsEnd)
                    {
                        if (Current == ']' && Peek(1) == ']')
                        {
                            string value = _source.Substring(
                                start,
                                _index - start);

                            Advance();
                            Advance();

                            return value;
                        }

                        Advance();
                    }

                    throw Error("Unterminated long string.");
                }

                private string ReadOperator()
                {
                    int start = _index;

                    while (!IsEnd && IsOperatorChar(Current))
                        Advance();

                    return _source.Substring(start, _index - start);
                }

                private void SkipWhiteSpaceAndComments()
                {
                    while (!IsEnd)
                    {
                        if (char.IsWhiteSpace(Current))
                        {
                            Advance();
                            continue;
                        }

                        // --
                        if (Current == '-' && Peek(1) == '-')
                        {
                            Advance();
                            Advance();

                            // --[[ long comment ]]
                            if (Current == '[' && Peek(1) == '[')
                            {
                                Advance();
                                Advance();

                                while (!IsEnd)
                                {
                                    if (Current == ']' && Peek(1) == ']')
                                    {
                                        Advance();
                                        Advance();
                                        break;
                                    }

                                    Advance();
                                }
                            }
                            else
                            {
                                // 单行注释
                                while (!IsEnd &&
                                       Current != '\n' &&
                                       Current != '\r')
                                {
                                    Advance();
                                }
                            }

                            continue;
                        }

                        break;
                    }
                }

                private bool IsIdentifierStart(char c)
                {
                    return char.IsLetter(c) || c == '_';
                }

                private bool IsIdentifierPart(char c)
                {
                    return char.IsLetterOrDigit(c) || c == '_';
                }

                private bool IsOperatorChar(char c)
                {
                    return "+-*/%^#<>&|~".IndexOf(c) >= 0;
                }

                private char Peek(int offset)
                {
                    int index = _index + offset;

                    if (index >= _source.Length)
                        return '\0';

                    return _source[index];
                }

                private void Advance()
                {
                    if (IsEnd)
                        return;

                    if (Current == '\n')
                    {
                        _line++;
                        _column = 1;
                    }
                    else
                    {
                        _column++;
                    }

                    _index++;
                }

                private bool IsEnd => _index >= _source.Length;

                private char Current => _source[_index];

                private LuaAnalyzeException Error(
                    string message,
                    int line = -1,
                    int column = -1)
                {
                    return new LuaAnalyzeException(
                        message,
                        line < 0 ? _line : line,
                        column < 0 ? _column : column);
                }
            }

            // ============================================================
            // Parser
            // ============================================================

            private sealed class Parser
            {
                private readonly List<Token> _tokens;
                private readonly string _source;

                private int _index;

                private readonly List<string> _keys =
                    new List<string>();

                public Parser(
                    List<Token> tokens,
                    string source)
                {
                    _tokens = tokens;
                    _source = source;
                }

                public string[] ParseReturnTable(int returnIndex)
                {
                    _index = returnIndex;

                    Expect(TokenType.Return);

                    if (Current.Type != TokenType.LeftBrace)
                    {
                        throw Error(
                            "The value returned by 'return' must be a table '{ ... }'.",
                            Current);
                    }

                    ParseTable();

                    return _keys.ToArray();
                }

                private void ParseTable()
                {
                    Expect(TokenType.LeftBrace);

                    while (Current.Type != TokenType.RightBrace)
                    {
                        if (Current.Type == TokenType.Eof)
                            throw Error(
                                "Unexpected end of file. Expected '}'.",
                                Current);

                        if (Current.Type == TokenType.Comma ||
                            Current.Type == TokenType.Semicolon)
                        {
                            Advance();
                            continue;
                        }

                        ParseField();

                        // field 后面允许 ,
                        // 也允许 ;
                        if (Current.Type == TokenType.Comma ||
                            Current.Type == TokenType.Semicolon)
                        {
                            Advance();
                        }
                        else if (Current.Type != TokenType.RightBrace)
                        {
                            throw Error(
                                "Expected ',', ';' or '}' after table field.",
                                Current);
                        }
                    }

                    Expect(TokenType.RightBrace);
                }

                private void ParseField()
                {
                    // [expression] = value
                    if (Current.Type == TokenType.LeftBracket)
                    {
                        Advance();

                        string key = ParseBracketKey();

                        Expect(TokenType.RightBracket);
                        Expect(TokenType.Assign);

                        AddKey(key);

                        SkipValue();

                        return;
                    }

                    // identifier = value
                    if (Current.Type == TokenType.Identifier)
                    {
                        Token identifier = Current;

                        Advance();

                        if (Current.Type == TokenType.Assign)
                        {
                            Advance();

                            AddKey(identifier.Value);

                            SkipValue();

                            return;
                        }

                        // 没有 "="，说明是数组式 field：
                        //
                        // {
                        //     1,
                        //     "hello",
                        //     foo(),
                        // }
                        //
                        // 这种没有 key，直接跳过。
                        SkipExpressionFrom(identifier);
                        return;
                    }

                    // 其他表达式作为 value。
                    SkipValue();
                }

                private string ParseBracketKey()
                {
                    if (Current.Type == TokenType.String)
                    {
                        string value = Current.Value;
                        Advance();
                        return value;
                    }

                    // [123] = ...
                    if (Current.Type == TokenType.Number)
                    {
                        string value = Current.Value;
                        Advance();
                        return value;
                    }

                    // [SomeVariable] = ...
                    //
                    // 无法静态求值，因此把表达式原样恢复。
                    var builder = new StringBuilder();

                    int depth = 0;

                    while (Current.Type != TokenType.Eof)
                    {
                        if (Current.Type == TokenType.RightBracket &&
                            depth == 0)
                        {
                            break;
                        }

                        if (Current.Type == TokenType.LeftParen ||
                            Current.Type == TokenType.LeftBracket ||
                            Current.Type == TokenType.LeftBrace)
                        {
                            depth++;
                        }
                        else if (Current.Type == TokenType.RightParen ||
                                 Current.Type == TokenType.RightBracket ||
                                 Current.Type == TokenType.RightBrace)
                        {
                            depth--;
                        }

                        if (builder.Length > 0)
                            builder.Append(' ');

                        builder.Append(Current.Value);
                        Advance();
                    }

                    if (Current.Type != TokenType.RightBracket)
                    {
                        throw Error(
                            "Unterminated table key '[ ... ]'.",
                            Current);
                    }

                    return builder.ToString();
                }

                private void SkipValue()
                {
                    int braceDepth = 0;
                    int parenDepth = 0;
                    int bracketDepth = 0;

                    int functionDepth = 0;
                    int blockDepth = 0;

                    while (Current.Type != TokenType.Eof)
                    {
                        Token token = Current;

                        // 当前 table 的 field 已结束。
                        if (braceDepth == 0 &&
                            parenDepth == 0 &&
                            bracketDepth == 0 &&
                            functionDepth == 0 &&
                            blockDepth == 0 &&
                            (token.Type == TokenType.Comma ||
                             token.Type == TokenType.Semicolon ||
                             token.Type == TokenType.RightBrace))
                        {
                            return;
                        }

                        switch (token.Type)
                        {
                            case TokenType.LeftBrace:
                                braceDepth++;
                                break;

                            case TokenType.RightBrace:
                                if (braceDepth > 0)
                                {
                                    braceDepth--;
                                }
                                else
                                {
                                    return;
                                }

                                break;

                            case TokenType.LeftParen:
                                parenDepth++;
                                break;

                            case TokenType.RightParen:
                                if (parenDepth > 0)
                                    parenDepth--;

                                break;

                            case TokenType.LeftBracket:
                                bracketDepth++;
                                break;

                            case TokenType.RightBracket:
                                if (bracketDepth > 0)
                                    bracketDepth--;

                                break;

                            case TokenType.Function:
                                functionDepth++;
                                break;

                            case TokenType.End:
                                if (functionDepth > 0)
                                    functionDepth--;

                                if (blockDepth > 0)
                                    blockDepth--;

                                break;

                            case TokenType.If:
                            case TokenType.For:
                            case TokenType.While:
                            case TokenType.Do:
                                blockDepth++;
                                break;

                            case TokenType.Repeat:
                                blockDepth++;
                                break;

                            case TokenType.Until:
                                if (blockDepth > 0)
                                    blockDepth--;

                                break;
                        }

                        Advance();
                    }

                    if (braceDepth != 0 ||
                        parenDepth != 0 ||
                        bracketDepth != 0 ||
                        functionDepth != 0 ||
                        blockDepth != 0)
                    {
                        throw Error(
                            "Unexpected end of file while parsing table value.",
                            Current);
                    }
                }

                private void SkipExpressionFrom(Token firstToken)
                {
                    // firstToken 已经被消费。
                    //
                    // 后续继续按照 expression 的方式寻找
                    // 当前 field 的结束位置。

                    int braceDepth = 0;
                    int parenDepth = 0;
                    int bracketDepth = 0;

                    int functionDepth = 0;
                    int blockDepth = 0;

                    while (Current.Type != TokenType.Eof)
                    {
                        Token token = Current;

                        if (braceDepth == 0 &&
                            parenDepth == 0 &&
                            bracketDepth == 0 &&
                            functionDepth == 0 &&
                            blockDepth == 0 &&
                            (token.Type == TokenType.Comma ||
                             token.Type == TokenType.Semicolon ||
                             token.Type == TokenType.RightBrace))
                        {
                            return;
                        }

                        switch (token.Type)
                        {
                            case TokenType.LeftBrace:
                                braceDepth++;
                                break;

                            case TokenType.RightBrace:
                                if (braceDepth > 0)
                                    braceDepth--;
                                else
                                    return;
                                break;

                            case TokenType.LeftParen:
                                parenDepth++;
                                break;

                            case TokenType.RightParen:
                                if (parenDepth > 0)
                                    parenDepth--;
                                break;

                            case TokenType.LeftBracket:
                                bracketDepth++;
                                break;

                            case TokenType.RightBracket:
                                if (bracketDepth > 0)
                                    bracketDepth--;
                                break;

                            case TokenType.Function:
                                functionDepth++;
                                break;

                            case TokenType.End:
                                if (functionDepth > 0)
                                    functionDepth--;

                                if (blockDepth > 0)
                                    blockDepth--;

                                break;

                            case TokenType.If:
                            case TokenType.For:
                            case TokenType.While:
                            case TokenType.Do:
                                blockDepth++;
                                break;

                            case TokenType.Repeat:
                                blockDepth++;
                                break;

                            case TokenType.Until:
                                if (blockDepth > 0)
                                    blockDepth--;

                                break;
                        }

                        Advance();
                    }
                }

                private void AddKey(string key)
                {
                    // 默认去重。
                    if (!_keys.Contains(key))
                        _keys.Add(key);
                }

                private void Expect(TokenType type)
                {
                    if (Current.Type != type)
                    {
                        throw Error(
                            $"Expected '{GetTokenDisplay(type)}', " +
                            $"but got '{Current.Value}'.",
                            Current);
                    }

                    Advance();
                }

                private string GetTokenDisplay(TokenType type)
                {
                    switch (type)
                    {
                        case TokenType.LeftBrace: return "{";
                        case TokenType.RightBrace: return "}";
                        case TokenType.LeftParen: return "(";
                        case TokenType.RightParen: return ")";
                        case TokenType.LeftBracket: return "[";
                        case TokenType.RightBracket: return "]";
                        case TokenType.Assign: return "=";
                        case TokenType.Comma: return ",";
                        case TokenType.Semicolon: return ";";
                        case TokenType.Return: return "return";
                        default: return type.ToString();
                    }
                }

                private void Advance()
                {
                    if (_index < _tokens.Count - 1)
                        _index++;
                }

                private Token Current => _tokens[_index];

                private LuaAnalyzeException Error(
                    string message,
                    Token token)
                {
                    return new LuaAnalyzeException(
                        message,
                        token.Line,
                        token.Column,
                        GetLine(token.Line));
                }

                private string GetLine(int line)
                {
                    string[] lines = _source.Replace(
                        "\r\n",
                        "\n").Split('\n');

                    if (line <= 0 || line > lines.Length)
                        return null;

                    return lines[line - 1];
                }
            }

            // ============================================================
            // Token
            // ============================================================

            private enum TokenType
            {
                Eof,

                Identifier,
                Number,
                String,

                LeftBrace,
                RightBrace,

                LeftParen,
                RightParen,

                LeftBracket,
                RightBracket,

                Comma,
                Semicolon,

                Assign,
                Operator,

                Return,
                Function,
                End,

                If,
                Then,
                ElseIf,
                Else,

                For,
                While,
                Repeat,
                Until,
                Do,

                Local,
                And,
                Or,
                Not,

                True,
                False,
                Nil,

                Break,
                Goto,
            }

            private sealed class Token
            {
                public readonly TokenType Type;
                public readonly string Value;
                public readonly int Line;
                public readonly int Column;

                public Token(
                    TokenType type,
                    string value,
                    int line,
                    int column)
                {
                    Type = type;
                    Value = value;
                    Line = line;
                    Column = column;
                }

                public override string ToString()
                {
                    return $"{Type} '{Value}' ({Line}:{Column})";
                }
            }

            // ============================================================
            // Exception
            // ============================================================

            public sealed class LuaAnalyzeException : Exception
            {
                public int Line { get; }
                public int Column { get; }
                public string SourceLine { get; }

                public LuaAnalyzeException(string message, int line = 0, int column = 0, string sourceLine = null) : base(BuildMessage(message, line, column, sourceLine))
                {
                    Line = line;
                    Column = column;
                    SourceLine = sourceLine;
                }

                private static string BuildMessage(string message, int line, int column, string sourceLine)
                {
                    if (line <= 0)
                        return message;

                    var builder = new StringBuilder();

                    builder.Append(message);
                    builder.Append($" (line {line}, column {column})");

                    if (!string.IsNullOrEmpty(sourceLine))
                    {
                        builder.AppendLine();
                        builder.Append("    ");
                        builder.Append(sourceLine);
                        builder.AppendLine();

                        builder.Append("    ");

                        for (int i = 1; i < column; i++)
                            builder.Append(' ');

                        builder.Append('^');
                    }

                    return builder.ToString();
                }
            }
        }
    }
}