namespace Markdown;

public sealed class WhiteSpaceRule : ITokenRule
{
    public Token? TryReadTokenAndMoveCursor(InputCursor cursor)
    {
        // Не начинаем, если текущий символ не пробел/таб
        if (cursor.Current != ' ' || cursor.End)
            return null;

        // Копим подряд идущие пробелы/табы, но останавливаемся перед переводом строки
        var start = cursor.Position;

        while (!cursor.End)
        {
            if (cursor.Current != ' ')
                break;
            cursor.Move(1);
        }
        
        // Сдвигаем курсор и возвращаем токен пробелов как Text (или свой спецтип, если он у вас есть)
        var value = cursor.Slice(start, cursor.Position);
        return TokenFactory.Create(TokenType.WhiteSpace,value, start);
    }
}