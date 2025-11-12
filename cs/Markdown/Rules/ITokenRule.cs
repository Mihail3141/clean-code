namespace Markdown;

public interface ITokenRule
{
    // Возвращает null при неуспехе. При неуспехе не сдвигает курсор.
    Token TryRead(InputCursor cursor, bool atLineStart);
}