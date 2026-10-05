using Abgerechnet.Core.Model;

namespace Abgerechnet.Core.Storage;

/// <summary>
/// The invoice folder chosen by the user. Everything lives in it, so it can be backed up or synchronized
/// (OneDrive, Dropbox) as a whole:
/// <code>
/// Rechnungsordner/
/// ├── abgerechnet.json   sender data and invoice settings
/// ├── kunden.json        customers
/// ├── rechnungen.json    invoices with their line items
/// ├── Vorlagen/          HTML templates and logo
/// └── PDF/               created invoices
/// </code>
/// </summary>
public sealed class DataFolder
{
    public const string EinstellungenFileName = "abgerechnet.json";
    public const string KundenFileName = "kunden.json";
    public const string RechnungenFileName = "rechnungen.json";
    public const string VorlagenFolderName = "Vorlagen";
    public const string PdfFolderName = "PDF";

    private DataFolder(string path, Einstellungen einstellungen, KundenDatei kunden, RechnungenDatei rechnungen)
    {
        FolderPath = path;
        Einstellungen = einstellungen;
        Kunden = kunden;
        Rechnungen = rechnungen;
    }

    /// <summary>Full path of the folder.</summary>
    public string FolderPath { get; }

    public Einstellungen Einstellungen { get; private set; }

    public KundenDatei Kunden { get; private set; }

    public RechnungenDatei Rechnungen { get; }

    public string EinstellungenPath => Path.Combine(FolderPath, EinstellungenFileName);

    public string KundenPath => Path.Combine(FolderPath, KundenFileName);

    public string RechnungenPath => Path.Combine(FolderPath, RechnungenFileName);

    public string VorlagenPath => Path.Combine(FolderPath, VorlagenFolderName);

    public string PdfPath => Path.Combine(FolderPath, PdfFolderName);

    /// <summary>True when the folder already holds Abgerechnet data.</summary>
    public static bool ContainsData(string path) =>
        File.Exists(Path.Combine(path, EinstellungenFileName)) || File.Exists(Path.Combine(path, RechnungenFileName));

    /// <summary>
    /// Opens the folder. Missing data files and subfolders are created, but only after all existing files were
    /// read successfully – a damaged file stops the opening and nothing in the folder is changed.
    /// </summary>
    /// <exception cref="DirectoryNotFoundException">The folder does not exist.</exception>
    /// <exception cref="DataFileException">A data file cannot be read or created.</exception>
    public static DataFolder Open(string path)
    {
        path = Path.GetFullPath(path);
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException(path);

        var einstellungen = JsonDataFile.Load<Einstellungen>(Path.Combine(path, EinstellungenFileName));
        var kunden = JsonDataFile.Load<KundenDatei>(Path.Combine(path, KundenFileName));
        var rechnungen = JsonDataFile.Load<RechnungenDatei>(Path.Combine(path, RechnungenFileName));

        var folder = new DataFolder(path, einstellungen.Data, kunden.Data, rechnungen.Data);
        try
        {
            Directory.CreateDirectory(folder.VorlagenPath);
            Directory.CreateDirectory(folder.PdfPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new DataFileException(path, DataFileProblem.NotWritable, ex.Message, ex);
        }

        if (!einstellungen.Exists)
            folder.SaveEinstellungen();
        if (!kunden.Exists)
            folder.SaveKunden();
        if (!rechnungen.Exists)
            folder.SaveRechnungen();

        return folder;
    }

    /// <exception cref="DataFileException">The file cannot be written.</exception>
    public void SaveEinstellungen() => JsonDataFile.Save(EinstellungenPath, Einstellungen);

    /// <summary>
    /// Saves changed settings and uses them from now on. If saving fails, <see cref="Einstellungen"/> keeps the
    /// previous values.
    /// </summary>
    /// <exception cref="DataFileException">The file cannot be written.</exception>
    public void SaveEinstellungen(Einstellungen changed)
    {
        ArgumentNullException.ThrowIfNull(changed);
        JsonDataFile.Save(EinstellungenPath, changed);
        Einstellungen = changed;
    }

    /// <exception cref="DataFileException">The file cannot be written.</exception>
    public void SaveKunden() => JsonDataFile.Save(KundenPath, Kunden);

    /// <summary>
    /// Saves changed customers and uses them from now on. If saving fails, <see cref="Kunden"/> keeps the
    /// previous values.
    /// </summary>
    /// <exception cref="DataFileException">The file cannot be written.</exception>
    public void SaveKunden(KundenDatei changed)
    {
        ArgumentNullException.ThrowIfNull(changed);
        JsonDataFile.Save(KundenPath, changed);
        Kunden = changed;
    }

    /// <exception cref="DataFileException">The file cannot be written.</exception>
    public void SaveRechnungen() => JsonDataFile.Save(RechnungenPath, Rechnungen);
}
