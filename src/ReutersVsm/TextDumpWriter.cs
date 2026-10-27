using System.Text;
using ReutersVsm.Corpus;

namespace ReutersVsm;

/// <summary>Debug output only: the extracted text and the final terms of each article. Never read back as input.</summary>
internal sealed class TextDumpWriter(string directory)
{
    private static readonly Encoding Utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public void Write(NewsArticle article, IReadOnlyList<string> terms)
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, $"{article.Name}.extracted.txt"), article.Content, Utf8WithoutBom);
        File.WriteAllText(Path.Combine(directory, $"{article.Name}.terms.txt"), string.Join(' ', terms), Utf8WithoutBom);
    }
}
