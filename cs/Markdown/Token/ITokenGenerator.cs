namespace Markdown;

public interface ITokenGenerator
{
    IEnumerable<Token?> Tokenize(string input);
}