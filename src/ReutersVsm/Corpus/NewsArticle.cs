namespace ReutersVsm.Corpus;

/// <summary>One Reuters article as extracted from its XML file, before any text processing.</summary>
/// <param name="Name">File name without extension, e.g. "2504NEWS".</param>
/// <param name="SetLabel">"Training", "Testing" or "All".</param>
/// <param name="Content">Title followed by every paragraph, one per line.</param>
/// <param name="Topics">Codes from &lt;codes class="bip:topics:1.0"&gt;, in file order.</param>
internal sealed record NewsArticle(string Name, string SetLabel, string Content, IReadOnlyList<string> Topics);
