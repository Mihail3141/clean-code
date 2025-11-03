namespace Markdown;

public interface IParser
{
    IReadOnlyList<LineNode> Parse(IEnumerable<Token> tokens);
}

public class Parser : IParser
{
    public IReadOnlyList<LineNode> Parse(IEnumerable<Token> tokens)
    {
        return System.Array.Empty<LineNode>();
    }
}