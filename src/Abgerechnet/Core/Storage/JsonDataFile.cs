using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Abgerechnet.Core.Model;

namespace Abgerechnet.Core.Storage;

/// <summary>
/// Reads and writes one data file (<c>rechnungen.json</c>, <c>kunden.json</c>, <c>abgerechnet.json</c>) as
/// readable, indented JSON. Writing is safe: the new content goes to a temporary file that then replaces the old
/// one, and the previous version is kept as <c>*.bak.json</c>.
/// </summary>
public static class JsonDataFile
{
    /// <summary>
    /// Format version written into every data file. Raise it when the format changes in a way older versions of
    /// Abgerechnet cannot read, and migrate older files in <see cref="Load{T}"/>.
    /// </summary>
    public const int CurrentVersion = 1;

    internal static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        // Hand-edited files: accept "Nummer" as well as "nummer", comments and trailing commas.
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        // Keep umlauts and "€" readable in the file.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) },
    };

    /// <summary>"rechnungen.json" → "rechnungen.bak.json", next to the file.</summary>
    public static string BackupPath(string path) =>
        Path.Combine(Path.GetDirectoryName(path)!, Path.GetFileNameWithoutExtension(path) + ".bak" + Path.GetExtension(path));

    /// <summary>
    /// Loads the file. A missing file gives an empty data set (<c>Exists</c> is false); nothing is written here.
    /// </summary>
    /// <exception cref="DataFileException">The file cannot be read, is not valid JSON or comes from a newer version.</exception>
    public static (T Data, bool Exists) Load<T>(string path)
        where T : class, IDataFile, new()
    {
        if (!File.Exists(path))
            return (new T { Version = CurrentVersion }, false);

        string json;
        try
        {
            json = File.ReadAllText(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new DataFileException(path, DataFileProblem.Unreadable, ex.Message, ex);
        }

        T? data;
        try
        {
            data = JsonSerializer.Deserialize<T>(json, Options);
        }
        catch (JsonException ex)
        {
            // LineNumber is zero-based.
            var line = ex.LineNumber is { } number ? (int)number + 1 : (int?)null;
            throw new DataFileException(path, DataFileProblem.Corrupt, ex.Message, ex) { Line = line };
        }

        if (data is null)
            throw new DataFileException(path, DataFileProblem.Corrupt, "The file contains null.");

        if (data.Version > CurrentVersion)
            throw new DataFileException(path, DataFileProblem.TooNew, $"Version {data.Version} > {CurrentVersion}.");

        // Version 0 means the field is missing (hand-written file): treat it as the first format.
        // Migrations from older versions go here once CurrentVersion is raised.
        data.Version = CurrentVersion;
        data.Normalize();
        return (data, true);
    }

    /// <summary>A deep copy, e.g. to edit data in a dialog that can be cancelled.</summary>
    public static T Clone<T>(T data)
        where T : class, IDataFile
    {
        var copy = JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(data, Options), Options)!;
        copy.Normalize();
        return copy;
    }

    /// <summary>A deep copy of a single object, e.g. an invoice edited in a form.</summary>
    public static T CloneObject<T>(T value)
        where T : class =>
        JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, Options), Options)!;

    /// <summary>The JSON text of an object as it would be stored, e.g. to detect unsaved changes.</summary>
    public static string ToJson<T>(T value) => JsonSerializer.Serialize(value, Options);

    /// <summary>Saves the data, keeping the previous file as <c>*.bak.json</c>.</summary>
    /// <exception cref="DataFileException">The file cannot be written.</exception>
    public static void Save<T>(string path, T data)
        where T : class, IDataFile
    {
        ArgumentNullException.ThrowIfNull(data);
        data.Version = CurrentVersion;
        var json = JsonSerializer.Serialize(data, Options);

        try
        {
            WriteSafely(path, json);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new DataFileException(path, DataFileProblem.NotWritable, ex.Message, ex);
        }
    }

    /// <summary>
    /// Writes to a temporary file first, so a crash never leaves a half-written file, then replaces the old file
    /// and keeps it as backup.
    /// </summary>
    private static void WriteSafely(string path, string content)
    {
        var tempPath = path + ".tmp";
        File.WriteAllText(tempPath, content);

        if (!File.Exists(path))
        {
            File.Move(tempPath, path);
            return;
        }

        var backupPath = BackupPath(path);
        try
        {
            File.Replace(tempPath, path, backupPath, ignoreMetadataErrors: true);
        }
        catch (IOException)
        {
            // File.Replace can fail on some synchronized or network folders. Same result in two steps.
            File.Copy(path, backupPath, overwrite: true);
            File.Move(tempPath, path, overwrite: true);
        }
    }
}
