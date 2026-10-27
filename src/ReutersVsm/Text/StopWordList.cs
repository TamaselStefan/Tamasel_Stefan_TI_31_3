namespace ReutersVsm.Text;

internal static class StopWordList
{
    /// <summary>Loads one stop word per line into a hash set (blank lines ignored).</summary>
    public static IReadOnlySet<string> Load(string path) =>
        File.ReadLines(path)
            .Select(line => line.Trim().ToLowerInvariant())
            .Where(line => line.Length > 0)
            .ToHashSet(StringComparer.Ordinal);
}
