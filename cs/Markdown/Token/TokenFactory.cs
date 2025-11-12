namespace Markdown;

public static class TokenFactory
{
    public static Token Create(TokenType type, string info, int pos)
        => new Token { Type = type, Value = info, StartPos = pos };
}