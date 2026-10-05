using System.Globalization;
using System.Text;
using Abgerechnet.Core.Model;

namespace Abgerechnet.Core.Vorlagen;

/// <summary>Everything a template is filled with.</summary>
/// <param name="Empfaenger">The recipient (frozen copy or current customer); <c>null</c> when none is chosen.</param>
public sealed record RechnungsDaten(Einstellungen Einstellungen, Rechnung Rechnung, Kunde? Empfaenger);

/// <summary>Description of one placeholder, for the help and the AI prompt (issue #10).</summary>
/// <param name="Name">Name without braces, e.g. "absender_firma".</param>
/// <param name="Gruppe">"Absender", "Kunde", "Rechnung", "Beträge" or "Blöcke".</param>
/// <param name="IstBlock">True for ready-made HTML (tables); false for plain text.</param>
public sealed record PlatzhalterInfo(string Name, string Gruppe, string Beschreibung, bool IstBlock = false)
{
    /// <summary>"{{absender_firma}}".</summary>
    public string Schreibweise => "{{" + Name + "}}";
}

/// <summary>
/// The placeholders of the invoice templates and their values. Text values are HTML-escaped (line breaks become
/// <c>&lt;br&gt;</c>); blocks are ready-made HTML with CSS classes that the template styles.
/// </summary>
public static class Platzhalter
{
    private static readonly CultureInfo German = CultureInfo.GetCultureInfo("de-DE");

    /// <summary>All placeholders, grouped, in the order the help lists them.</summary>
    public static readonly IReadOnlyList<PlatzhalterInfo> Alle =
    [
        new("absender_firma", "Absender", "Ihre Firma"),
        new("absender_unterzeile", "Absender", "Zeile unter der Firma, z. B. Ihr Tätigkeitsfeld"),
        new("absender_name", "Absender", "Ihr Name"),
        new("absender_strasse", "Absender", "Straße und Hausnummer"),
        new("absender_plz", "Absender", "Postleitzahl"),
        new("absender_ort", "Absender", "Ort"),
        new("absender_land", "Absender", "Land"),
        new("absender_telefon", "Absender", "Telefon"),
        new("absender_email", "Absender", "E-Mail"),
        new("absender_web", "Absender", "Website"),
        new("steuernummer", "Absender", "Steuernummer"),
        new("ust_idnr", "Absender", "Ihre USt-IdNr."),
        new("kontoinhaber", "Absender", "Kontoinhaber (leer: Ihre Firma bzw. Ihr Name)"),
        new("bank", "Absender", "Name der Bank"),
        new("iban", "Absender", "IBAN in Viererblöcken"),
        new("bic", "Absender", "BIC"),

        new("kunde_name", "Kunde", "Firma bzw. Name des Kunden"),
        new("kunde_ansprechpartner", "Kunde", "Ansprechpartner"),
        new("kunde_strasse", "Kunde", "Straße und Hausnummer"),
        new("kunde_plz", "Kunde", "Postleitzahl"),
        new("kunde_ort", "Kunde", "Ort"),
        new("kunde_land", "Kunde", "Land"),
        new("kunde_ust_idnr", "Kunde", "USt-IdNr. des Kunden"),
        new("kunde_anschrift", "Kunde", "Die ganze Anschrift, eine Zeile pro Angabe (leere Angaben fallen weg)"),

        new("rechnung_nummer", "Rechnung", "Rechnungsnummer"),
        new("rechnung_datum", "Rechnung", "Rechnungsdatum, z. B. 01.10.2026"),
        new("leistungszeitraum", "Rechnung", "Leistungszeitraum, z. B. September 2026"),
        new("projekt", "Rechnung", "Projekt / Betreff"),
        new("faellig_am", "Rechnung", "Fälligkeitsdatum (Rechnungsdatum + Zahlungsziel); leer ohne Zahlungsziel"),
        new("zahlungsziel_tage", "Rechnung", "Zahlungsziel in Tagen; leer ohne Zahlungsziel"),

        new("netto", "Beträge", "Nettobetrag, z. B. 1.234,50 €"),
        new("ust_satz", "Beträge", "Umsatzsteuersatz, z. B. 19 %; leer bei Kleinunternehmern"),
        new("ust_betrag", "Beträge", "Umsatzsteuer, z. B. 234,56 €"),
        new("brutto", "Beträge", "Rechnungsbetrag"),
        new("steuerhinweis", "Beträge", "Hinweis zur Steuer, z. B. der § 19-Hinweis bei Kleinunternehmern; sonst leer"),

        new("positionen_tabelle", "Blöcke", "Alle Positionen als Tabelle (CSS-Klasse „positionen“)", IstBlock: true),
        new("summen_tabelle", "Blöcke", "Netto, Umsatzsteuer und Rechnungsbetrag als Tabelle (CSS-Klasse „summen“)", IstBlock: true),
    ];

    /// <summary>The values for all placeholders, ready to insert into HTML.</summary>
    public static IReadOnlyDictionary<string, string> Werte(RechnungsDaten daten)
    {
        var (einstellungen, rechnung, kunde) = daten;
        var a = einstellungen.Absender;
        var b = einstellungen.Bank;
        var betrag = Rechnungsbetrag.Berechnen(rechnung);
        var zahlungsziel = einstellungen.Rechnung.ZahlungszielTage;
        kunde ??= new Kunde();

        var text = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["absender_firma"] = a.Firma,
            ["absender_unterzeile"] = a.Unterzeile,
            ["absender_name"] = a.Name,
            ["absender_strasse"] = a.Strasse,
            ["absender_plz"] = a.Plz,
            ["absender_ort"] = a.Ort,
            ["absender_land"] = a.Land,
            ["absender_telefon"] = a.Telefon,
            ["absender_email"] = a.Email,
            ["absender_web"] = a.Website,
            ["steuernummer"] = a.Steuernummer,
            ["ust_idnr"] = a.UstIdNr,
            ["kontoinhaber"] = string.IsNullOrWhiteSpace(b.Kontoinhaber) ? a.Anzeigename : b.Kontoinhaber,
            ["bank"] = b.Bank,
            ["iban"] = b.Iban,
            ["bic"] = b.Bic,

            ["kunde_name"] = kunde.Firma,
            ["kunde_ansprechpartner"] = kunde.Ansprechpartner,
            ["kunde_strasse"] = kunde.Strasse,
            ["kunde_plz"] = kunde.Plz,
            ["kunde_ort"] = kunde.Ort,
            ["kunde_land"] = kunde.Land,
            ["kunde_ust_idnr"] = kunde.UstIdNr,
            ["kunde_anschrift"] = string.Join('\n', kunde.Anschrift()),

            ["rechnung_nummer"] = rechnung.Nummer,
            ["rechnung_datum"] = Datum(rechnung.Datum),
            ["leistungszeitraum"] = rechnung.Zeitraum,
            ["projekt"] = rechnung.Projekt,
            ["faellig_am"] = zahlungsziel > 0 ? Datum(rechnung.Datum.AddDays(zahlungsziel)) : string.Empty,
            ["zahlungsziel_tage"] = zahlungsziel > 0 ? zahlungsziel.ToString(German) : string.Empty,

            ["steuerhinweis"] = rechnung.Kleinunternehmer ? einstellungen.Rechnung.KleinunternehmerHinweis : string.Empty,
        };

        var werte = text.ToDictionary(e => e.Key, e => Escape(e.Value), StringComparer.OrdinalIgnoreCase);
        // Already HTML (with a non-breaking space before € and %), not escaped again.
        werte["netto"] = Betrag(betrag.Netto);
        werte["ust_satz"] = rechnung.Kleinunternehmer ? string.Empty : Satz(rechnung.Umsatzsteuersatz);
        werte["ust_betrag"] = Betrag(betrag.Umsatzsteuer);
        werte["brutto"] = Betrag(betrag.Brutto);
        werte["positionen_tabelle"] = PositionenTabelle(rechnung);
        werte["summen_tabelle"] = SummenTabelle(rechnung, betrag);
        return werte;
    }

    /// <summary>
    /// The line items as a table. Every cell has a class, so the template can style, rearrange or hide columns:
    /// <c>.pos-nr</c>, <c>.pos-zeitraum</c>, <c>.pos-beschreibung</c> (with <c>.pos-detail</c> inside),
    /// <c>.pos-menge</c>, <c>.pos-einheit</c>, <c>.pos-preis</c>, <c>.pos-betrag</c>.
    /// </summary>
    internal static string PositionenTabelle(Rechnung rechnung)
    {
        var html = new StringBuilder();
        html.AppendLine("<table class=\"positionen\">");
        html.AppendLine("  <thead>");
        html.AppendLine("    <tr>");
        html.AppendLine("      <th class=\"pos-nr\">Pos.</th>");
        html.AppendLine("      <th class=\"pos-zeitraum\">Zeitraum</th>");
        html.AppendLine("      <th class=\"pos-beschreibung\">Beschreibung</th>");
        html.AppendLine("      <th class=\"pos-menge\">Menge</th>");
        html.AppendLine("      <th class=\"pos-einheit\">Einheit</th>");
        html.AppendLine("      <th class=\"pos-preis\">Einzelpreis</th>");
        html.AppendLine("      <th class=\"pos-betrag\">Betrag</th>");
        html.AppendLine("    </tr>");
        html.AppendLine("  </thead>");
        html.AppendLine("  <tbody>");
        var nr = 0;
        foreach (var position in rechnung.Positionen)
        {
            nr++;
            // A line item without its own period has the period of the invoice (as in the web tool).
            var zeitraum = string.IsNullOrWhiteSpace(position.Zeitraum) ? rechnung.Zeitraum : position.Zeitraum;
            html.AppendLine("    <tr class=\"position\">");
            html.AppendLine($"      <td class=\"pos-nr\">{nr}</td>");
            html.AppendLine($"      <td class=\"pos-zeitraum\">{Escape(zeitraum)}</td>");
            html.Append($"      <td class=\"pos-beschreibung\">{Escape(position.Beschreibung)}");
            if (!string.IsNullOrWhiteSpace(position.Detail))
                html.Append($"<div class=\"pos-detail\">{Escape(position.Detail)}</div>");
            html.AppendLine("</td>");
            html.AppendLine($"      <td class=\"pos-menge\">{Menge(position.Menge)}</td>");
            html.AppendLine($"      <td class=\"pos-einheit\">{Escape(position.Einheit)}</td>");
            html.AppendLine($"      <td class=\"pos-preis\">{Betrag(position.Einzelpreis)}</td>");
            html.AppendLine($"      <td class=\"pos-betrag\">{Betrag(Rechnungsbetrag.Positionsbetrag(position))}</td>");
            html.AppendLine("    </tr>");
        }
        html.AppendLine("  </tbody>");
        html.Append("</table>");
        return html.ToString();
    }

    /// <summary>
    /// The totals as a table with the rows <c>.netto</c>, <c>.ust</c> and <c>.brutto</c>. Small businesses get only the
    /// <c>.brutto</c> row.
    /// </summary>
    internal static string SummenTabelle(Rechnung rechnung, Rechnungsbetrag betrag)
    {
        var html = new StringBuilder();
        html.AppendLine("<table class=\"summen\">");
        if (!rechnung.Kleinunternehmer)
        {
            html.AppendLine($"  <tr class=\"netto\"><th>Nettobetrag</th><td>{Betrag(betrag.Netto)}</td></tr>");
            html.AppendLine($"  <tr class=\"ust\"><th>Umsatzsteuer {Satz(rechnung.Umsatzsteuersatz)}</th><td>{Betrag(betrag.Umsatzsteuer)}</td></tr>");
        }
        html.AppendLine($"  <tr class=\"brutto\"><th>Rechnungsbetrag</th><td>{Betrag(betrag.Brutto)}</td></tr>");
        html.Append("</table>");
        return html.ToString();
    }

    /// <summary>
    /// HTML-escapes text; line breaks become &lt;br&gt;. Only the five special characters are replaced, so umlauts
    /// and "€" stay readable in the generated HTML (WebUtility.HtmlEncode would write "&amp;#228;" for "ä").
    /// </summary>
    internal static string Escape(string? text)
    {
        var builder = new StringBuilder();
        foreach (var c in (text ?? string.Empty).Trim().ReplaceLineEndings("\n"))
        {
            builder.Append(c switch
            {
                '&' => "&amp;",
                '<' => "&lt;",
                '>' => "&gt;",
                '"' => "&quot;",
                '\'' => "&#39;",
                '\n' => "<br>",
                _ => c.ToString(),
            });
        }
        return builder.ToString();
    }

    // Non-breaking space between number and unit, so "1.234,50 €" is never split across lines.
    internal static string Betrag(decimal betrag) => betrag.ToString("#,##0.00", German) + "&nbsp;€";

    internal static string Menge(decimal menge) => menge.ToString("#,##0.##", German);

    internal static string Satz(decimal satz) => satz.ToString("0.##", German) + "&nbsp;%";

    internal static string Datum(DateOnly datum) => datum.ToString("dd.MM.yyyy", German);
}
