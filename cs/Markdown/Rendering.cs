namespace Markdown;

public interface IRenderer
{
    string Render(IEnumerable<LineNode> lineNodes);
}

public class HtmlRenderer : IRenderer
{
    public string Render(IEnumerable<LineNode> lineNodes)
    {
        return string.Empty;
    }
}