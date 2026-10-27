namespace ReutersVsm.Vsm;

internal readonly record struct SparseEntry(int Index, int Frequency);

/// <summary>The non-zero entries of a document vector, sorted by index.</summary>
internal sealed class SparseVector(IReadOnlyList<SparseEntry> entries)
{
    public IReadOnlyList<SparseEntry> Entries { get; } = entries;

    public static SparseVector From(ProcessedDocument document, TermDictionary dictionary) =>
        new(document.TermFrequencies
            .Select(pair => new SparseEntry(dictionary.IndexOf(pair.Key), pair.Value))
            .OrderBy(entry => entry.Index)
            .ToList());

    /// <summary>"index:frequency" pairs separated by single spaces, e.g. "0:1 3:2".</summary>
    public override string ToString() =>
        string.Join(' ', Entries.Select(entry => FormattableString.Invariant($"{entry.Index}:{entry.Frequency}")));
}
