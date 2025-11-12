using System.Collections.Generic;

namespace Markdown;

public class TokenGenerator : ITokenGenerator
{
    private readonly List<ITokenRule> rules = new()
    {
        new NewLineRule(),
        new EscapeRule(),
        new HeaderRule(),
        new UnderscoreRule(),
        new TextRunRule()
    };

    public IEnumerable<Token?> Tokenize(string input)
    {
        if (string.IsNullOrEmpty(input))
            yield break;
        //todo: подумать про строку из пробелов - IsNullOfWhitespace

        var cursor = new InputCursor(input);
        while (!cursor.End)
        {
            var isLineStart = true;
            yield return TryCreateToken(cursor);
        }
    }

    private Token? TryCreateToken(InputCursor cursor)
    {
        Token? token = null;
        foreach (var rule in rules)
        {
            token = rule.TryReadTokenAndMoveCursor(cursor);
            if (token != null)
                return token;
        }

        return token;
    }
}