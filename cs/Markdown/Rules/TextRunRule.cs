namespace Markdown;

public sealed class TextRunRule : ITokenRule
{
    public Token TryReadTokenAndMoveCursor(InputCursor cursor)
    {
        if (cursor.End) return null;

        if (cursor.Current ==  '_' ||
            cursor.Current == '\n' ||
            cursor.Current == '\r' ||
            cursor.Current == '\\')
            return null;

        var start = cursor.Position;
        while (!cursor.End)
        {
            var c = cursor.Current;
            if (c == '_' || c == '\n' || c == '\r' || c == '\\') break;
            cursor.Move(1);
        }

        if (cursor.Position > start)
            return TokenFactory.Create(TokenType.Text, cursor.Slice(start, cursor.Position), start);

        return null;
    }
}