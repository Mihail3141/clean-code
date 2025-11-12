using System.Collections.Generic;

namespace Markdown;

public interface INodeGenerator
{
    IEnumerable<Node> Create(IEnumerable<Token> tokens);
}