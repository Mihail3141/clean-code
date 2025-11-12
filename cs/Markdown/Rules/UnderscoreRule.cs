namespace Markdown;

public sealed class UnderscoreRule : ITokenRule
{
    public Token TryReadTokenAndMoveCursor(InputCursor cursor)
    {
        if (cursor.End) return null;
        if (cursor.Current != '_') return null;

        var pos = cursor.Position;

        var count = 0;
        while (!cursor.End && cursor.Current == '_')
        {
            count++;
            cursor.Move(1);
        }

        if (count >= 2)
        {
            cursor.Revert(pos + 2);
            cursor.Move(2);
            return TokenFactory.Create(TokenType.DoubleUnderscore, "__", pos);
        }

        return TokenFactory.Create(TokenType.Underscore, "_", pos);
    }
}