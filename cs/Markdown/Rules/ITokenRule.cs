namespace Markdown;

public interface ITokenRule
{
    Token? TryReadTokenAndMoveCursor(InputCursor cursor);
}