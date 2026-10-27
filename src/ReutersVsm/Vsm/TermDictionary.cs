namespace ReutersVsm.Vsm;

/// <summary>All distinct terms of the corpus, sorted ordinally. A term's position is its vector index.</summary>
internal sealed class TermDictionary
{
    private readonly Dictionary<string, int> _indexByTerm;

    private TermDictionary(IReadOnlyList<string> sortedTerms)
    {
        Terms = sortedTerms;
        _indexByTerm = new Dictionary<string, int>(sortedTerms.Count, StringComparer.Ordinal);
        for (var index = 0; index < sortedTerms.Count; index++)
        {
            _indexByTerm.Add(sortedTerms[index], index);
        }
    }

    public IReadOnlyList<string> Terms { get; }

    /// <summary>The dimension of the vector space.</summary>
    public int Count => Terms.Count;

    public int IndexOf(string term) => _indexByTerm[term];

    public static TermDictionary Build(IEnumerable<ProcessedDocument> documents) =>
        new(documents
            .SelectMany(document => document.TermFrequencies.Keys)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList());
}
