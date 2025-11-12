namespace Markdown;

public interface ITokenProcessor
{
    IEnumerable<Token?> Tokenize(string input);
}