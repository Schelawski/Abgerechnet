namespace Abgerechnet.Core.Model;

/// <summary>Columns of the invoice list that can be sorted.</summary>
public enum RechnungsSpalte
{
    Nummer,
    Kunde,
    Zeitraum,
    Datum,
    Status,
    BezahltAm,
    Betrag,
}

/// <summary>
/// Order of the invoice list: by a column, ascending or descending; ties are ordered by number. The default is
/// newest first. Clicking the same column again reverses the order.
/// </summary>
public sealed class RechnungsSortierung
{
    /// <summary>Customers for sorting by name; set it again when the customers were saved.</summary>
    public KundenDatei Kunden { get; set; } = new();

    public RechnungsSpalte Spalte { get; private set; } = RechnungsSpalte.Datum;

    public bool Absteigend { get; private set; } = true;

    /// <summary>Sorts by <paramref name="spalte"/>; the same column again reverses the order.</summary>
    public void Umschalten(RechnungsSpalte spalte)
    {
        if (spalte == Spalte)
        {
            Absteigend = !Absteigend;
            return;
        }

        Spalte = spalte;
        // Numbers, dates and amounts start with the largest, text columns with A.
        Absteigend = spalte is RechnungsSpalte.Nummer or RechnungsSpalte.Datum or RechnungsSpalte.BezahltAm or RechnungsSpalte.Betrag;
    }

    public int Compare(Rechnung a, Rechnung b)
    {
        var result = Spalte switch
        {
            RechnungsSpalte.Nummer => Rechnungsnummer.Vergleich.Compare(a.Nummer, b.Nummer),
            RechnungsSpalte.Kunde => string.Compare(KundenName(a), KundenName(b), StringComparison.CurrentCultureIgnoreCase),
            RechnungsSpalte.Zeitraum => string.Compare(a.Zeitraum, b.Zeitraum, StringComparison.CurrentCultureIgnoreCase),
            RechnungsSpalte.Status => a.Status.CompareTo(b.Status),
            RechnungsSpalte.BezahltAm => Nullable.Compare(a.BezahltAm, b.BezahltAm),
            RechnungsSpalte.Betrag => Rechnungsbetrag.Berechnen(a).Brutto.CompareTo(Rechnungsbetrag.Berechnen(b).Brutto),
            _ => a.Datum.CompareTo(b.Datum),
        };
        // Ties (e.g. same day or same customer): by number, in the same direction.
        if (result == 0)
            result = Rechnungsnummer.Vergleich.Compare(a.Nummer, b.Nummer);
        return Absteigend ? -result : result;
    }

    public IEnumerable<Rechnung> Sortieren(IEnumerable<Rechnung> rechnungen) =>
        rechnungen.Order(Comparer<Rechnung>.Create(Compare));

    private string KundenName(Rechnung rechnung) => rechnung.EmpfaengerAus(Kunden)?.Firma ?? string.Empty;
}
