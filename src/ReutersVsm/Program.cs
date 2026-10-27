using System.Diagnostics;
using ReutersVsm;
using ReutersVsm.Cli;
using ReutersVsm.Corpus;
using ReutersVsm.Text;
using ReutersVsm.Vsm;

return Application.Run(args);

internal static class Application
{
    private const int Success = 0;
    private const int MissingInput = 1;
    private const int InvalidArguments = 2;

    public static int Run(string[] args)
    {
        CommandLineOptions options;
        try
        {
            options = CommandLineParser.Parse(args);
        }
        catch (ArgumentException exception)
        {
            Console.Error.WriteLine(exception.Message);
            Console.Error.WriteLine(CommandLineParser.Usage);
            return InvalidArguments;
        }

        if (options.ShowHelp)
        {
            Console.WriteLine(CommandLineParser.Usage);
            return Success;
        }

        var root = ProjectRoot.Locate(Directory.GetCurrentDirectory());
        return InputsExist(options, root) ? BuildVectorSpaceModel(options, root) : MissingInput;
    }

    private static bool InputsExist(CommandLineOptions options, string root)
    {
        var allFound = true;
        var stopWordsPath = ProjectRoot.Resolve(root, options.StopWordsPath);
        if (!File.Exists(stopWordsPath))
        {
            Console.Error.WriteLine($"Stop-word list not found: {stopWordsPath}. Pass its location with --stopwords <file>.");
            allFound = false;
        }

        foreach (var folder in options.InputFolders.Where(folder => !Directory.Exists(ProjectRoot.Resolve(root, folder.Path))))
        {
            Console.Error.WriteLine($"Input folder not found: {ProjectRoot.Resolve(root, folder.Path)}");
            allFound = false;
        }

        return allFound;
    }

    private static int BuildVectorSpaceModel(CommandLineOptions options, string root)
    {
        var stopwatch = Stopwatch.StartNew();
        var stopWords = StopWordList.Load(ProjectRoot.Resolve(root, options.StopWordsPath));
        Console.WriteLine($"Loaded {stopWords.Count} stop words.");

        var articles = LoadArticles(options.InputFolders, root);
        var outputPath = ProjectRoot.Resolve(root, options.OutputPath);
        var dumpWriter = options.DumpText ? new TextDumpWriter(Path.Combine(Path.GetDirectoryName(outputPath)!, "text")) : null;
        var documents = ProcessArticles(articles, new TextPreprocessor(stopWords), dumpWriter);

        var dictionary = TermDictionary.Build(documents);
        VsmFileWriter.Write(outputPath, dictionary, documents);

        Console.WriteLine($"Documents: {documents.Count}, dictionary size (vector dimension): {dictionary.Count}");
        Console.WriteLine($"Wrote {Path.GetRelativePath(root, outputPath)} in {stopwatch.Elapsed.TotalSeconds:F1} s");
        return Success;
    }

    private static List<NewsArticle> LoadArticles(IEnumerable<InputFolder> folders, string root)
    {
        var articles = new List<NewsArticle>();
        foreach (var folder in folders)
        {
            var result = CorpusLoader.LoadFolder(folder, ProjectRoot.Resolve(root, folder.Path));
            PrintLoadResult(result);
            articles.AddRange(result.Articles);
        }

        return articles;
    }

    private static void PrintLoadResult(FolderLoadResult result)
    {
        foreach (var error in result.Errors)
        {
            Console.Error.WriteLine($"  ERROR {error.FileName}: {error.Message}");
        }

        Console.WriteLine(
            $"{result.Folder.Label} ({result.Folder.Path}): {result.FilesFound} files found, " +
            $"{result.Articles.Count} OK, {result.Errors.Count} errors");
    }

    private static List<ProcessedDocument> ProcessArticles(
        IEnumerable<NewsArticle> articles, TextPreprocessor preprocessor, TextDumpWriter? dumpWriter)
    {
        var documents = new List<ProcessedDocument>();
        foreach (var article in articles)
        {
            var terms = preprocessor.Process(article.Content);
            dumpWriter?.Write(article, terms);
            documents.Add(ProcessedDocument.FromTerms(article, terms));
        }

        return documents;
    }
}
