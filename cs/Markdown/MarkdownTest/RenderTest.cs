using FluentAssertions;
using Markdown;
using NUnit.Framework;

namespace MarkdownTest;

public class RenderTest
{
    private readonly Md md = new Md();

    [Test]
    public void Проверка_экранирования()
    {
        var text = "\\_Вот это\\_";
        var html = md.Render(text);

        html.Should().Be("_Вот это_");
    }
    
    [Test]
    public void Курсив_внутри_жирного()
    {
        var text = "__a _b_ c__";
        var html = md.Render(text);

        html.Should().Be("<strong>a <em>b</em> c</strong>");
    }
    
    [Test]
    public void Жирный_внутри_курсива()
    {
        var text = "_a __b__ c_";
        var html = md.Render(text);

        html.Should().Be("<em>a __b__ c</em>");
    }
    
    [Test]
    public void Неправильное_прилипание()
    {
        var text = "_ a_ bbb _a _";
        var html = md.Render(text);

        html.Should().Be("_ a_ bbb _a _");
    }
    
    [Test]
    public void Renders_Strong_With_Double_Underscore()
    {
        var text = "Это __жирный__ текст";
        var html = md.Render(text);

        html.Should().Be("Это <strong>жирный</strong> текст");
    }

    [Test]
    public void Renders_Emphasis_With_Single_Underscore_When_Not_Inside_Word()
    {
        var text = "Текст, _окруженный с двух сторон_ одинарными символами подчерка";
        var html = md.Render(text);

        html.Should().Be("Текст, <em>окруженный с двух сторон</em> одинарными символами подчерка");
    }

    [Test]
    public void Does_Not_Emphasize_Single_Underscore_Inside_Word()
    {
        var text = "вну_три";
        var html = md.Render(text);

        html.Should().Be("вну_три");
    }

    [Test]
    public void Renders_Header_H1_For_Line_Starting_With_Hash_Space()
    {
        var text = "# Заголовок";
        var html = md.Render(text);

        html.Should().Be("<h1>Заголовок</h1>");
    }

    [Test]
    public void Keeps_Newline_After_Header_And_Renders_Inline_Text_On_Next_Line()
    {
        var text = "# Заголовок\n_курсив_ и __жирный__ и вну_три";
        var html = md.Render(text);

        html.Should().Be("<h1>Заголовок</h1>\n<em>курсив</em> и <strong>жирный</strong> и вну_три");
    }

    [Test]
    public void Supports_Multiple_Headers_In_One_Text()
    {
        var text = "# Заголовок1\n_курсив_ и __жирный__ и вну_три\n# Заголовок2\nкапибара";
        var html = md.Render(text);

        html.Should()
            .Be(
                "<h1>Заголовок1</h1>\n<em>курсив</em> и <strong>жирный</strong> и вну_три\n<h1>Заголовок2</h1>\nкапибара");
    }

    public static IEnumerable<object[]> StrongCases()
    {
        yield return new object[] { "__a__", "<strong>a</strong>" };
        yield return new object[] { "____", "<strong></strong><strong></strong>" };
        yield return
            new object[] { "___a__", "<strong>_a</strong>" }; // одинарное останется как текст
    }


    public static IEnumerable<object[]> EmphasisCases()
    {
        yield return new object[] { "_a_", "<em>a</em>" };
        yield return new object[] { "a_b", "a_b" }; // внутри слова не срабатывает
        yield return
            new object[] { "_ a_", "_ a_" }; // открывающее слитно с пробелом — остаётся как текст
    }
}