namespace Markdown;

public enum NodeType
{
    Plain,       // обычная строка
    Header,      // заголовок
    Emphasis,    // целиком строка в _..._
    Strong,      // целиком строка в __...__
    Link         // ссылка
}