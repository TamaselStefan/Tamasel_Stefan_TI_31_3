using System.Text;

namespace WordFrequency;

/// <summary>Raw word frequencies (no TF-IDF, no normalization) computed from the first line of each TXT.</summary>
internal static class FeatureExtraction
{
    private static readonly Encoding Utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    /// <summary>Word -&gt; number of occurrences in the article line of <paramref name="file"/>. Metadata is ignored.</summary>
    public static SortedDictionary<string, int> CreateLocalVector(string file)
    {
        var vector = new SortedDictionary<string, int>(StringComparer.Ordinal);
        foreach (var word in ReadArticleLine(file).Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            vector[word] = vector.GetValueOrDefault(word) + 1;
        }

        return vector;
    }

    /// <summary>Writes one line per TXT under <paramref name="folder"/>: "name # word:frequency word:frequency ...".
    /// The name is the path relative to <paramref name="folder"/>, without extension. The file is overwritten.</summary>
    public static int SaveLocalVectors(string folder, string outputFile)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputFile))!);

        var lines = FindTxtFiles(folder)
            .Select(file => FormatVectorLine(DocumentName(folder, file), CreateLocalVector(file)))
            .ToList();
        File.WriteAllLines(outputFile, lines, Utf8WithoutBom);
        return lines.Count;
    }

    /// <summary>All distinct words of the article lines in the given folders, sorted ordinally.
    /// Not used by the main flow yet (global indices and @attribute/@data come in the next stage).</summary>
    public static string[] CreateGlobalVector(params string[] folders) =>
        folders
            .SelectMany(FindTxtFiles)
            .SelectMany(file => ReadArticleLine(file).Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    internal static string FormatVectorLine(string documentName, SortedDictionary<string, int> vector)
    {
        var entries = string.Join(' ', vector.Select(pair => $"{pair.Key}:{pair.Value}"));
        return entries.Length == 0 ? $"{documentName} #" : $"{documentName} # {entries}";
    }

    private static string ReadArticleLine(string file) => File.ReadLines(file).FirstOrDefault() ?? string.Empty;

    private static IEnumerable<string> FindTxtFiles(string folder) =>
        Directory.EnumerateFiles(folder, "*.txt", SearchOption.AllDirectories)
            .Order(StringComparer.Ordinal);

    /// <summary>Relative path without extension, always with '/' so the output is the same on Windows and Linux.</summary>
    private static string DocumentName(string folder, string file) =>
        Path.ChangeExtension(Path.GetRelativePath(folder, file), null).Replace('\\', '/');
}
