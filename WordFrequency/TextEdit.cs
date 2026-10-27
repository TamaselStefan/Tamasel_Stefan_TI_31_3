using System.Text;

namespace WordFrequency;

/// <summary>
/// Preprocesses ONLY the first line of each TXT (the article text). Metadata lines are written back unchanged.
/// Must run on freshly extracted files: running it twice on the same file would stem already stemmed words.
/// </summary>
internal sealed class TextEdit(IReadOnlySet<string> stopWords)
{
    private static readonly Encoding Utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private readonly PorterStemmer _stemmer = new();

    public static HashSet<string> LoadStopWords(string path) =>
        File.ReadLines(path)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .ToHashSet(StringComparer.Ordinal);

    public ProcessingResult PreprocessFiles(IEnumerable<string> txtFiles)
    {
        var processed = new List<string>();
        var errors = new List<FileError>();

        foreach (var txtFile in txtFiles)
        {
            try
            {
                PreprocessFile(txtFile);
                processed.Add(txtFile);
            }
            catch (Exception exception) when (ProcessingResult.IsFileError(exception))
            {
                errors.Add(new FileError(Path.GetFileName(txtFile), exception.Message));
            }
        }

        return new ProcessingResult(processed, errors);
    }

    public void PreprocessFile(string txtFile)
    {
        var lines = File.ReadAllLines(txtFile);
        if (lines.Length == 0)
        {
            return;
        }

        lines[0] = PreprocessText(lines[0]);
        File.WriteAllLines(txtFile, lines, Utf8WithoutBom);
    }

    /// <summary>Lowercase, keep only a-z and spaces, tokenize, drop stop words, stem, join with single spaces.</summary>
    public string PreprocessText(string text)
    {
        var lettersOnly = ReplaceNonLettersWithSpaces(text.ToLowerInvariant());
        var terms = lettersOnly
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(token => !stopWords.Contains(token))
            .Select(_stemmer.StemWord);
        return string.Join(' ', terms);
    }

    private static string ReplaceNonLettersWithSpaces(string text) =>
        string.Create(text.Length, text, static (destination, source) =>
        {
            for (var i = 0; i < source.Length; i++)
            {
                destination[i] = source[i] is (>= 'a' and <= 'z') or ' ' ? source[i] : ' ';
            }
        });
}
