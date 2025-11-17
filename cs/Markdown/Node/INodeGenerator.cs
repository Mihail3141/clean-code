using System.Collections.Generic;

namespace Markdown;

public interface INodeGenerator
{
    List<Node> Create(List<Token> tokens);
}