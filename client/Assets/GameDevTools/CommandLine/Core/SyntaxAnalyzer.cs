using System.Collections.Generic;

namespace RedSaw.CommandLineInterface
{
    /// <summary> analyze lexer result and generate syntax tree. </summary>
    public class SyntaxAnalyzer
    {
        public bool HasMore => _index < Tokens.Length;

        public Token CurrentToken => Tokens[_index];

        public Token NextToken => Tokens[_index++];

        private LexerResult _lexerResult;

        private int _index;

        private bool _commandLatch;

        private Token[] Tokens => _lexerResult.tokens;

        private string[] TokenValues => _lexerResult.sourceBuffer;

        public SyntaxTree Analyze(LexerResult lexerResult)
        {
            this._lexerResult = lexerResult;
            _index = 0;
            _commandLatch = false;
            return NextExpr(true);
        }

        public SyntaxTree NextExpr(bool isRoot = false)
        {
            SyntaxTree access = NextAccess();

            if (!HasMore || PeekMatch(TokenType.Eof)) return access;

            if (Match(TokenType.Assign))
            {
                if (access.opcode == SyntaxTreeCode.Dot)
                {
                    SyntaxTree expr = NextExpr();

                    return new SyntaxTree(SyntaxTreeCode.SetField, string.Empty, new SyntaxTree[] { access.children[0], access.children[1], expr });
                }

                // instance["test"] = 10 or instance[10] = 10
                if (access.opcode == SyntaxTreeCode.Index)
                {
                    SyntaxTree expr = NextExpr();

                    return new SyntaxTree(SyntaxTreeCode.SetElement, string.Empty, new SyntaxTree[] { access.children[0], access.children[1], expr });
                }

                return new SyntaxTree(SyntaxTreeCode.Assign, string.Empty, access, NextExpr());
            }

            if (isRoot)
            {
                throw new CommandSyntaxException($"Unexpected token {CurrentToken.type} near \"..{_lexerResult.NearInfo(CurrentToken)}\"");
            }

            return access;
        }

        private bool Match(TokenType tokenType)
        {
            if (HasMore && CurrentToken.type == tokenType)
            {
                _index++;
                return true;
            }

            return false;
        }

        // only check if current token type is matched, don't move
        private bool PeekMatch(TokenType tokenType)
        {
            return HasMore && CurrentToken.type == tokenType;
        }

        private bool MatchId(out SyntaxTree factor)
        {
            var token = CurrentToken;

            if (token.type == TokenType.Input && Lexer.IsId(TokenValues[token.sourceIdx]))
            {
                factor = new SyntaxTree(SyntaxTreeCode.FactorId, TokenValues[token.sourceIdx]);
                _index++;
                return true;
            }

            factor = null;
            return false;
        }

        private SyntaxTree NextParameterList()
        {
            if (Match(TokenType.RParen)) return new SyntaxTree(SyntaxTreeCode.FactorParams, string.Empty);

            _commandLatch = true;
            List<SyntaxTree> parameters = new();

            while (HasMore)
            {
                parameters.Add(NextExpr());
                if (Match(TokenType.RParen))
                {
                    break;
                }

                if (Match(TokenType.Comma))
                {
                    continue;
                }
            }

            _commandLatch = false;
            return new SyntaxTree(SyntaxTreeCode.FactorParams, string.Empty, parameters.ToArray());
        }

        private SyntaxTree NextparameterlistCommand()
        {
            _commandLatch = true;
            List<SyntaxTree> parameters = new();

            while (HasMore)
            {
                if (Match(TokenType.Eof))
                {
                    break;
                }

                parameters.Add(NextExpr());
            }

            _commandLatch = false;
            return new SyntaxTree(SyntaxTreeCode.FactorParams, string.Empty, parameters.ToArray());
        }

        private SyntaxTree NextFactor()
        {
            Token tk = NextToken;

            switch (tk.type)
            {
                case TokenType.Int:
                    return new SyntaxTree(SyntaxTreeCode.FactorInt, TokenValues[tk.sourceIdx]);
                case TokenType.Float:
                    return new SyntaxTree(SyntaxTreeCode.FactorFloat, TokenValues[tk.sourceIdx]);
                case TokenType.String:
                    return new SyntaxTree(SyntaxTreeCode.FactorString, TokenValues[tk.sourceIdx]);
                case TokenType.Null:
                    return new SyntaxTree(SyntaxTreeCode.FactorNull, Lexer.nullKeyword);
                case TokenType.True:
                    return new SyntaxTree(SyntaxTreeCode.FactorTrue, Lexer.trueKeyword);
                case TokenType.False:
                    return new SyntaxTree(SyntaxTreeCode.FactorFalse, Lexer.falseKeyword);
                case TokenType.Var:
                    return new SyntaxTree(SyntaxTreeCode.Loadvar, TokenValues[tk.sourceIdx]);
                case TokenType.Input:
                    // all other input would be treated as command, if next token is '(', then treat as function call, otherwise treat as command
                    if (Match(TokenType.LParen))
                    {
                        var functionCmd = new SyntaxTree(SyntaxTreeCode.FactorInput, TokenValues[tk.sourceIdx]);

                        return new SyntaxTree(SyntaxTreeCode.Call, string.Empty, functionCmd, NextParameterList());
                    }

                    if (_commandLatch) return new SyntaxTree(SyntaxTreeCode.FactorInput, TokenValues[tk.sourceIdx]);
                    var commandCall = new SyntaxTree(SyntaxTreeCode.FactorInput, TokenValues[tk.sourceIdx]);
                    return new SyntaxTree(SyntaxTreeCode.Call, string.Empty, commandCall, NextparameterlistCommand());
                case TokenType.LParen:
                    SyntaxTree expr = NextExpr();
                    if (!Match(TokenType.RParen))
                    {
                        throw new CommandSyntaxException($"Missing ')' near \"..{_lexerResult.NearInfo(tk)}\"");
                    }

                    return expr;
            }

            throw new CommandSyntaxException($"Unexpected token {tk.type} near \"..{_lexerResult.NearInfo(tk)}\"");
        }

        private SyntaxTree NextConverter()
        {
            var factor = NextFactor();

            if (Match(TokenType.Colon))
            {
                var currentTk = NextToken;

                if (currentTk.type == TokenType.String || currentTk.type == TokenType.Input)
                {
                    var opFactor = new SyntaxTree(SyntaxTreeCode.FactorInput, TokenValues[currentTk.sourceIdx]);
                    var opConverter = new SyntaxTree(SyntaxTreeCode.Cvt, string.Empty, factor, opFactor);

                    return opConverter;
                }

                throw new CommandSyntaxException($"Unexpected token near \"..{_lexerResult.NearInfo(currentTk)}\"");
            }

            return factor;
        }

        private SyntaxTree NextAccess()
        {
            SyntaxTree tmp = NextConverter();

            while (HasMore)
            {
                int startPosition = CurrentToken.startIdx;

                if (Match(TokenType.Dot))
                {
                    if (MatchId(out var id))
                    {
                        if (Match(TokenType.LParen))
                        {
                            var callable = new SyntaxTree(SyntaxTreeCode.Dot, string.Empty, tmp, id);

                            tmp = new SyntaxTree(SyntaxTreeCode.Call, string.Empty, callable, NextParameterList());
                            continue;
                        }

                        tmp = new SyntaxTree(SyntaxTreeCode.Dot, string.Empty, tmp, id);
                        continue;
                    }

                    throw new CommandSyntaxException($"Unexpected token near \"..{_lexerResult.NearInfo(startPosition, CurrentToken.endIdx)}\"");
                }

                if (Match(TokenType.LBracket))
                {
                    SyntaxTree expr = NextExpr();

                    if (!Match(TokenType.RBracket))
                    {
                        throw new CommandSyntaxException($"Missing ']' near \"..{_lexerResult.NearInfo(startPosition, CurrentToken.endIdx)}\"");
                    }

                    tmp = new SyntaxTree(SyntaxTreeCode.Index, string.Empty, tmp, expr);
                    continue;
                }

                break;
            }

            return tmp;
        }
    }
}
