namespace Markdown;

public sealed class HeaderRule : ITokenRule
{
    public Token? TryReadTokenAndMoveCursor(InputCursor cursor)
    {
        if (!cursor.IsNewLine() || cursor.End) return null;
        
        if (cursor.Current == '#' && cursor.Peek() == ' ')
        {
            var currentPos = cursor.Position;
            cursor.Move(2);
            return TokenFactory.Create(TokenType.HeaderMarker, "# ", currentPos);
        }
        
        return null;
    }
}