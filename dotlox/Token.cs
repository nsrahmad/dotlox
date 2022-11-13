using System.Diagnostics;

namespace dotlox;

public enum TokenType
{
    // Single-character tokens.
    LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
    COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR,
    // One or two character tokens.
    BANG, BANG_EQUAL,
    EQUAL, EQUAL_EQUAL,
    GREATER, GREATER_EQUAL,
    LESS, LESS_EQUAL,
    // Literals.
    IDENTIFIER, STRING, NUMBER,
    // Keywords.
    AND, CLASS, ELSE, FALSE, FUN, FOR, IF, NIL, OR,
    PRINT, RETURN, SUPER, THIS, TRUE, VAR, WHILE,
    EOF
}

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class Token
{
    private readonly TokenType _type;
    private readonly string _lexeme;
    private readonly object _literal;
    private readonly int _line;

    public Token(TokenType type, string lexeme, object literal, int line)
    {
        this._type = type;
        this._lexeme = lexeme;
        this._literal = literal;
        this._line = line;
    }

    public override string ToString()
    {
        return $"{_type} {_lexeme} {_literal}";
    }

    private string GetDebuggerDisplay() => ToString();
}