using Abgerechnet.Core.Model;
using Abgerechnet.Core.Vorlagen;

namespace Abgerechnet.Tests;

public class VorlageTests
{
    private static RechnungsDaten Beispiel(bool kleinunternehmer = false)
    {
        var einstellungen = new Einstellungen
        {
            Absender = { Firma = "Schmidt IT", Unterzeile = "Software & Beratung", Name = "Jörg Schmidt", Strasse = "Am Markt 3", Plz = "50667", Ort = "Köln", Steuernummer = "214/5678/1234" },
            Bank = { Bank = "Beispielbank", Iban = "DE89 3704 0044 0532 0130 00", Bic = "COBADEFFXXX" },
            Rechnung = { Kleinunternehmer = kleinunternehmer, ZahlungszielTage = 14 },
        };
        var kunde = new Kunde { Firma = "Nordlicht GmbH", Ansprechpartner = "Frau Berg", Strasse = "Hafenstr. 7", Plz = "20457", Ort = "Hamburg" };
        var rechnung = new Rechnung
        {
            Nummer = "2026-007",
            Datum = new DateOnly(2026, 10, 1),
            Zeitraum = "September 2026",
            Projekt = "Kundenportal <Relaunch>",
            Kleinunternehmer = kleinunternehmer,
            Umsatzsteuersatz = 19,
            Positionen =
            [
                new Position { Beschreibung = "Softwareentwicklung", Detail = "Sprint 17\nTicket 42", Menge = 152.5m, Einheit = "Std.", Einzelpreis = 88m },
                new Position { Zeitraum = "15.09.2026", Beschreibung = "Workshop", Menge = 1, Einheit = "pauschal", Einzelpreis = 1200m },
            ],
        };
        return new RechnungsDaten(einstellungen, rechnung, kunde);
    }

    [Fact]
    public void SimplePlaceholders_AreReplaced()
    {
        var ergebnis = Vorlage.Ausfuellen("<h1>{{absender_firma}}</h1><p>Rechnung {{rechnung_nummer}} vom {{rechnung_datum}}, fällig am {{faellig_am}}</p>", Beispiel());

        Assert.Equal("<h1>Schmidt IT</h1><p>Rechnung 2026-007 vom 01.10.2026, fällig am 15.10.2026</p>", ergebnis.Html);
        Assert.Empty(ergebnis.UnbekanntePlatzhalter);
    }

    [Fact]
    public void SpacesAndUpperCase_AreAllowed()
    {
        Assert.Equal("Hamburg", Vorlage.Ausfuellen("{{ Kunde_Ort }}", Beispiel()).Html);
    }

    [Fact]
    public void Values_AreHtmlEscaped()
    {
        var html = Vorlage.Ausfuellen("{{absender_unterzeile}} | {{projekt}}", Beispiel()).Html;

        Assert.Equal("Software &amp; Beratung | Kundenportal &lt;Relaunch&gt;", html);
    }

    [Fact]
    public void Umlauts_StayReadable_QuotesAreEscaped()
    {
        var daten = Beispiel();
        daten.Einstellungen.Absender.Name = "Jörg \"JS\" O'Neill €";

        Assert.Equal("Jörg &quot;JS&quot; O&#39;Neill €", Vorlage.Ausfuellen("{{absender_name}}", daten).Html);
    }

    [Fact]
    public void Amounts_UseGermanFormatWithNonBreakingSpace()
    {
        var html = Vorlage.Ausfuellen("{{netto}}|{{ust_satz}}|{{ust_betrag}}|{{brutto}}", Beispiel()).Html;

        // 152,5 × 88 = 13.420,00 + 1.200,00 = 14.620,00; 19 % = 2.777,80
        Assert.Equal("14.620,00&nbsp;€|19&nbsp;%|2.777,80&nbsp;€|17.397,80&nbsp;€", html);
    }

    [Fact]
    public void KundeAnschrift_OneLinePerPart()
    {
        Assert.Equal("Nordlicht GmbH<br>Frau Berg<br>Hafenstr. 7<br>20457 Hamburg", Vorlage.Ausfuellen("{{kunde_anschrift}}", Beispiel()).Html);
    }

    [Fact]
    public void WithoutCustomerAndPaymentTerm_FieldsAreEmpty()
    {
        var daten = Beispiel() with { Empfaenger = null };
        daten.Einstellungen.Rechnung.ZahlungszielTage = 0;

        var html = Vorlage.Ausfuellen("[{{kunde_name}}][{{kunde_anschrift}}][{{faellig_am}}][{{zahlungsziel_tage}}][{{absender_land}}]", daten).Html;

        Assert.Equal("[][][][][]", html);
    }

    [Fact]
    public void Kontoinhaber_DefaultsToTheSender()
    {
        Assert.Equal("Schmidt IT", Vorlage.Ausfuellen("{{kontoinhaber}}", Beispiel()).Html);
    }

    [Fact]
    public void UnknownPlaceholders_StayAndAreListed()
    {
        var ergebnis = Vorlage.Ausfuellen("{{kundenname}} {{rechnung_nummer}} {{kundenname}} {{ Logo }}", Beispiel());

        Assert.Equal("{{kundenname}} 2026-007 {{kundenname}} {{ Logo }}", ergebnis.Html);
        Assert.Equal(["{{kundenname}}", "{{ Logo }}"], ergebnis.UnbekanntePlatzhalter);
    }

    [Fact]
    public void Comments_AreLeftAlone()
    {
        const string vorlage = "<!-- Beispiel: {{absender_firma}} oder {{eigener_platzhalter}} -->\n<p>{{absender_firma}}</p><!-- {{iban}} -->";

        var ergebnis = Vorlage.Ausfuellen(vorlage, Beispiel());

        Assert.Equal("<!-- Beispiel: {{absender_firma}} oder {{eigener_platzhalter}} -->\n<p>Schmidt IT</p><!-- {{iban}} -->", ergebnis.Html);
        Assert.Empty(ergebnis.UnbekanntePlatzhalter);
    }

    [Fact]
    public void SmallBusiness_HasNoVatAndTheNote()
    {
        var daten = Beispiel(kleinunternehmer: true);

        var ergebnis = Vorlage.Ausfuellen("{{ust_satz}}|{{ust_betrag}}|{{brutto}}|{{steuerhinweis}}|{{summen_tabelle}}", daten);

        Assert.StartsWith("|0,00&nbsp;€|14.620,00&nbsp;€|Gemäß § 19 UStG wird keine Umsatzsteuer berechnet.|", ergebnis.Html);
        Assert.Contains("class=\"brutto\"", ergebnis.Html);
        Assert.DoesNotContain("class=\"netto\"", ergebnis.Html);
        Assert.DoesNotContain("class=\"ust\"", ergebnis.Html);
    }

    [Fact]
    public void RegularInvoice_HasNoTaxNote()
    {
        var html = Vorlage.Ausfuellen("[{{steuerhinweis}}]{{summen_tabelle}}", Beispiel()).Html;

        Assert.StartsWith("[]", html);
        Assert.Contains("<tr class=\"netto\"><th>Nettobetrag</th><td>14.620,00&nbsp;€</td></tr>", html);
        Assert.Contains("<tr class=\"ust\"><th>Umsatzsteuer 19&nbsp;%</th><td>2.777,80&nbsp;€</td></tr>", html);
        Assert.Contains("<tr class=\"brutto\"><th>Rechnungsbetrag</th><td>17.397,80&nbsp;€</td></tr>", html);
    }

    [Fact]
    public void PositionenTabelle_HasClassesAndValues()
    {
        var html = Vorlage.Ausfuellen("{{positionen_tabelle}}", Beispiel()).Html;

        Assert.StartsWith("<table class=\"positionen\">", html);
        foreach (var klasse in new[] { "pos-nr", "pos-zeitraum", "pos-beschreibung", "pos-menge", "pos-einheit", "pos-preis", "pos-betrag" })
            Assert.Contains($"<th class=\"{klasse}\">", html);
        Assert.Equal(2, html.Split("<tr class=\"position\">").Length - 1);

        // First item: no own period → period of the invoice; detail with line break; quantity without trailing zeros.
        Assert.Contains("<td class=\"pos-zeitraum\">September 2026</td>", html);
        Assert.Contains("<td class=\"pos-beschreibung\">Softwareentwicklung<div class=\"pos-detail\">Sprint 17<br>Ticket 42</div></td>", html);
        Assert.Contains("<td class=\"pos-menge\">152,5</td>", html);
        Assert.Contains("<td class=\"pos-preis\">88,00&nbsp;€</td>", html);
        Assert.Contains("<td class=\"pos-betrag\">13.420,00&nbsp;€</td>", html);

        // Second item: own period, no detail element.
        Assert.Contains("<td class=\"pos-zeitraum\">15.09.2026</td>", html);
        Assert.Contains("<td class=\"pos-beschreibung\">Workshop</td>", html);
        Assert.Contains("<td class=\"pos-menge\">1</td>", html);
        Assert.Contains("<td class=\"pos-nr\">2</td>", html);
    }

    [Fact]
    public void PositionenTabelle_EscapesUserText()
    {
        var daten = Beispiel();
        daten.Rechnung.Positionen[0].Beschreibung = "<script>alert(1)</script>";

        var html = Vorlage.Ausfuellen("{{positionen_tabelle}}", daten).Html;

        Assert.DoesNotContain("<script>", html);
        Assert.Contains("&lt;script&gt;", html);
    }

    [Fact]
    public void Katalog_MatchesTheValues()
    {
        var werte = Platzhalter.Werte(Beispiel());

        Assert.Equal(Platzhalter.Alle.Select(p => p.Name).Order(), werte.Keys.Order());
        Assert.Equal(Platzhalter.Alle.Count, Platzhalter.Alle.Select(p => p.Name).Distinct().Count());
        Assert.All(Platzhalter.Alle, p => Assert.False(string.IsNullOrWhiteSpace(p.Beschreibung)));
        Assert.Equal("{{absender_firma}}", Platzhalter.Alle[0].Schreibweise);
    }

    [Fact]
    public void Base_PointsToTheTemplateFolder()
    {
        using var temp = new TempFolder();
        var ordner = Directory.CreateDirectory(Path.Combine(temp.Path, "Vorlagen mit Leerzeichen")).FullName;

        var html = Vorlage.Ausfuellen("<html><head><title>x</title></head><body><img src=\"logo.png\"></body></html>", Beispiel(), ordner).Html;

        var expected = new Uri(ordner + Path.DirectorySeparatorChar).AbsoluteUri;
        Assert.Contains($"<head><base href=\"{expected}\"><title>", html);
        Assert.EndsWith("/", expected);
        Assert.Contains("%20", expected);
    }

    [Fact]
    public void Base_OwnBaseIsKept_AndWorksWithoutHead()
    {
        const string eigene = "<head><base href=\"https://example.org/\"></head>";
        Assert.Equal(eigene, Vorlage.Ausfuellen(eigene, Beispiel(), "C:\\Vorlagen").Html);

        Assert.StartsWith("<base href=\"file:///C:/Vorlagen/\">", Vorlage.Ausfuellen("<p>x</p>", Beispiel(), "C:\\Vorlagen").Html);
    }
}
