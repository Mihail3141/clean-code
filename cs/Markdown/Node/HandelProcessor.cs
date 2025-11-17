namespace Markdown;

public static class Handel
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
    
    public static void Underscores(
        List<Node> children,
        List<(TokenType Type, int ChildrenIndex)> underscores,
        TokenCursor tokenCursor)
    {
        var currentToken = tokenCursor.Current;
        var previousToken = tokenCursor.tokens.ElementAtOrDefault(tokenCursor.Position - 1);
        var nextToken = tokenCursor.tokens.ElementAtOrDefault(tokenCursor.Position + 1);

        if (IsUnderscoreBetweenDigits(previousToken, nextToken)) // между числами
        {
            AddUnderscoreAsLiteral(children, currentToken.Value, tokenCursor);
            return;
        }

        if (!IsTokenWhitespaceLike(previousToken)) // слева пробел или граница
        {
            if (TryCloseExistingHighlight(underscores, currentToken, children, tokenCursor))
                return;
        }


        if (IsTokenWhitespaceLike(nextToken) ||
            (currentToken.Type == TokenType.DoubleUnderscore && DoesDoubleUnderscoreBreak(tokenCursor)))
        {
            AddUnderscoreAsLiteral(children, currentToken.Value, tokenCursor);
        }
        else
        {
            underscores.Add((currentToken.Type, children.Count));
            tokenCursor.Move();
        }
    }

    public static void InsertUnmatchedUnderscores(List<Node> children,
        List<(TokenType Type, int ChildrenIndex)> underscores)
    {
        for (var i = underscores.Count - 1; i >= 0; i--)
        {
            var (type, index) = underscores[i];
            var literal = type == TokenType.DoubleUnderscore ? "__" : "_";
            children.Insert(index, NodeFactory.Create(NodeType.Text, literal, null));
        }
    }

    private static bool TryCloseExistingHighlight(
        List<(TokenType Type, int ChildrenIndex)> underscores,
        Token currentToken,
        List<Node> children,
        TokenCursor cursor)
    {
        var highlightingType = currentToken.Type;
        var openerIndex = FindMatchingOpenerIndex(underscores, highlightingType);
        if (openerIndex < 0)
            return false;

        var openerUnderscore = underscores[openerIndex];
        var startIndex = openerUnderscore.ChildrenIndex;
        var innerTokensCount = children.Count - startIndex;

        if (innerTokensCount == 0)
            return false;

        var innerTokens = children.GetRange(startIndex, innerTokensCount);

        if (!IsValidHighlighting(underscores, highlightingType, openerUnderscore, innerTokens))
            return false;

        if (HasIntersection(underscores, openerUnderscore, highlightingType, children, openerIndex, out var innerIndex))
        {
            InsertIntersection(children, underscores, openerUnderscore, underscores[innerIndex], openerIndex,
                innerIndex);
            AddUnderscoreAsLiteral(children, currentToken.Value, cursor);
            return true;
        }

        CloseHighlight(children, underscores, openerIndex, highlightingType, innerTokens, startIndex);
        cursor.Move();
        return true;
    }

    private static int FindMatchingOpenerIndex(List<(TokenType Type, int ChildrenIndex)> underscores, TokenType type)
    {
        for (var i = underscores.Count - 1; i >= 0; i--)
        {
            if (underscores[i].Type == type)
                return i;
        }

        return -1;
    }

    private static bool IsValidHighlighting(
        List<(TokenType Type, int ChildrenIndex)> underscores,
        TokenType highlightingType,
        (TokenType Type, int ChildrenIndex) openerUnderscore,
        List<Node> innerTokens)
    {
        if (highlightingType != TokenType.Underscore)
            return true;
        //точно "_"
        var highlightingIsInsideDoubleUnderscore = underscores.Any(underscore =>
            underscore.Type == TokenType.DoubleUnderscore &&
            underscore.ChildrenIndex < openerUnderscore.ChildrenIndex); // "_" внутри двойного

        var areThereAnyWhitespaces = HasWhiteSpaceInPlainNodes(innerTokens); // Есть пробелы

        return highlightingIsInsideDoubleUnderscore
               || !areThereAnyWhitespaces;
    }

    private static bool HasWhiteSpaceInPlainNodes(List<Node> innerTokens)
    {
        if (innerTokens == null || innerTokens.Count == 0)
            return false;

        foreach (var t in innerTokens)
        {
            if (t.Type == NodeType.Text && t.Value != null)
            {
                var s = t.Value;
                for (int j = 0; j < s.Length; j++)
                {
                    if (char.IsWhiteSpace(s[j]))
                        return true;
                }
            }
        }

        return false;
    }

    private static bool HasIntersection(
        List<(TokenType Type, int ChildrenIndex)> underscores,
        (TokenType Type, int ChildrenIndex) opener,
        TokenType highlightingType,
        List<Node> children,
        int openerIndex,
        out int intersectionIndex)
    {
        for (var i = openerIndex + 1; i < underscores.Count; i++)
        {
            if (underscores[i].Type == highlightingType ||
                underscores[i].ChildrenIndex <= opener.ChildrenIndex ||
                underscores[i].ChildrenIndex >= children.Count)
                continue;

            intersectionIndex = i;
            return true;
        }

        intersectionIndex = -1;
        return false;
    }

    private static void CloseHighlight(
        List<Node> children,
        List<(TokenType Type, int ChildrenIndex)> underscores,
        int openerIndex,
        TokenType highlightingType,
        List<Node> innerTokens,
        int startIndex)
    {
        children.RemoveRange(startIndex, innerTokens.Count);

        var node = highlightingType == TokenType.DoubleUnderscore
            ? NodeFactory.Create(NodeType.Strong, null, innerTokens)
            : NodeFactory.Create(NodeType.Emphasis, null, innerTokens);

        children.Add(node);
        underscores.RemoveAt(openerIndex);
    }

    private static void InsertIntersection(
        List<Node> children,
        List<(TokenType Type, int ChildrenIndex)> underscores,
        (TokenType Type, int ChildrenIndex) opener,
        (TokenType Type, int ChildrenIndex) inner,
        int openerIndex, int innerIndex)
    {
        var innerLiteral = inner.Type == TokenType.DoubleUnderscore ? "__" : "_";
        var openerLiteral = opener.Type == TokenType.DoubleUnderscore ? "__" : "_";

        children.Insert(inner.ChildrenIndex, NodeFactory.Create(NodeType.Text, innerLiteral, null));
        children.Insert(inner.ChildrenIndex, NodeFactory.Create(NodeType.Text, openerLiteral, null));

        underscores.RemoveAt(innerIndex);
        underscores.RemoveAt(openerIndex);
    }

    private static void AddUnderscoreAsLiteral(List<Node> children, string value, TokenCursor cursor)
    {
        children.Add(NodeFactory.Create(NodeType.Text, value, null));
        cursor.Move();
    }

    private static bool IsTokenWhitespaceLike(Token? token)
    {
        return token == null ||
               token.Type == TokenType.WhiteSpace ||
               token.Type == TokenType.NewLine ||
               token.Type == TokenType.EndOfText;
    }

    private static bool IsUnderscoreBetweenDigits(Token? previousToken, Token? nextToken)
    {
        var leftTokenIsDigit = previousToken is { Type: TokenType.Text, Value.Length: > 0 } &&
                               char.IsDigit(previousToken.Value.Last());
        var rightTokenIsDigit = nextToken is { Type: TokenType.Text, Value.Length: > 0 } &&
                                char.IsDigit(nextToken.Value.First());
        return leftTokenIsDigit && rightTokenIsDigit;
    }

    private static bool DoesDoubleUnderscoreBreak(TokenCursor cursor)
    {
        var singleUnderscoresCount = 0;
        for (var i = cursor.Position + 1; i < cursor.TokenCount; i++)
        {
            var currentToken = cursor.tokens[i];
            if (currentToken.Type is TokenType.NewLine or TokenType.EndOfText) return false;
            if (currentToken.Type == TokenType.DoubleUnderscore) return (singleUnderscoresCount % 2) == 1;
            if (currentToken.Type != TokenType.Underscore) continue;

            var previousToken = cursor.tokens.ElementAtOrDefault(i - 1);
            if (previousToken is { Type: TokenType.Escape }) continue;

            singleUnderscoresCount++;
        }

        return false;
    }
}