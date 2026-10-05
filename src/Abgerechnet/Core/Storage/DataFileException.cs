namespace Abgerechnet.Core.Storage;

/// <summary>What went wrong with a data file. The user interface turns it into a plain-language message.</summary>
public enum DataFileProblem
{
    /// <summary>The file exists but cannot be opened (locked, no permission).</summary>
    Unreadable,

    /// <summary>The file is not valid JSON or does not match the expected structure.</summary>
    Corrupt,

    /// <summary>The file was written by a newer version of Abgerechnet.</summary>
    TooNew,

    /// <summary>The file cannot be saved.</summary>
    NotWritable,
}

/// <summary>
/// A data file cannot be read or written. A file that cannot be read is never overwritten.
/// </summary>
public sealed class DataFileException(string path, DataFileProblem problem, string message, Exception? inner = null)
    : IOException(message, inner)
{
    /// <summary>Full path of the file.</summary>
    public string FilePath { get; } = path;

    public string FileName => Path.GetFileName(FilePath);

    public DataFileProblem Problem { get; } = problem;

    /// <summary>One-based line of a JSON error, if known.</summary>
    public int? Line { get; init; }
}
