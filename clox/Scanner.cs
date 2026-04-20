namespace clox;

public ref struct Scanner
{
    public ReadOnlySpan<char> Source;
    private int _start;
    private  int _current;
    private int _line;

    public void InitScanner(ReadOnlySpan<char> source)
    {
        Source = source;
        _start = 0;
        _current = 0;
        _line = 1;
    }

    public Token ScanToken()
    {
        SkipWhiteSpace();
        _start = _current;

        if (IsAtEnd())
        {
            return MakeToken(TokenType.EOF);
        }

        char c = Advance();

        if (IsAlpha(c))
        {
            return MakeIdentifier();
        }

        if (IsDigit(c))
        {
            return MakeNumber();
        }

        return c switch
        {
            // single characters
            '(' => MakeToken(TokenType.LEFT_PAREN),
            ')' => MakeToken(TokenType.RIGHT_PAREN),
            '{' => MakeToken(TokenType.LEFT_BRACE),
            '}' => MakeToken(TokenType.RIGHT_BRACE),
            ';' => MakeToken(TokenType.SEMICOLON),
            ',' => MakeToken(TokenType.COMMA),
            '.' => MakeToken(TokenType.DOT),
            '-' => MakeToken(TokenType.MINUS),
            '+' => MakeToken(TokenType.PLUS),
            '/' => MakeToken(TokenType.SLASH),
            '*' => MakeToken(TokenType.STAR),
            // double characters
            '!' => MakeToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG),
            '=' => MakeToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL),
            '<' => MakeToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS),
            '>' => MakeToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER),
            '"' => MakeString(),
            _ => ErrorToken("Unexpected character.".AsSpan())
        };
    }

    private Token MakeIdentifier()
    {
        while (IsAlpha(Peek()) || IsDigit(Peek()))
        {
            Advance();
        }

        return MakeToken(IdentifierType());
    }

    private TokenType IdentifierType()
    {
        // Trie
        switch (Source[_start])
        {
            // ReSharper disable StringLiteralTypo
            case 'a':
                return CheckKeyWord(1, 2, "nd".AsSpan(), TokenType.AND);
            case 'c':
                return CheckKeyWord(1, 4, "lass".AsSpan(), TokenType.CLASS);
            case 'e':
                return CheckKeyWord(1, 3, "lse".AsSpan(), TokenType.ELSE);
            case 'f':
                if (_current - _start > 1)
                {
                    switch (Source[_start + 1])
                    {
                        case 'a': return CheckKeyWord(2, 3, "lse".AsSpan(), TokenType.FALSE);
                        case 'o': return CheckKeyWord(2, 1, "r".AsSpan(), TokenType.FOR);
                        case 'u': return CheckKeyWord(2, 1, "n".AsSpan(), TokenType.FUN);
                    }
                }
                break;
            case 'i':
                return CheckKeyWord(1, 1, "f".AsSpan(), TokenType.IF);
            case 'n':
                return CheckKeyWord(1, 2, "il".AsSpan(), TokenType.NIL);
            case 'o':
                return CheckKeyWord(1, 1, "r".AsSpan(), TokenType.OR);
            case 'p':
                return CheckKeyWord(1, 4, "rint".AsSpan(), TokenType.PRINT);
            case 'r':
                return CheckKeyWord(1, 5, "eturn".AsSpan(), TokenType.RETURN);
            case 's':
                return CheckKeyWord(1, 4, "uper".AsSpan(), TokenType.SUPER);
            case 't':
                if (_current - _start > 1)
                {
                    switch (Source[_start + 1])
                    {
                        case 'h': return CheckKeyWord(2, 2, "is".AsSpan(), TokenType.THIS);
                        case 'r': return CheckKeyWord(2, 2, "ue".AsSpan(), TokenType.TRUE);
                    }
                }
                break;
            case 'v':
                return CheckKeyWord(1, 2, "ar".AsSpan(), TokenType.VAR);
            case 'w':
                return CheckKeyWord(1, 4, "hile".AsSpan(), TokenType.WHILE);
        }
        // ReSharper enable StringLiteralTypo
        return TokenType.IDENTIFIER;
    }

    private TokenType CheckKeyWord(int start, int length, ReadOnlySpan<char> rest, TokenType type)
    {
        if (_current - _start == start + length &&
            rest.Equals(Source[(_start + start)..(_start + start + length)], StringComparison.Ordinal))
        {
            return type;
        }

        return TokenType.IDENTIFIER;
    }

    private bool IsAlpha(char c)
    {
        return c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';
    }

    private Token MakeNumber()
    {
        while (IsDigit(Peek()))
        {
            Advance();
        }
        // look for fractional part
        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            // Consume '.'
            Advance();
            while (IsDigit(Peek()))
            {
                Advance();
            }
        }

        return MakeToken(TokenType.NUMBER);
    }

    private static bool IsDigit(char c)
    {
        return c is >= '0' and <= '9';
    }

    private Token MakeString()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n')
            {
                _line++;
            }

            Advance();
        }

        if (IsAtEnd())
        {
            return ErrorToken("Unterminated string.");
        }
        // The closing quote
        Advance();
        return MakeToken(TokenType.STRING);
    }

    /// <summary>
    /// Comments are also treated as Whitespace
    /// </summary>
    private void SkipWhiteSpace()
    {
        while (true)
        {
            if (_current == Source.Length)
            {
                return;
            }
            var c = Peek();
            switch (c)
            {
                case '\0':
                    return;
                case ' ':
                case '\r':
                case '\t':
                    Advance();
                    break;
                case '\n':
                    _line++;
                    Advance();
                    break;
                // Skim comments as well
                case '/':
                    if (PeekNext() == '/')
                    {
                        while (Peek() != '\n' && !IsAtEnd())
                        {
                            Advance();
                        }
                    }
                    else
                    {
                        return;
                    }
                    break;
                default:
                    return;
            }
        }
    }

    private char PeekNext()
    {
        return IsAtEnd() ? '\0' : Source[_current + 1];
    }

    private char Peek()
    {
        return Source[_current];
    }

    private char Advance()
    {
        _current++;
        return Source[_current - 1];
    }

    private Token ErrorToken(ReadOnlySpan<char> message)
    {
        Console.Error.WriteLine(message);
        return new Token()
        {
            Start = _start,
            Length = _current - _start,
            Type = TokenType.ERROR,
            Line = _line
        };
    }

    private Token MakeToken(TokenType type)
    {
        return new Token()
        {
            Start = _start,
            Length = _current - _start,
            Type = type,
            Line = _line
        };
    }

    private bool IsAtEnd()
    {
        return _current == Source.Length;
    }

    private bool Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (Source[_current] != expected) return false;
        _current++;
        return true;
    }
}