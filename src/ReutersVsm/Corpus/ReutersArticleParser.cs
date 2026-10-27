using System.Xml.Linq;

namespace ReutersVsm.Corpus;

internal static class ReutersArticleParser
{
    private const string TopicsCodesClass = "bip:topics:1.0";

    public static NewsArticle Parse(string filePath, string setLabel)
    {
        using var stream = File.OpenRead(filePath);
        return Parse(stream, Path.GetFileNameWithoutExtension(filePath), setLabel);
    }

    /// <summary>Parses an article. The XML reader honors the encoding declaration (iso-8859-1) and decodes entities.</summary>
    public static NewsArticle Parse(Stream xml, string name, string setLabel)
    {
        var root = XDocument.Load(xml).Root ?? throw new InvalidDataException("The document has no root element.");
        return new NewsArticle(name, setLabel, ExtractContent(root), ExtractTopics(root));
    }

    /// <summary>Title plus each &lt;p&gt; of &lt;text&gt; taken separately, so paragraphs never run together.</summary>
    private static string ExtractContent(XElement root)
    {
        var title = root.Element("title")?.Value ?? string.Empty;
        var text = root.Element("text") ?? throw new InvalidDataException("Missing <text> element.");
        var paragraphs = text.Descendants("p").Select(paragraph => paragraph.Value);
        return string.Join("\n", paragraphs.Prepend(title));
    }

    private static List<string> ExtractTopics(XElement root) =>
        root.Elements("metadata")
            .Elements("codes")
            .Where(codes => (string?)codes.Attribute("class") == TopicsCodesClass)
            .Elements("code")
            .Select(code => (string?)code.Attribute("code"))
            .OfType<string>()
            .ToList();
}
