namespace Markdown;

public interface ITokenProcessor
{
    IEnumerable<Token> Tokenize(string input);
}

public class TokenProcessor : ITokenProcessor
{
    public IEnumerable<Token> Tokenize(string input)
    {
        yield break;
    }
}