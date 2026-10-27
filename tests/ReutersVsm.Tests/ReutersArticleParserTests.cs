using System.Text;
using ReutersVsm.Corpus;

namespace ReutersVsm.Tests;

public class ReutersArticleParserTests
{
    private const string Xml = """
        <?xml version="1.0" encoding="iso-8859-1" ?>
        <newsitem itemid="1" id="root" date="1996-08-20" xml:lang="en">
        <title>USA: Novell Inc. unveils intranet product.</title>
        <headline>HEADLINE must be ignored</headline>
        <dateline>BURLINGAME, Calif. 1996-08-20</dateline>
        <text>
        <p>Novell Inc. Tuesday unveiled intranets.</p>
        <p>&quot;Caf&#233; caf=</p>
        </text>
        <copyright>(c) Reuters Limited 1996</copyright>
        <metadata>
        <codes class="bip:countries:1.0"><code code="USA"/></codes>
        <codes class="bip:industries:1.0"><code code="I33020"/></codes>
        <codes class="bip:topics:1.0"><code code="C22"><editdetail action="confirmed"/></code><code code="CCAT"/></codes>
        </metadata>
        </newsitem>
        """;

    private static NewsArticle ParseLatin1(string xml) =>
        ReutersArticleParser.Parse(new MemoryStream(Encoding.Latin1.GetBytes(xml)), "2504NEWS", "Testing");

    [Fact]
    public void Parse_ExtractsTitleAndEachParagraphOnItsOwnLine()
    {
        // "caf=" holds a raw Latin-1 byte (0xE9) to prove the declared encoding is honored.
        var article = ParseLatin1(Xml.Replace("caf=", "café"));

        Assert.Equal(
            "USA: Novell Inc. unveils intranet product.\nNovell Inc. Tuesday unveiled intranets.\n\"Café café",
            article.Content);
    }

    [Fact]
    public void Parse_ReadsOnlyTopicCodes()
    {
        var article = ParseLatin1(Xml);

        Assert.Equal(["C22", "CCAT"], article.Topics);
        Assert.Equal("2504NEWS", article.Name);
        Assert.Equal("Testing", article.SetLabel);
    }

    [Fact]
    public void Parse_RejectsArticleWithoutText() =>
        Assert.Throws<InvalidDataException>(() => ParseLatin1("<newsitem><title>t</title></newsitem>"));
}
