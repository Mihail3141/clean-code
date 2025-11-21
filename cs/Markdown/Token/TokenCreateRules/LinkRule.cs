namespace Markdown;

public class LinkRule : ITokenRule
{
    private static readonly string[] ValidLinkPrefixes =
    [
        "http://",
        "https://",
        "www."
    ];

    public Token? TryReadTokenAndMoveCursor(TextCursor cursor)
    {
        if (cursor.End)
            return null;

        var start = cursor.Position;
        
        if (!IsLinkStart(cursor))
        {
            cursor.Revert(start);
            return null;
        }
        
        while (!cursor.End && IsLinkChar(cursor.Current))
            cursor.Move();

        var end = cursor.Position;
        
        var value = cursor.Slice(start, end);

        if (!IsValidLinkStart(value))
        {
            cursor.Revert(start);
            return null;
        }

        return TokenFactory.Create(TokenType.Link, value, start);
    }

    private static bool IsLinkStart(TextCursor cursor)
    {
        var mark = cursor.Position;
        
        if (ValidLinkPrefixes.Any(cursor.Matches))
        {
            cursor.Revert(mark);
            return true;
        }
        
        if (char.IsLetterOrDigit(cursor.Current))
        {
            var pos = cursor.Position;
            var hasDot = false;

            while (pos < cursor.Length && !char.IsWhiteSpace(cursor.Peek(pos - cursor.Position)))
            {
                if (cursor.Peek(pos - cursor.Position) == '.')
                {
                    hasDot = true;
                    break;
                }
                pos++;
            }

            cursor.Revert(mark);
            if (hasDot)
                return true;
        }
        else
        {
            cursor.Revert(mark);
        }

        return false;
    }

    private static bool IsLinkChar(char c)
    {
        if (char.IsWhiteSpace(c))
            return false;

        switch (c)
        {
            case ')':
            case ']':
            case '<':
            case '>':
            case '\"':
                return false;
        }

        return true;
    }

    private static bool IsValidLinkStart(string value)
    {
        if (ValidLinkPrefixes.Any(prefix => string.Equals(value, prefix, StringComparison.OrdinalIgnoreCase)))
            return false;

        var dotIndex = value.IndexOf('.');
        return dotIndex > 0 && dotIndex != value.Length - 1;
    }

}
