namespace ReutersVsm.Cli;

/// <summary>A folder of Reuters XML files and the set label ("Training", "Testing", "All") its documents get.</summary>
internal sealed record InputFolder(string Path, string Label);

internal sealed record CommandLineOptions(
    IReadOnlyList<InputFolder> InputFolders,
    string OutputPath,
    string StopWordsPath,
    bool DumpText,
    bool ShowHelp);
