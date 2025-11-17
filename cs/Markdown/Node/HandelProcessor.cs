namespace Markdown;

public static class HandelProcessor
{
    public static void Escape(List<Node> children, TokenCursor tokenCursor)
    {
        tokenCursor.Move();
        switch (tokenCursor.Current!.Type)
        {
            case TokenType.NewLine
                or TokenType.EndOfText:
                children.Add(NodeFactory.Create(NodeType.Text, tokenCursor.Current.Value, null));
                return;
            case TokenType.Text:
                children.Add(NodeFactory.Create(NodeType.Text, @"\" + tokenCursor.Current.Value, null));
                break;
            case TokenType.Escape:
            default:
                children.Add(NodeFactory.Create(NodeType.Text, tokenCursor.Current.Value, null));
                break;
        }

        tokenCursor.Move();
    }

    public static void Score(List<Node> children, TokenCursor tokenCursor)
    {
        var currentToken = tokenCursor.Current;
        var previousToken = tokenCursor.tokens.ElementAtOrDefault(tokenCursor.Position - 1);
        var nextToken = tokenCursor.tokens.ElementAtOrDefault(tokenCursor.Position + 1);

        if (TokenBetweenDigits(previousToken, nextToken) || ItWhiteSpaceOrEndToken(nextToken))
        {
            children.Add(NodeFactory.Create(NodeType.Text, currentToken.Value, null));
            tokenCursor.Move();
            return;
        }

        var currentTokenType = currentToken.Type;
        var currentTokenPos = tokenCursor.Position;
        var currentNodeType = currentTokenType == TokenType.DoubleUnderscore ? NodeType.Strong : NodeType.Emphasis;

        var innerScore = new List<(int Index, TokenType Type)>();
        var closeIndex = -1;

        for (var i = currentTokenPos + 1; i < tokenCursor.TokenCount; i++)
        {
            var t = tokenCursor.tokens[i];


            if (t.Type is TokenType.NewLine or TokenType.EndOfText)
                break;

            if (t.Type == currentTokenType)
            {
                var left = tokenCursor.tokens.ElementAtOrDefault(i - 1);
                var right = tokenCursor.tokens.ElementAtOrDefault(i + 1);
                if (left?.Type != TokenType.WhiteSpace &&
                    right?.Type is TokenType.WhiteSpace or TokenType.EndOfText)
                {
                    closeIndex = i;
                    break;
                }
            }

            if (t.Type == TokenType.Underscore || t.Type == TokenType.DoubleUnderscore)
                innerScore.Add((i, t.Type));
        }

        if (closeIndex < 0)
        {
            for (var i = currentTokenPos + 1; i < tokenCursor.TokenCount; i++)
            {
                var token = tokenCursor.tokens[i];
                if (token.Type == TokenType.WhiteSpace)
                {
                    children.Add(NodeFactory.Create(NodeType.Text, currentToken.Value, null));
                    tokenCursor.Move();
                    return;
                }

                if (token.Type == currentTokenType)
                {
                    closeIndex = i;
                    break;
                }
            }

            if (closeIndex < 0)
            {
                children.Add(NodeFactory.Create(NodeType.Text, currentToken.Value, null));
                tokenCursor.Move();
                return;
            }

            var nodes = GetChildren(tokenCursor, currentTokenPos + 1, closeIndex);
            children.Add(NodeFactory.Create(currentNodeType, null, nodes));
            tokenCursor.Move(closeIndex - currentTokenPos + 1);
            return;
        }

        var start = currentTokenPos + 1;
        var end = closeIndex;

        if (HasCrossingPairs(closeIndex, innerScore, tokenCursor))
        {
            children.Add(NodeFactory.Create(NodeType.Text, currentToken.Value, null));
            children.Add(NodeFactory.Create(NodeType.Text, null, GetChildren(tokenCursor, start, end)));
            children.Add(NodeFactory.Create(NodeType.Text, currentToken.Value, null));
            tokenCursor.Move(closeIndex - currentTokenPos + 1);
            return;
        }
        
        switch (currentTokenType)
        {
            case TokenType.Underscore:
            {
                var hasDoubleInside = innerScore.Any(u =>
                    u.Type == TokenType.DoubleUnderscore && u.Index > start && u.Index < end);
                if (hasDoubleInside)
                {
                    var node = NodeFactory.Create(NodeType.Emphasis, null, GetChildren(tokenCursor, start, end));
                    children.Add(node);
                }
                else
                {
                    var node = NodeFactory.Create(NodeType.Emphasis, null, GetChildren(tokenCursor, start, end));
                    children.Add(node);
                }

                tokenCursor.Move(closeIndex - currentTokenPos + 1);
                return;
            }
            case TokenType.DoubleUnderscore:
            {
                var childTokens = tokenCursor.tokens.Slice(currentTokenPos + 1, closeIndex - currentTokenPos);
                var childNode = new NodeGenerator().Create(childTokens);
                children.Add(NodeFactory.Create(currentNodeType, null, childNode));
                tokenCursor.Move(end - currentTokenPos + 1);
                break;
            }
        }
    }
    
    private static bool HasCrossingPairs(int closeIndex,
        List<(int Index, TokenType Type)> innerUnderscores,
        TokenCursor cursor)
    {
        var scoreType = cursor.Current.Type == TokenType.Underscore? TokenType.DoubleUnderscore : TokenType.Underscore;
        foreach (var u in innerUnderscores.Where(x => x.Type == scoreType))
        {
            for (var j = u.Index + 1; j < cursor.TokenCount; j++)
            {
                var t = cursor.tokens[j];
                if (t.Type is TokenType.NewLine or TokenType.EndOfText) 
                    break;
                if (t.Type == scoreType)
                {
                    var left = cursor.tokens.ElementAtOrDefault(j - 1);
                    if (left?.Type != TokenType.WhiteSpace)
                    {
                        if (j > closeIndex) 
                            return true;
                        break;
                    }
                }
            }
        }

        return false;
    }

    private static List<Node> GetChildren(TokenCursor cursor, int start, int end)
    {
        var slice = new List<Node>();
        for (int i = start; i < end; i++)
        {
            var t = cursor.tokens[i];
            slice.Add(NodeFactory.Create(NodeType.Text, t.Value, null));
        }

        return slice;
    }

    private static bool ItWhiteSpaceOrEndToken(Token? token)
    {
        return token is { Type: TokenType.WhiteSpace } or { Type: TokenType.EndOfText };
    }

    private static bool TokenBetweenDigits(Token? previousToken, Token? nextToken)
    {
        var leftTokenIsDigit = previousToken is { Type: TokenType.Text, Value.Length: > 0 } &&
                               char.IsDigit(previousToken.Value.Last());
        var rightTokenIsDigit = nextToken is { Type: TokenType.Text, Value.Length: > 0 } &&
                                char.IsDigit(nextToken.Value.First());
        return leftTokenIsDigit && rightTokenIsDigit;
    }
}