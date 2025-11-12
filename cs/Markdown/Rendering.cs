using System.Text;

namespace Markdown;

public interface IRenderer
{
    string Render(IEnumerable<Node> lineNodes);
}

public class HtmlRenderer : IRenderer
{
    public string Render(IEnumerable<Node> lines)
    {
        throw new NotImplementedException();
    }
}