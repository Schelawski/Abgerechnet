namespace Abgerechnet.Core.Model;

/// <summary>
/// Common shape of the data files: a <c>"version"</c> field for later migrations and a hook that repairs
/// missing values after loading.
/// </summary>
public interface IDataFile
{
    /// <summary>Format version of the file; see <see cref="Storage.JsonDataFile.CurrentVersion"/>.</summary>
    int Version { get; set; }

    /// <summary>Replaces missing values (older or hand-edited files) with defaults.</summary>
    void Normalize();
}

/// <summary>Content of <c>rechnungen.json</c>.</summary>
public sealed class RechnungenDatei : IDataFile
{
    public int Version { get; set; }

    public List<Rechnung> Rechnungen { get; set; } = [];

    /// <summary>True when at least one invoice is addressed to the customer; such a customer cannot be deleted.</summary>
    public bool VerwendetKunde(Guid kundeId) => Rechnungen.Any(r => r.KundeId == kundeId);

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

    /// <summary>The customer with this id, or <c>null</c>.</summary>
    public Kunde? Finden(Guid? id) => id is { } value ? Kunden.Find(k => k.Id == value) : null;

    /// <summary>Customers sorted by company, as shown in lists and drop-downs.</summary>
    public IReadOnlyList<Kunde> Sortiert() =>
        Kunden.OrderBy(k => k.Firma, StringComparer.Create(System.Globalization.CultureInfo.GetCultureInfo("de-DE"), ignoreCase: true)).ToList();

    public void Normalize()
    {
        Kunden = (Kunden ?? []).Where(k => k is not null).ToList();
        foreach (var kunde in Kunden)
            kunde.Normalize();
    }
}
