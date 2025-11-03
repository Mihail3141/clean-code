using System.Collections.Generic;

namespace Markdown;

public class LineNode
{
}

public class TextNode : LineNode
{
    public string Text;
}

public class EmphasisNode : LineNode
{
    public List<LineNode> Children;
}

public class StrongNode : LineNode
{
    public List<LineNode> Children;
}

public class LinkNode : LineNode
{
    public string Href;
    public List<LineNode> Label;
}