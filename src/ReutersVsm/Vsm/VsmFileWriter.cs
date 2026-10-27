using System.Text;

namespace ReutersVsm.Vsm;

/// <summary>
/// Writes the vector space model:
/// one "@attribute term" line per dictionary term, then "@data", then one line per document:
/// "name # i:f i:f ... # TOPIC1 TOPIC2 # SetLabel".
/// </summary>
internal static class VsmFileWriter
{
    private const string FieldSeparator = " # ";

    public static void Write(string path, TermDictionary dictionary, IEnumerable<ProcessedDocument> documents)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var writer = new StreamWriter(path, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        Write(writer, dictionary, documents);
    }

    public static void Write(TextWriter writer, TermDictionary dictionary, IEnumerable<ProcessedDocument> documents)
    {
        writer.NewLine = "\n";
        foreach (var term in dictionary.Terms)
        {
            writer.WriteLine($"@attribute {term}");
        }

        writer.WriteLine("@data");
        foreach (var document in documents)
        {
            writer.WriteLine(FormatDataLine(document, dictionary));
        }
    }

    internal static string FormatDataLine(ProcessedDocument document, TermDictionary dictionary) =>
        string.Join(FieldSeparator,
            document.Article.Name,
            SparseVector.From(document, dictionary).ToString(),
            string.Join(' ', document.Article.Topics),
            document.Article.SetLabel);
}
