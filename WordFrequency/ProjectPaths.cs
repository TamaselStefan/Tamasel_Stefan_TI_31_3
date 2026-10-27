namespace WordFrequency;

/// <summary>
/// All paths in the lab are relative to the WordFrequency directory. This finds that directory whether the
/// program is started from it, from the repository root (dotnet run --project WordFrequency) or from Visual Studio.
/// </summary>
internal static class ProjectPaths
{
    private const string ProjectFileName = "WordFrequency.csproj";

    public static string ProjectDirectory { get; } = Locate();

    public static string Resolve(string relativePath) =>
        Path.GetFullPath(relativePath, ProjectDirectory);

    private static string Locate() =>
        FindUpwards(Directory.GetCurrentDirectory())
        ?? FindUpwards(AppContext.BaseDirectory)
        ?? Directory.GetCurrentDirectory();

    private static string? FindUpwards(string startDirectory)
    {
        for (var directory = new DirectoryInfo(startDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, ProjectFileName)))
            {
                return directory.FullName;
            }
        }

        return null;
    }
}
