namespace Abgerechnet.Core.Model;

/// <summary>Sum and number of invoices, as shown on a tile of the main window.</summary>
public readonly record struct Summe(decimal Betrag, int Anzahl)
{
    public static Summe operator +(Summe a, Summe b) => new(a.Betrag + b.Betrag, a.Anzahl + b.Anzahl);
}

/// <summary>
/// The tiles of the main window for a selection of invoices (e.g. one year). "Gesamt" counts only issued invoices
/// (open and paid); drafts and cancelled invoices are not part of it, drafts are counted separately.
/// </summary>
public sealed record Uebersicht(Summe Offen, Summe Bezahlt, int Entwuerfe)
{
    public Summe Gesamt => Offen + Bezahlt;

    public static Uebersicht Berechnen(IEnumerable<Rechnung> rechnungen)
    {
        var offen = new Summe(0, 0);
        var bezahlt = new Summe(0, 0);
        var entwuerfe = 0;
        foreach (var rechnung in rechnungen)
        {
            switch (rechnung.Status)
            {
                case RechnungsStatus.Offen:
                    offen += new Summe(Rechnungsbetrag.Berechnen(rechnung).Brutto, 1);
                    break;
                case RechnungsStatus.Bezahlt:
                    bezahlt += new Summe(Rechnungsbetrag.Berechnen(rechnung).Brutto, 1);
                    break;
                case RechnungsStatus.Entwurf:
                    entwuerfe++;
                    break;
            }
        }
        return new Uebersicht(offen, bezahlt, entwuerfe);
    }

    /// <summary>Years that have invoices, plus the current year, newest first (for the year filter).</summary>
    public static IReadOnlyList<int> Jahre(IEnumerable<Rechnung> rechnungen, int aktuellesJahr) =>
        rechnungen.Select(r => r.Datum.Year).Append(aktuellesJahr).Distinct().OrderDescending().ToList();
}
