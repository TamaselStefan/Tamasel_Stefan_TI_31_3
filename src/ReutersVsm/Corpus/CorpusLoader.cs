using System.Xml;
using ReutersVsm.Cli;

namespace ReutersVsm.Corpus;

internal sealed record LoadError(string FileName, string Message);

internal sealed record FolderLoadResult(
    InputFolder Folder,
    int FilesFound,
    IReadOnlyList<NewsArticle> Articles,
    IReadOnlyList<LoadError> Errors);

internal static class CorpusLoader
{
    /// <summary>Parses every .xml file (any case) in the folder. A bad file is recorded as an error, not thrown.</summary>
    public static FolderLoadResult LoadFolder(InputFolder folder, string fullPath)
    {
        var files = FindXmlFiles(fullPath);
        var articles = new List<NewsArticle>(files.Count);
        var errors = new List<LoadError>();

        foreach (var file in files)
        {
            try
            {
                articles.Add(ReutersArticleParser.Parse(file, folder.Label));
            }
            catch (Exception exception) when (IsBadFileException(exception))
            {
                errors.Add(new LoadError(Path.GetFileName(file), exception.Message));
            }
        }

        return new FolderLoadResult(folder, files.Count, articles, errors);
    }

    internal static List<string> FindXmlFiles(string directory) =>
        Directory.EnumerateFiles(directory)
            .Where(file => Path.GetExtension(file).Equals(".xml", StringComparison.OrdinalIgnoreCase))
            .OrderBy(Path.GetFileName, StringComparer.Ordinal)
            .ToList();

    private static bool IsBadFileException(Exception exception) =>
        exception is XmlException or IOException or UnauthorizedAccessException or InvalidDataException;
}
