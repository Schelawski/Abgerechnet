using Abgerechnet.Core.Model;

namespace Abgerechnet.Tests;

public class PflichtangabenTests
{
    private static Einstellungen VollstaendigeEinstellungen() => new()
    {
        Absender = { Firma = "Schmidt IT", Strasse = "Am Markt 3", Plz = "50667", Ort = "Köln", Steuernummer = "214/5678/1234" },
    };

    private static Kunde VollstaendigerKunde() => new() { Firma = "Beispiel AG", Strasse = "Hauptstr. 1", Plz = "10115", Ort = "Berlin" };

    private static Rechnung VollstaendigeRechnung() => new()
    {
        Nummer = "111412",
        Zeitraum = "September 2026",
        Positionen = [new Position { Beschreibung = "Entwicklung", Menge = 10, Einzelpreis = 88 }],
    };

    [Fact]
    public void CompleteInvoice_HasNothingMissing()
    {
        Assert.Empty(Pflichtangaben.Fehlende(VollstaendigeEinstellungen(), VollstaendigeRechnung(), VollstaendigerKunde()));
    }

    [Fact]
    public void EmptySettings_ReportSender()
    {
        var missing = Pflichtangaben.Fehlende(new Einstellungen(), VollstaendigeRechnung(), VollstaendigerKunde());

        Assert.Equal([Pflichtangabe.AbsenderName, Pflichtangabe.AbsenderAnschrift, Pflichtangabe.Steuernummer], missing);
    }

    [Fact]
    public void VatIdInsteadOfTaxNumber_IsEnough()
    {
        var einstellungen = VollstaendigeEinstellungen();
        einstellungen.Absender.Steuernummer = "";
        einstellungen.Absender.UstIdNr = "DE123456789";

        Assert.Empty(Pflichtangaben.Fehlende(einstellungen, VollstaendigeRechnung(), VollstaendigerKunde()));
    }

    [Fact]
    public void NoCustomer_ReportsRecipient()
    {
        var missing = Pflichtangaben.Fehlende(VollstaendigeEinstellungen(), VollstaendigeRechnung(), empfaenger: null);

        Assert.Equal([Pflichtangabe.EmpfaengerName, Pflichtangabe.EmpfaengerAnschrift], missing);
    }

    [Fact]
    public void PeriodOnEveryPosition_IsEnough()
    {
        var rechnung = VollstaendigeRechnung();
        rechnung.Zeitraum = "";
        rechnung.Positionen[0].Zeitraum = "01.–15.09.2026";

        Assert.Empty(Pflichtangaben.Fehlende(VollstaendigeEinstellungen(), rechnung, VollstaendigerKunde()));

        rechnung.Positionen.Add(new Position { Beschreibung = "Support" });
        Assert.Equal([Pflichtangabe.Leistungszeitraum], Pflichtangaben.Fehlende(VollstaendigeEinstellungen(), rechnung, VollstaendigerKunde()));
    }

    [Fact]
    public void MissingNumberAndDescription_AreReported()
    {
        var rechnung = VollstaendigeRechnung();
        rechnung.Nummer = " ";
        rechnung.Positionen[0].Beschreibung = "";

        var missing = Pflichtangaben.Fehlende(VollstaendigeEinstellungen(), rechnung, VollstaendigerKunde());

        Assert.Equal([Pflichtangabe.Rechnungsnummer, Pflichtangabe.Leistungsbeschreibung], missing);
    }

    [Fact]
    public void SmallBusinessWithoutNote_IsReported()
    {
        var einstellungen = VollstaendigeEinstellungen();
        einstellungen.Rechnung.Kleinunternehmer = true;
        einstellungen.Rechnung.KleinunternehmerHinweis = "";

        Assert.Equal([Pflichtangabe.KleinunternehmerHinweis], Pflichtangaben.Fehlende(einstellungen, VollstaendigeRechnung(), VollstaendigerKunde()));
    }
}
