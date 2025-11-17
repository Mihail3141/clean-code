using Markdown;

public sealed class RuleBasedNodeGenerator : INodeGenerator
{
    private readonly InlinePipeline _inline;

    public RuleBasedNodeGenerator()
    {
        // Инициализация конвейера: порядок — от более специализированных к общим.
        _inline = new InlinePipeline(new INodeRule[]
        {
            new EscapeRule(),                 // экранирование [web:30]
            new StrongUnderscoreRule(null!),  // временно null — заменим после [web:34]
            new EmphasisUnderscoreRule(null!),// временно null [web:34]
            new PlainTextRule(),              // текст по умолчанию [web:36]
            new NewLineRule()                 // перенос строки [web:36]
        });

        // Внедряем ссылку на pipeline в правила, которые её требуют.
        ReplacePipeline(_inline);
    }

    private void ReplacePipeline(InlinePipeline pipeline)
    {
        foreach (var rule in pipeline
                 .GetType()
                 .GetField("_rules", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                 .GetValue(pipeline) as List<INodeRule>)
        {
            if (rule is StrongUnderscoreRule s && s.GetType().GetField("_inline", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null)
                s.GetType().GetField("_inline", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(s, pipeline); // внедрение контекста [web:34];

            if (rule is EmphasisUnderscoreRule e && e.GetType().GetField("_inline", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null)
                e.GetType().GetField("_inline", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(e, pipeline); // внедрение [web:34];
        }
    }

    public List<Node> Create(List<Token> tokens)
    {
        var currentHeader = new List<Token>(); // буфер строки 
        foreach (var t in tokens)
        {
            if (t.Type == TokenType.NewLine)
            {
                foreach (var node in FlushLine(currentHeader))
                    yield return node; // сброс строки 
                yield return new Node { Type = NodeType.Plain, Value = "\n" }; // перенос 
                currentHeader.Clear(); // новая строка 
                continue;
            }

            if (currentHeader.Count == 0 && t.Type == TokenType.HeaderMarker)
            {
                // Собираем до конца строки и обрабатываем заголовок отдельным правилом.
                currentHeader.Add(t); // маркер [web:30].
                continue;
            }

            currentHeader.Add(t); // копим токены строки [web:35].
        }

        // хвост
        foreach (var node in FlushLine(currentHeader)) 
            yield return node; // финальная строка 
    }

    private IEnumerable<Node> FlushLine(List<Token> lineTokens)
    {
        if (lineTokens.Count == 0) 
            yield break; // пусто 

        if (lineTokens[0].Type == TokenType.HeaderMarker)
        {
            var headerRule = new HeaderRule(_inline); // правило заголовка 
            int idx = 0;
            var outNodes = new List<Node>(); // временный список 
            headerRule.Apply(lineTokens, ref idx, outNodes); // применяем 
            foreach (var n in outNodes) yield return n; // отдаём 
        }
        else
        {
            foreach (var n in _inline.Parse(lineTokens)) 
                yield return n; // инлайны строки [web:34].
        }
    }
}
