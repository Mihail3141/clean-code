using System.Collections.Generic;

namespace Markdown;

public interface IParser
{
    IReadOnlyList<LineNode> Parse(IEnumerable<Token> tokens);
}