namespace WordFrequency;

internal sealed record FileError(string FileName, string Message);

/// <summary>Outcome of one processing step over a set of files.</summary>
internal sealed record ProcessingResult(IReadOnlyList<string> ProcessedFiles, IReadOnlyList<FileError> Errors)
{
    public static bool IsFileError(Exception exception) =>
        exception is System.Xml.XmlException or IOException or UnauthorizedAccessException or InvalidDataException;
}
