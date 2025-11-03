using System.Collections.Generic;

namespace Markdown;

public class Block
{
    public string Raw;
}

public class ParagraphBlock : Block
{
    public ParagraphBlock(string raw)
    {
    }
}

public class HeaderBlock : Block
{
    public HeaderBlock(string raw)
    {
    }
}