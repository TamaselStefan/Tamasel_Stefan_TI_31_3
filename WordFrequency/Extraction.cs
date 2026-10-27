using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace WordFrequency;

/// <summary>
/// Turns each Reuters XML article into a TXT file:
/// line 1 = title + all text of &lt;text&gt; on one line; next lines = metadata/codes
/// (the class value, then each code value), in XML order.
/// </summary>
internal static partial class Extraction
{
    private static readonly Encoding Utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    /// <summary>Extracts every .xml file (any letter case) under <paramref name="xmlFolder"/> into <paramref name="txtFolder"/>.
    /// A bad article is reported in the result and does not stop the others.</summary>
    public static ProcessingResult ExtractFolder(string xmlFolder, string txtFolder)
    {
        var written = new List<string>();
        var errors = new List<FileError>();

        foreach (var xmlFile in FindXmlFiles(xmlFolder))
        {
            try
            {
                var txtFile = TxtPathFor(xmlFile, xmlFolder, txtFolder);
                Directory.CreateDirectory(Path.GetDirectoryName(txtFile)!);
                File.WriteAllLines(txtFile, ExtractArticle(xmlFile), Utf8WithoutBom);
                written.Add(txtFile);
            }
            catch (Exception exception) when (ProcessingResult.IsFileError(exception))
            {
                errors.Add(new FileError(Path.GetRelativePath(xmlFolder, xmlFile), exception.Message));
            }
        }

        return new ProcessingResult(written, errors);
    }

    /// <summary>The lines of the TXT file for one article. XDocument honors the declared encoding (iso-8859-1).</summary>
    public static List<string> ExtractArticle(string xmlFile)
    {
        var root = XDocument.Load(xmlFile).Root ?? throw new InvalidDataException("The document has no root element.");
        return [ArticleLine(root), .. MetadataLines(root)];
    }

    private static string ArticleLine(XElement root)
    {
        var title = root.Element("title")?.Value ?? string.Empty;
        var text = root.Element("text") ?? throw new InvalidDataException("Missing <text> element.");

        // Joining every text node with a space keeps paragraphs apart even when the tags are glued: </p><p>.
        var textContent = string.Join(" ", text.DescendantNodes().OfType<XText>().Select(node => node.Value));
        return NormalizeWhitespace(title + " " + textContent);
    }

    private static IEnumerable<string> MetadataLines(XElement root)
    {
        var codeGroups = root.Element("metadata")?.Elements("codes") ?? [];
        foreach (var codes in codeGroups)
        {
            var codeClass = (string?)codes.Attribute("class");
            if (string.IsNullOrWhiteSpace(codeClass))
            {
                continue;
            }

            yield return codeClass;
            foreach (var code in codes.Elements("code").Select(element => (string?)element.Attribute("code")))
            {
                if (!string.IsNullOrWhiteSpace(code))
                {
                    yield return code;
                }
            }
        }
    }

    private static string NormalizeWhitespace(string text) => Whitespace().Replace(text, " ").Trim();

    private static IEnumerable<string> FindXmlFiles(string folder) =>
        Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories)
            .Where(file => Path.GetExtension(file).Equals(".xml", StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.Ordinal);

    private static string TxtPathFor(string xmlFile, string xmlFolder, string txtFolder) =>
        Path.ChangeExtension(Path.Combine(txtFolder, Path.GetRelativePath(xmlFolder, xmlFile)), ".txt");

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
