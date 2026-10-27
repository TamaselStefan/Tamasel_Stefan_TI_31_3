using ReutersVsm.Corpus;

namespace ReutersVsm.Vsm;

/// <summary>An article together with its term frequencies (stemmed term -> occurrences in this document).</summary>
internal sealed record ProcessedDocument(NewsArticle Article, IReadOnlyDictionary<string, int> TermFrequencies)
{
    public static ProcessedDocument FromTerms(NewsArticle article, IEnumerable<string> terms) =>
        new(article, CountTerms(terms));

    internal static Dictionary<string, int> CountTerms(IEnumerable<string> terms)
    {
        var frequencies = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var term in terms)
        {
            frequencies[term] = frequencies.GetValueOrDefault(term) + 1;
        }

        return frequencies;
    }
}
