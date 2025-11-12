using System.Collections.Generic;

namespace Markdown;

public class Token
{
    public TokenType Type;
    public required string Value { get; init; }
    public int StartPos { get; init; }
    public int EndPos => StartPos + Value.Length;
}