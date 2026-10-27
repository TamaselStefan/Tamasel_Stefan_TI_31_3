namespace ReutersVsm;

/// <summary>Finds the project root so relative paths work from any working directory (CLI, IDE, bin folder).</summary>
internal static class ProjectRoot
{
    private const string MarkerFileName = "ReutersVsm.sln";

    /// <summary>The closest folder at or above <paramref name="startDirectory"/> that contains the solution file,
    /// or <paramref name="startDirectory"/> itself when there is none.</summary>
    public static string Locate(string startDirectory)
    {
        for (var directory = new DirectoryInfo(startDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, MarkerFileName)))
            {
                return directory.FullName;
            }
        }

        return startDirectory;
    }

    public static string Resolve(string root, string path) => Path.GetFullPath(path, root);
}
