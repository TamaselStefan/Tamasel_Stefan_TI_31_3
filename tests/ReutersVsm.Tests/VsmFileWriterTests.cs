using ReutersVsm.Corpus;
using ReutersVsm.Vsm;

namespace ReutersVsm.Tests;

public class VsmFileWriterTests
{
    [Fact]
    public void Write_ProducesTheLayoutFromTheLabStatement()
    {
        ProcessedDocument[] documents =
        [
            Document("D1", "Training", ["ml", "cla", "clu"], "algorithm", "classification", "clustering", "learning"),
            Document("D2", "Training", ["ml", "cla"], "classification", "learning", "supervised"),
            Document("D3", "Testing", ["ml", "clu"], "clustering", "learning", "unsupervised"),
        ];
        var dictionary = TermDictionary.Build(documents);
        using var writer = new StringWriter();

        VsmFileWriter.Write(writer, dictionary, documents);

        const string expected = """
            @attribute algorithm
            @attribute classification
            @attribute clustering
            @attribute learning
            @attribute supervised
            @attribute unsupervised
            @data
            D1 # 0:1 1:1 2:1 3:1 # ml cla clu # Training
            D2 # 1:1 3:1 4:1 # ml cla # Training
            D3 # 2:1 3:1 5:1 # ml clu # Testing

            """;
        Assert.Equal(expected.ReplaceLineEndings("\n"), writer.ToString());
    }

    [Fact]
    public void SparseVector_CountsRepeatedTermsAndSortsByIndex()
    {
        var document = Document("D", "All", [], "zeta", "alpha", "zeta");
        var dictionary = TermDictionary.Build([document]);

        Assert.Equal("0:1 1:2", SparseVector.From(document, dictionary).ToString());
    }

    private static ProcessedDocument Document(string name, string set, string[] topics, params string[] terms) =>
        ProcessedDocument.FromTerms(new NewsArticle(name, set, string.Empty, topics), terms);
}
