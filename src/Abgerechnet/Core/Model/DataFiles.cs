namespace Abgerechnet.Core.Model;

/// <summary>
/// Common shape of the data files: a <c>"version"</c> field for later migrations and a hook that repairs
/// missing values after loading.
/// </summary>
public interface IDataFile
{
    /// <summary>Format version of the file; see <see cref="Storage.JsonDataFile{T}.CurrentVersion"/>.</summary>
    int Version { get; set; }

    /// <summary>Replaces missing values (older or hand-edited files) with defaults.</summary>
    void Normalize();
}

/// <summary>Content of <c>rechnungen.json</c>.</summary>
public sealed class RechnungenDatei : IDataFile
{
    public int Version { get; set; }

    public List<Rechnung> Rechnungen { get; set; } = [];

    public void Normalize()
    {
        Rechnungen = (Rechnungen ?? []).Where(r => r is not null).ToList();
        foreach (var rechnung in Rechnungen)
            rechnung.Normalize();
    }
}

/// <summary>Content of <c>kunden.json</c>.</summary>
public sealed class KundenDatei : IDataFile
{
    public int Version { get; set; }

    public List<Kunde> Kunden { get; set; } = [];

    public void Normalize()
    {
        Kunden = (Kunden ?? []).Where(k => k is not null).ToList();
        foreach (var kunde in Kunden)
            kunde.Normalize();
    }
}

/// <summary>
/// Content of <c>abgerechnet.json</c>: sender data and invoice settings. The fields follow in issue #3.
/// </summary>
public sealed class Einstellungen : IDataFile
{
    public int Version { get; set; }

    public void Normalize()
    {
    }
}
