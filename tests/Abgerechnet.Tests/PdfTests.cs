using Abgerechnet.Core.Model;
using Abgerechnet.Core.Storage;
using Abgerechnet.Core.Vorlagen;

namespace Abgerechnet.Tests;

public class PdfTests
{
    [Fact]
    public void PdfErzeugt_DraftBecomesOpen_AndTheAddressIsFrozen()
    {
        var kunde = new Kunde { Firma = "Nordlicht GmbH", Ort = "Hamburg" };
        var rechnung = new Rechnung { KundeId = kunde.Id };

        rechnung.PdfErzeugt("2026-001_Nordlicht_2026-10-05.pdf", kunde);
        kunde.Ort = "Bremen";

        Assert.Equal(RechnungsStatus.Offen, rechnung.Status);
        Assert.Equal("2026-001_Nordlicht_2026-10-05.pdf", rechnung.PdfDatei);
        Assert.Equal("Hamburg", rechnung.Empfaenger!.Ort);
    }

    [Theory]
    [InlineData(RechnungsStatus.Offen)]
    [InlineData(RechnungsStatus.Bezahlt)]
    [InlineData(RechnungsStatus.Storniert)]
    public void PdfErzeugt_AgainKeepsTheStatus(RechnungsStatus status)
    {
        var rechnung = new Rechnung { Status = status };

        rechnung.PdfErzeugt("neu.pdf", empfaenger: null);

        Assert.Equal(status, rechnung.Status);
        Assert.Equal("neu.pdf", rechnung.PdfDatei);
        Assert.Null(rechnung.Empfaenger);
    }

    [Fact]
    public void Schlicht_IsBuiltIn_AndUsesOnlyKnownPlaceholders()
    {
        var html = MitgelieferteVorlagen.Lesen(MitgelieferteVorlagen.Standard);
        var daten = new RechnungsDaten(new Einstellungen(), new Rechnung { Positionen = [new Position()] }, new Kunde());

        var ergebnis = Vorlage.Ausfuellen(html, daten, new Uri("https://vorlage.abgerechnet.example/"));

        Assert.Empty(ergebnis.UnbekanntePlatzhalter);
        Assert.Contains("<table class=\"positionen\">", ergebnis.Html);
        Assert.Contains("<base href=\"https://vorlage.abgerechnet.example/\">", ergebnis.Html);
        Assert.Contains("@page", html);
        Assert.Contains("size: A4", html);
    }

    [Fact]
    public void Lesen_UnknownTemplate_Throws()
    {
        Assert.Throws<ArgumentException>(() => MitgelieferteVorlagen.Lesen("gibt-es-nicht"));
    }

    [Fact]
    public void Laden_PrefersTheCopyInTheFolder()
    {
        using var temp = new TempFolder();
        var folder = DataFolder.Open(temp.Path);

        var eingebaut = MitgelieferteVorlagen.Laden(folder);
        Assert.Null(eingebaut.Datei);
        Assert.Equal(MitgelieferteVorlagen.Lesen("schlicht"), eingebaut.Html);

        var eigene = temp.CreateFile(Path.Combine("Vorlagen", "schlicht.html"), "<p>{{rechnung_nummer}}</p>");
        var ausDemOrdner = MitgelieferteVorlagen.Laden(folder);
        Assert.Equal(eigene, ausDemOrdner.Datei);
        Assert.Equal("<p>{{rechnung_nummer}}</p>", ausDemOrdner.Html);
    }
}
