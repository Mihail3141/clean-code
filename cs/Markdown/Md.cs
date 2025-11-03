namespace Markdown;

using System;
using System.Collections.Generic;
using System.Linq;

public class Md
{
    private readonly BlockProcessor blockProcessor;
    private readonly TokenProcessor tokenProcessor;
    private readonly Parser parser;
    private readonly HtmlRenderer htmlRenderer;

    public Md()
        : this(new BlockProcessor(), new TokenProcessor(), new Parser(), new HtmlRenderer())
    {
    }

    public Md(BlockProcessor blockProcessor, TokenProcessor tokenProcessor, Parser parser,
        HtmlRenderer htmlRenderer)
    {
        this.blockProcessor = blockProcessor;
        this.tokenProcessor = tokenProcessor;
        this.parser = parser;
        this.htmlRenderer = htmlRenderer;
    }

    public string Render(string markdownText)
    {
        var blocks = blockProcessor.SplitToBlocks(markdownText);
        var renderedBlocks = new List<string>();
        foreach (var block in blocks)
        {
            var raw = block.Raw;
            var tokens = tokenProcessor.Tokenize(raw);
            var lineNodes = parser.Parse(tokens);
            var html = htmlRenderer.Render(lineNodes);
            renderedBlocks.Add(html);
        }

        return string.Concat(renderedBlocks);
    }
}