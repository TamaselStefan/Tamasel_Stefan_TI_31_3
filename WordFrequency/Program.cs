using WordFrequency;

// Paths are relative to the WordFrequency directory.
Dataset[] datasets =
[
    new("Reuters_34/Training", "../Reuters/Reuters_34/Training", "output/Reuters_34/Training", "output/vectors/Reuters_34_Training.txt"),
    new("Reuters_34/Testing", "../Reuters/Reuters_34/Testing", "output/Reuters_34/Testing", "output/vectors/Reuters_34_Testing.txt"),
    new("Reuters_7083", "../Reuters/Reuters_7083", "output/Reuters_7083", "output/vectors/Reuters_7083.txt"),
];
const string StopWordsFile = "../stopwords.txt";

var stopWordsPath = ProjectPaths.Resolve(StopWordsFile);
if (!File.Exists(stopWordsPath))
{
    Console.Error.WriteLine($"Nu găsesc stopwords.txt: {stopWordsPath}");
    return 1;
}

var textEdit = new TextEdit(TextEdit.LoadStopWords(stopWordsPath));
var totalErrors = 0;

foreach (var (dataset, index) in datasets.Select((dataset, index) => (dataset, index + 1)))
{
    Console.WriteLine($"[{index}/{datasets.Length}] {dataset.Name}");
    totalErrors += ProcessDataset(dataset, textEdit);
}

Console.WriteLine("Salvez vectorii locali...");
foreach (var dataset in datasets.Where(dataset => Directory.Exists(ProjectPaths.Resolve(dataset.TxtFolder))))
{
    var count = FeatureExtraction.SaveLocalVectors(ProjectPaths.Resolve(dataset.TxtFolder), ProjectPaths.Resolve(dataset.VectorsFile));
    Console.WriteLine($"  {dataset.VectorsFile}: {count} vectori");
}

Console.WriteLine(totalErrors == 0 ? "Gata, fără erori." : $"Gata, cu {totalErrors} erori (vezi mai sus).");
return 0;

static int ProcessDataset(Dataset dataset, TextEdit textEdit)
{
    var xmlFolder = ProjectPaths.Resolve(dataset.XmlFolder);
    if (!Directory.Exists(xmlFolder))
    {
        Console.Error.WriteLine($"  Lipsește folderul: {xmlFolder}");
        return 1;
    }

    // Always regenerate the TXT files from XML first, so preprocessing never runs on already stemmed text.
    var extraction = Extraction.ExtractFolder(xmlFolder, ProjectPaths.Resolve(dataset.TxtFolder));
    PrintErrors(extraction.Errors);
    Console.WriteLine($"  extrase: {extraction.ProcessedFiles.Count} articole, {extraction.Errors.Count} erori");

    var preprocessing = textEdit.PreprocessFiles(extraction.ProcessedFiles);
    PrintErrors(preprocessing.Errors);
    Console.WriteLine($"  preprocesate: {preprocessing.ProcessedFiles.Count} articole, {preprocessing.Errors.Count} erori");

    return extraction.Errors.Count + preprocessing.Errors.Count;
}

static void PrintErrors(IEnumerable<FileError> errors)
{
    foreach (var error in errors)
    {
        Console.Error.WriteLine($"  EROARE {error.FileName}: {error.Message}");
    }
}

internal sealed record Dataset(string Name, string XmlFolder, string TxtFolder, string VectorsFile);
