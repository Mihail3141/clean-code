// File: Parser.cs

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markdown;

public class Parser : IParser
{
    public IReadOnlyList<LineNode> Parse(IEnumerable<Token> tokens)
    {
        var list = tokens?.ToList() ?? new List<Token>();
        var result = new List<LineNode>();
        var textBuffer = new StringBuilder();

        // Стек открытых маркеров: тип + индекс начала детей в result
        var stack = new Stack<(TokenType Type, int OutIndex, int OpenPos)>();

        for (int i = 0; i < list.Count; i++)
        {
            var t = list[i];

            if (t.Type == TokenType.Text)
            {
                textBuffer.Append(t.Value);
                continue;
            }

            FlushTextBuffer(textBuffer, result);

            if (t.Type == TokenType.Underscore)
            {
                if (IsSingleUnderscoreInsideWord(list, i))
                {
                    result.Add(new TextNode("_", t.StartPos, t.StartPos + 1));
                    continue;
                }

                if (TryClose(stack, TokenType.Underscore, result, (open, children) =>
                        new EmphasisNode(children, startPos: open.openPos, endPos: t.StartPos + 1)))
                {
                    continue;
                }

                stack.Push((TokenType.Underscore, result.Count, t.StartPos));
                continue;
            }

            if (t.Type == TokenType.DoubleUnderscore)
            {
                if (TryClose(stack, TokenType.DoubleUnderscore, result, (open, children) =>
                        new StrongNode(children, startPos: open.openPos, endPos: t.StartPos + 2)))
                {
                    continue;
                }

                stack.Push((TokenType.DoubleUnderscore, result.Count, t.StartPos));
                continue;
            }

            // Другие типы при расширении синтаксиса:
            // можно добавить HeaderMarker обработку, ссылки и т.д.
        }

        FlushTextBuffer(textBuffer, result);

        // Возврат незакрытых маркеров в поток как текст
        while (stack.Count > 0)
        {
            var unclosed = stack.Pop();
            var marker = unclosed.Type == TokenType.Underscore ? "_" : "__";
            result.Insert(unclosed.OutIndex, new TextNode(marker));
        }

        return result;
    }

    private static void FlushTextBuffer(StringBuilder sb, List<LineNode> output)
    {
        if (sb.Length == 0) return;
        output.Add(new TextNode(sb.ToString()));
        sb.Clear();
    }

    // Закрывает верх стека, если тип совпадает; заворачивает детей в контейнер
    // Parser.cs — исправленная версия TryClose и вызовы

    private static bool TryClose(
        Stack<(TokenType Type, int OutIndex, int OpenPos)> stack,
        TokenType type,
        List<LineNode> output,
        Func<(int openPos, int outIndex), List<LineNode>, LineNode> makeNode)
    {
        if (stack.Count == 0) return false;
        var open = stack.Peek();
        if (open.Type != type) return false;

        // Снимаем открывающий маркер
        stack.Pop();

        // Забираем детей с позиции открытия
        var children = output.Skip(open.OutIndex).ToList();
        output.RemoveRange(open.OutIndex, output.Count - open.OutIndex);

        // Создаём узел, передавая координату открытия
        var node = makeNode((open.OpenPos, open.OutIndex), children);
        output.Add(node);
        return true;
    }


    // Одиночный '_' «внутри слова»: по обе стороны буква/цифра — тогда это текст
    private static bool IsSingleUnderscoreInsideWord(IReadOnlyList<Token> tokens, int index)
    {
        bool leftAlnum = false, rightAlnum = false;

        for (int i = index - 1; i >= 0; i--)
        {
            var t = tokens[i];
            if (t.Type == TokenType.Text && !string.IsNullOrEmpty(t.Value))
            {
                char lc = t.Value[t.Value.Length - 1];
                leftAlnum = char.IsLetterOrDigit(lc);
                break;
            }
            else if (t.Type == TokenType.Underscore || t.Type == TokenType.DoubleUnderscore)
            {
                break;
            }
        }

        for (int i = index + 1; i < tokens.Count; i++)
        {
            var t = tokens[i];
            if (t.Type == TokenType.Text && !string.IsNullOrEmpty(t.Value))
            {
                char rc = t.Value[0];
                rightAlnum = char.IsLetterOrDigit(rc);
                break;
            }
            else if (t.Type == TokenType.Underscore || t.Type == TokenType.DoubleUnderscore)
            {
                break;
            }
        }

        return leftAlnum && rightAlnum;
    }
}