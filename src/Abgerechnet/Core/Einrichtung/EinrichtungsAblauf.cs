using Abgerechnet.Core.Storage;

namespace Abgerechnet.Core.Einrichtung;

/// <summary>Pages of the welcome wizard, in order.</summary>
public enum EinrichtungsSchritt
{
    /// <summary>What Abgerechnet does; the data stays on this computer.</summary>
    Willkommen,

    /// <summary>Choose or create the invoice folder.</summary>
    Ordner,

    /// <summary>Sender's data, small business yes/no.</summary>
    MeineDaten,

    /// <summary>Choose one of the built-in templates, optionally a logo.</summary>
    Vorlage,

    /// <summary>"Erste Rechnung erstellen", optionally copy Abgerechnet out of the downloads folder.</summary>
    Fertig,
}

/// <summary>What the chosen folder is, shown below the path on the folder page.</summary>
public enum OrdnerArt
{
    /// <summary>The folder does not exist yet; it will be created.</summary>
    Neu,

    /// <summary>The folder exists and is empty.</summary>
    Leer,

    /// <summary>The folder holds other files; Abgerechnet adds its own.</summary>
    AndereDateien,

    /// <summary>The folder already holds Abgerechnet data; it is opened and the next two pages are skipped.</summary>
    Rechnungsordner,
}

/// <summary>
/// Navigation rules of the welcome wizard (issue #11). Kept free of UI code so the rules can be tested.
/// With an existing invoice folder the pages "Meine Daten" and "Vorlage" are skipped – that data is already there.
/// </summary>
public static class EinrichtungsAblauf
{
    /// <summary>The page after <paramref name="aktuell"/>.</summary>
    public static EinrichtungsSchritt Weiter(EinrichtungsSchritt aktuell, bool bestehenderOrdner) => aktuell switch
    {
        EinrichtungsSchritt.Willkommen => EinrichtungsSchritt.Ordner,
        EinrichtungsSchritt.Ordner => bestehenderOrdner ? EinrichtungsSchritt.Fertig : EinrichtungsSchritt.MeineDaten,
        EinrichtungsSchritt.MeineDaten => EinrichtungsSchritt.Vorlage,
        _ => EinrichtungsSchritt.Fertig,
    };

    /// <summary>The page before <paramref name="aktuell"/>, skipping the same pages as <see cref="Weiter"/>.</summary>
    public static EinrichtungsSchritt Zurueck(EinrichtungsSchritt aktuell, bool bestehenderOrdner) => aktuell switch
    {
        EinrichtungsSchritt.Fertig => bestehenderOrdner ? EinrichtungsSchritt.Ordner : EinrichtungsSchritt.Vorlage,
        EinrichtungsSchritt.Vorlage => EinrichtungsSchritt.MeineDaten,
        EinrichtungsSchritt.MeineDaten => EinrichtungsSchritt.Ordner,
        _ => EinrichtungsSchritt.Willkommen,
    };

    /// <summary>Number of pages shown in "Schritt n von m".</summary>
    public static int Anzahl(bool bestehenderOrdner) => bestehenderOrdner ? 3 : 5;

    /// <summary>Position of <paramref name="schritt"/> in "Schritt n von m", starting at 1.</summary>
    public static int Nummer(EinrichtungsSchritt schritt, bool bestehenderOrdner) =>
        bestehenderOrdner && schritt == EinrichtungsSchritt.Fertig ? 3 : (int)schritt + 1;

    /// <summary>
    /// The suggested invoice folder: "Rechnungen" in the documents. If that folder already holds other files (e.g.
    /// invoices written by hand), "Rechnungen (Abgerechnet)" is suggested instead, so nothing gets mixed up.
    /// </summary>
    public static string OrdnerVorschlag(string dokumente)
    {
        var rechnungen = Path.Combine(dokumente, "Rechnungen");
        return ArtDesOrdners(rechnungen) == OrdnerArt.AndereDateien
            ? Path.Combine(dokumente, "Rechnungen (Abgerechnet)")
            : rechnungen;
    }

    /// <summary>What <paramref name="pfad"/> is: new, empty, with other files or an invoice folder.</summary>
    public static OrdnerArt ArtDesOrdners(string pfad)
    {
        if (!Directory.Exists(pfad))
            return OrdnerArt.Neu;
        if (DataFolder.ContainsData(pfad))
            return OrdnerArt.Rechnungsordner;
        try
        {
            return Directory.EnumerateFileSystemEntries(pfad).Any() ? OrdnerArt.AndereDateien : OrdnerArt.Leer;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return OrdnerArt.AndereDateien;
        }
    }
}
