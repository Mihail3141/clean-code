using System.Collections.Generic;

namespace Markdown;

public class TokenGenerator : ITokenGenerator
{
    private readonly List<ITokenRule> rules = new()
    {
        new NewLineRule(),
        new WhiteSpaceRule(),
        new EscapeRule(),
        new HeaderRule(),
        new UnderscoreRule(),
        new TextRunRule(),
        new DigitRule()
    };

    public List<Token> Tokenize(string text)
    {
        if (string.IsNullOrEmpty(text))
            return null;
        var tokens = new List<Token>();
        //todo: подумать про строку из пробелов - IsNullOfWhitespace

        var cursor = new TextCursor(text);
        while (!cursor.End)
        {
            var token = TryCreateToken(cursor);
            tokens.Add(token);
        }
        tokens.Add(TokenFactory.Create(TokenType.EndOfText, null, text.Length));
        return tokens;
    }

    private Token? TryCreateToken(TextCursor cursor)
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