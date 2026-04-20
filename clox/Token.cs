namespace clox;

public struct Token
{
    public TokenType Type;
    public int Start;
    public int Length;
    public int Line;
}