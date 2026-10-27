namespace ReutersVsm.Cli;

internal static class CommandLineParser
{
    public const string DefaultStopWordsPath = "stopwords.txt";

    public const string Usage = """
        Usage: dotnet run --project src/ReutersVsm -- [options]

          --preset <34|7083>        Built-in inputs and output (default: 34)
                                      34   = Reuters/Reuters_34/Training + Testing -> output/reuters_34.txt
                                      7083 = Reuters/Reuters_7083 (label All)      -> output/reuters_7083.txt
          --input <folder>=<label>  Input folder and its set label; repeatable, replaces the preset inputs
          --output <file>           Output file (overrides the preset output)
          --stopwords <file>        Stop-word list (default: stopwords.txt)
          --dump-text               Also write each article's extracted text and final tokens
                                    to <output folder>/text/ (debugging only, never read back)
          --help                    Show this help

        Relative paths are resolved against the project root (the folder containing ReutersVsm.sln).
        """;

    private sealed record Preset(IReadOnlyList<InputFolder> InputFolders, string OutputPath);

    private static readonly Dictionary<string, Preset> Presets = new(StringComparer.Ordinal)
    {
        ["34"] = new Preset(
            [
                new InputFolder("Reuters/Reuters_34/Training", "Training"),
                new InputFolder("Reuters/Reuters_34/Testing", "Testing"),
            ],
            "output/reuters_34.txt"),
        ["7083"] = new Preset(
            [new InputFolder("Reuters/Reuters_7083", "All")],
            "output/reuters_7083.txt"),
    };

    /// <exception cref="ArgumentException">The arguments are invalid.</exception>
    public static CommandLineOptions Parse(IReadOnlyList<string> args)
    {
        var presetName = "34";
        var explicitInputs = new List<InputFolder>();
        string? outputPath = null;
        var stopWordsPath = DefaultStopWordsPath;
        var dumpText = false;
        var showHelp = false;

        for (var i = 0; i < args.Count; i++)
        {
            switch (args[i])
            {
                case "--preset": presetName = ReadValue(args, ref i); break;
                case "--input": explicitInputs.Add(ParseInputFolder(ReadValue(args, ref i))); break;
                case "--output": outputPath = ReadValue(args, ref i); break;
                case "--stopwords": stopWordsPath = ReadValue(args, ref i); break;
                case "--dump-text": dumpText = true; break;
                case "--help" or "-h": showHelp = true; break;
                default: throw new ArgumentException($"Unknown argument '{args[i]}'.");
            }
        }

        if (!Presets.TryGetValue(presetName, out var preset))
        {
            throw new ArgumentException($"Unknown preset '{presetName}'. Use one of: {string.Join(", ", Presets.Keys)}.");
        }

        return new CommandLineOptions(
            InputFolders: explicitInputs.Count > 0 ? explicitInputs : preset.InputFolders,
            OutputPath: outputPath ?? preset.OutputPath,
            StopWordsPath: stopWordsPath,
            DumpText: dumpText,
            ShowHelp: showHelp);
    }

    internal static InputFolder ParseInputFolder(string value)
    {
        var separatorIndex = value.LastIndexOf('=');
        if (separatorIndex <= 0 || separatorIndex == value.Length - 1)
        {
            throw new ArgumentException($"Expected --input <folder>=<label>, got '{value}'.");
        }

        return new InputFolder(value[..separatorIndex], value[(separatorIndex + 1)..]);
    }

    private static string ReadValue(IReadOnlyList<string> args, ref int index)
    {
        if (index + 1 >= args.Count)
        {
            throw new ArgumentException($"Missing value after '{args[index]}'.");
        }

        index++;
        return args[index];
    }
}
