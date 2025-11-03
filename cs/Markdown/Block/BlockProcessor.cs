namespace Markdown;

public interface IBlockProcessor
{
    IEnumerable<Block> SplitToBlocks(string text);
}

public class BlockProcessor : IBlockProcessor
{
    public IEnumerable<Block> SplitToBlocks(string text)
    {
        yield break;
    }
}