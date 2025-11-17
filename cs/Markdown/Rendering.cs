using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Markdown;

public interface IRenderer
{
    string Render(IEnumerable<Node> nodes);
}

public class HtmlRenderer : IRenderer
{
    public string Render(IEnumerable<Node> nodes)
    {
        if (nodes == null) return string.Empty;

        var sb = new StringBuilder();

        // Идём последовательно по узлам верхнего уровня
        foreach (var node in nodes)
        {
            RenderNode(node, sb);
        }

        return sb.ToString();
    }

    private static void RenderNode(Node node, StringBuilder sb)
    {
        if (node == null) return;

        switch (node.Type)
        {
            case NodeType.Header:
                // В соответствии с условием: абзац, начинающийся с "# ", -> <h1>...</h1>
                // Дети содержат инлайны (Plain/Emphasis/Strong/Link)
                sb.Append("<h1>");
                foreach (var child in node.Children)
                    RenderNode(child, sb);
                sb.Append("</h1>");
                break;

            case NodeType.Strong:
                sb.Append("<strong>");
                if (node.Children != null)
                    foreach (var child in node.Children)
                        RenderNode(child, sb);
                else
                    sb.Append(node.Value);
                sb.Append("</strong>");
                break;

            case NodeType.Emphasis:
                sb.Append("<em>");
                if (node.Children != null)
                    foreach (var child in node.Children)
                        RenderNode(child, sb);
                else
                    sb.Append(node.Value);
                sb.Append("</em>");
                break;

            case NodeType.Link:
                // В данной постановке правила ссылок не описаны подробно.
                // Предположим семантику: Value = href, Children = текст ссылки.
                // Если Children пусты — рендерим сам href как текст.
                var href = WebUtility.HtmlEncode(node.Value ?? string.Empty);
                sb.Append("<a href=\"").Append(href).Append("\">");
                if (node.Children != null && node.Children.Count > 0)
                    RenderInlineChildren(node.Children, sb);
                else
                    sb.Append(href);
                sb.Append("</a>");
                break;

            case NodeType.Text:
            default:
                // Текстовые узлы: экранируем
                var text = WebUtility.HtmlEncode(node.Value ?? string.Empty);
                sb.Append(text);
                // Если у Plain есть дети (на всякий случай) — тоже обойти
                if (node.Children != null && node.Children.Count > 0)
                    RenderInlineChildren(node.Children, sb);
                break;
        }
    }

    private static void RenderInlineChildren(List<Node> children, System.Text.StringBuilder sb)
    {
        if (children == null || children.Count == 0) return;

        // Соседние Plain можно слить ещё на этапе генерации; здесь просто обходим
        foreach (var child in children)
            RenderNode(child, sb);
    }
}