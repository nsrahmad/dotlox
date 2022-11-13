using static dotlox.TokenType;

namespace dotlox;

internal class Scanner
{
    private readonly string _source;
    private int _start;
    private int _current;
    private int _line = 1;
    private readonly List<Token> _tokens = new();

    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        {"and", AND},
        {"class", CLASS},
        {"else", ELSE},
        {"false", FALSE},
        {"for", FOR},
        {"fun", FUN},
        {"if", IF},
        {"nil", NIL},
        {"or", OR},
        {"print", PRINT},
        {"return", RETURN},
        {"super", SUPER},
        {"this", THIS},
        {"true", TRUE},
        {"var", VAR},
        {"while", WHILE}
    };
    
    public Scanner(string source)
    {
        _source = source;
    }

    internal List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            _start = _current;
            ScanToken();
        }
        _tokens.Add(new Token(EOF, "", default!, _line));
        return _tokens;
    }

    private void ScanToken()
    {
        var c = Advance();
        switch (c)
        {
            case '(': AddToken(LEFT_PAREN); break;
            case ')': AddToken(RIGHT_PAREN); break;
            case '{': AddToken(LEFT_BRACE); break;
            case '}': AddToken(RIGHT_BRACE); break;
            case ',': AddToken(COMMA); break;
            case '.': AddToken(DOT); break;
            case '-': AddToken(MINUS); break;
            case '+': AddToken(PLUS); break;
            case ';': AddToken(SEMICOLON); break;
            case '*': AddToken(STAR); break;
            case '!': AddToken(Match('=') ? BANG_EQUAL : BANG); break;
            case '=': AddToken(Match('=') ? EQUAL_EQUAL : EQUAL); break;
            case '<': AddToken(Match('=') ? LESS_EQUAL : LESS); break;
            case '>': AddToken(Match('=') ? GREATER_EQUAL : GREATER); break;
            
            // / is special because of comments
            case '/':
                if (Match('/'))
                {
                    while (Peek() != '\n' && !IsAtEnd()) Advance();
                }
                else
                {
                    AddToken(SLASH);
                }
                break;
            case ' ':
            case '\r':
            case '\t':
                break;
            case '\n':
                _line++;
                break;
            case '"': HandleString();

                void HandleString()
                {
                    while (Peek() != '"' && !IsAtEnd())
                    {
                        if (Peek() == '\n') _line++;
                        Advance();
                    }

                    if (IsAtEnd())
                    {
                        Program.Error(_line, "Unterminated string.");
                        return;
                    }
                    // The closing ".
                    Advance();
                        
                    var value = _source[(_start+1)..(_current - 1)];
                    AddToken(STRING, value);
                }

                break;
            default:
                if (IsDigit(c))
                {
                    HandleNumber();
                } 
                else if (IsAlpha(c))
                {
                    Identifier();
                }
                else
                {
                    Program.Error(_line, "Unexpected character.");
                }
                break;
        }
    }
    
    private void HandleNumber()
    {
        while (IsDigit(Peek())) Advance();

        // Look for fractional part
        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            //Consume the "."
            Advance();
            while (IsDigit(Peek())) Advance();
        }
        AddToken(NUMBER, Double.Parse(_source[_start.._current]));
    }

    private void Identifier()
    {
        while (IsAlphaNumeric(Peek())) Advance();
        var text = _source[_start.._current];
        Keywords.TryGetValue(text, out var type);
        if (type == default) type = IDENTIFIER;
        AddToken(type);
    }
    private static bool IsDigit(char c1)
    {
        return c1 is >= '0' and <= '9';
    }

    private static bool IsAlphaNumeric(char c1)
    {
        return IsAlpha(c1) || IsDigit(c1);
    }

    private static bool IsAlpha(char c)
    {
        return c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';
    }

    private char PeekNext()
    {
        return _current + 1 >= _source.Length ? '\0' : _source[_current + 1];
    }

    private char Peek()
    {
        return IsAtEnd() ? '\0' : _source[_current];
    }

    private bool Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (_source[_current] != expected) return false;
        _current++;
        return true;
    }

    private void AddToken(TokenType type, object p = default!)
    {
        var text = _source[_start.._current];
        _tokens.Add(new Token(type, text, p, _line));
    }

    private char Advance()
    {
        _current++;
        return _source[_current - 1];
    }

    private bool IsAtEnd()
    {
        return _current >= _source.Length;
    }
}