using FluentAssertions;
using Markdown;
using NUnit.Framework;

namespace MarkdownTest;

public class RenderTest
{
    private readonly Md md = new Md();


    [TestCase("_курсив_", "<em>курсив</em>", TestName = "Просто курсив")]
    [TestCase("__полужирный__", "<strong>полужирный</strong>", TestName = "Просто полужирный")]
    [TestCase("_это просто текст_", "<em>это просто текст</em>", TestName = "Просто текст с курсивом")]
    [TestCase("__a _b_ c__", "<strong>a <em>b</em> c</strong>", TestName = "Курсив внутри полужирного должен работать")]
    public void Проверка_ПравильногоВыделения(string text, string expected)
    {
        var html = md.Render(text);
        html.Should().Be(expected);
    }

    [TestCase("__неправильное выделение_", "__неправильное выделение_", TestName = "Разные символы выделения")]
    [TestCase("_неправильное выделение__", "_неправильное выделение__", TestName = "Разные символы выделения")]
    [TestCase("_a __b  b__ c_", "<em>a __b  b__ c</em>", TestName = "Внутри одинарного двойное не работает.")]
    [TestCase("_ a_ bbb _a _", "_ a_ bbb _a _", TestName = "Неправильное прилипание не выделяет")]
    [TestCase("__пересечение _двойных__ и одинарных_", "__пересечение _двойных__ и одинарных_",
        TestName = "Пересечение разных выделений")]
    [TestCase("_пересечение __одинарных_ и двойных__", "_пересечение __одинарных_ и двойных__",
        TestName = "Пересечение разных выделений")]
    public void Проверка_НеправильногоВыделения(string text, string expected)
    {
        var html = md.Render(text);
        html.Should().Be(expected);
    }

    [TestCase("текст c цифрами_12_3", "текст c цифрами_12_3", TestName = "Подчерки в тексте с цифрами")]
    [TestCase("вну_три", "вну_три", TestName = "Подчерк внутри слова")]
    [TestCase("в__нутр__и сл_о_ва", "в<strong>нутр</strong>и сл<em>о</em>ва",
        TestName = "Выделение внутри слова работает")]
    [TestCase("в __нач__але _сло_ва", "в <strong>нач</strong>але <em>сло</em>ва",
        TestName = "Выделение в начале слова работает")]
    [TestCase("в ко__нце__ сло_ва_", "в ко<strong>нце</strong> сло<em>ва</em>",
        TestName = "Выделение в конце слова работает")]
    [TestCase("в ра_зных сл_овах", "в ра_зных сл_овах", TestName = "Выделение в разных словах не работает")]
    [TestCase("эти_ подчерки_ не считаются выделением", "эти_ подчерки_ не считаются выделением",
        TestName = "Пробельный символ после подчерка")]
    public void Выделение_внутри_слова(string text, string expected)
    {
        var html = md.Render(text);
        html.Should().Be(expected);
    }


    [TestCase("# Заголовок", "<h1>Заголовок</h1>", TestName = "Заголовок")]
    [TestCase("# Заголовок\n_курсив_ и __жирный__ и вну_три",
        "<h1>Заголовок</h1>\n<em>курсив</em> и <strong>жирный</strong> и вну_три",
        TestName = "Сохраняется_перенос_после_заголовка_и_инлайн_на_следующей_строке")]
    [TestCase("# Заголовок1\n_курсив_ и __жирный__ и вну_три\n# Заголовок2\nкапибара",
        "<h1>Заголовок1</h1>\n<em>курсив</em> и <strong>жирный</strong> и вну_три\n<h1>Заголовок2</h1>\nкапибара",
        TestName = "Несколько заголовков в одном тексте")]
    [TestCase("# Заголовок\nпросто текст",
        "<h1>Заголовок</h1>\nпросто текст",
        TestName = "Заголовок и перенос строки")]
    [TestCase("# Это заголовок c # внутри", "<h1>Это заголовок c # внутри</h1>", TestName = "Решётка внутри заголовка")]
    public void ВерныйЗаголовок(string text, string expected)
    {
        var html = md.Render(text);
        html.Should().Be(expected);
    }

    [TestCase("#Это не заголовок", "#Это не заголовок", TestName = "Нет пробела после #")]
    [TestCase("Это # не заголовок", "Это # не заголовок", TestName = "# внутри текста не заголовок")]
    [TestCase("Это не заголовок #", "Это не заголовок #", TestName = "# после текста не заголовок")]
    public void НеверныйЗаголовок(string text, string expected)
    {
        var html = md.Render(text);
        html.Should().Be(expected);
    }


    [TestCase(@"\_это не работает_", "_это не работает_", TestName = "Экранирование курсива")]
    [TestCase(@"\# Заголовок", "# Заголовок", TestName = "Экранирование заголовка")]
    [TestCase(@"\\_это работает_", @"\<em>это работает</em>", TestName = "Экранирование экранирования")]
    [TestCase(@"_это не\_ работает_", "<em>это не_ работает</em>", TestName = "Экранирование внутри")]
    public void Экранирование(string text, string expected)
    {
        var html = md.Render(text);
        html.Should().Be(expected);
    }
}