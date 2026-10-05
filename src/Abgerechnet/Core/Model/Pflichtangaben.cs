namespace Abgerechnet.Core.Model;

/// <summary>A mandatory detail of an invoice according to § 14 Abs. 4 UStG.</summary>
public enum Pflichtangabe
{
    AbsenderName,
    AbsenderAnschrift,
    Steuernummer,
    EmpfaengerName,
    EmpfaengerAnschrift,
    Rechnungsnummer,
    Leistungszeitraum,
    Leistungsbeschreibung,
    KleinunternehmerHinweis,
}

/// <summary>
/// Finds mandatory details (§ 14 Abs. 4 UStG) that are missing before a PDF is created. The result is only a
/// warning: the user may still create the PDF.
/// </summary>
public static class Pflichtangaben
{
    /// <param name="empfaenger">The customer the invoice goes to; <c>null</c> when none is chosen.</param>
    public static IReadOnlyList<Pflichtangabe> Fehlende(Einstellungen einstellungen, Rechnung rechnung, Kunde? empfaenger)
    {
        var missing = new List<Pflichtangabe>();
        var absender = einstellungen.Absender;

        if (IsEmpty(absender.Firma) && IsEmpty(absender.Name))
            missing.Add(Pflichtangabe.AbsenderName);
        if (IsEmpty(absender.Strasse) || IsEmpty(absender.Plz) || IsEmpty(absender.Ort))
            missing.Add(Pflichtangabe.AbsenderAnschrift);
        if (IsEmpty(absender.Steuernummer) && IsEmpty(absender.UstIdNr))
            missing.Add(Pflichtangabe.Steuernummer);

        if (empfaenger is null || IsEmpty(empfaenger.Firma))
            missing.Add(Pflichtangabe.EmpfaengerName);
        if (empfaenger is null || IsEmpty(empfaenger.Strasse) || IsEmpty(empfaenger.Plz) || IsEmpty(empfaenger.Ort))
            missing.Add(Pflichtangabe.EmpfaengerAnschrift);

        if (IsEmpty(rechnung.Nummer))
            missing.Add(Pflichtangabe.Rechnungsnummer);
        // A line item without its own period uses the period of the invoice.
        if (IsEmpty(rechnung.Zeitraum) && (rechnung.Positionen.Count == 0 || rechnung.Positionen.Any(p => IsEmpty(p.Zeitraum))))
            missing.Add(Pflichtangabe.Leistungszeitraum);
        if (rechnung.Positionen.Count == 0 || rechnung.Positionen.Any(p => IsEmpty(p.Beschreibung)))
            missing.Add(Pflichtangabe.Leistungsbeschreibung);

        if (einstellungen.Rechnung.Kleinunternehmer && IsEmpty(einstellungen.Rechnung.KleinunternehmerHinweis))
            missing.Add(Pflichtangabe.KleinunternehmerHinweis);

        return missing;
    }

    private static bool IsEmpty(string? value) => string.IsNullOrWhiteSpace(value);
}
