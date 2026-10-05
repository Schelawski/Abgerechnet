using System.Text.Json;
using Abgerechnet.Core.Model;
using Abgerechnet.Core.Storage;

namespace Abgerechnet.Tests;

public class RechnungTests
{
    private static Rechnung Mit(RechnungsStatus status, decimal netto, int jahr = 2026) => new()
    {
        Status = status,
        Datum = new DateOnly(jahr, 3, 1),
        Positionen = [new Position { Menge = 1, Einzelpreis = netto }],
    };

    // ----- Amounts -----

    [Fact]
    public void Betrag_WithVat()
    {
        var rechnung = new Rechnung
        {
            Umsatzsteuersatz = 19,
            Positionen =
            [
                new Position { Menge = 152.5m, Einzelpreis = 88m },  // 13.420,00
                new Position { Menge = 1, Einzelpreis = 99.99m },    //     99,99
            ],
        };

        var betrag = Rechnungsbetrag.Berechnen(rechnung);

        Assert.Equal(13519.99m, betrag.Netto);
        Assert.Equal(2568.80m, betrag.Umsatzsteuer); // 2568,7981 → 2568,80
        Assert.Equal(16088.79m, betrag.Brutto);
    }

    [Fact]
    public void Betrag_RoundsEachPositionCommercially()
    {
        // 0,125 € × 3 = 0,375 → 0,38 (half up, not banker's rounding to 0,38/0,37)
        var position = new Position { Menge = 3, Einzelpreis = 0.125m };
        Assert.Equal(0.38m, Rechnungsbetrag.Positionsbetrag(position));
        Assert.Equal(0.13m, Rechnungsbetrag.Runden(0.125m));
        Assert.Equal(-0.13m, Rechnungsbetrag.Runden(-0.125m));

        // Two positions of 0,125 each: 0,13 + 0,13 = 0,26 (sum of the printed values, not round(0,25))
        var rechnung = new Rechnung { Kleinunternehmer = true, Positionen = [new Position { Menge = 1, Einzelpreis = 0.125m }, new Position { Menge = 1, Einzelpreis = 0.125m }] };
        Assert.Equal(0.26m, Rechnungsbetrag.Berechnen(rechnung).Netto);
    }

    [Fact]
    public void Betrag_SmallBusiness_HasNoVat()
    {
        var rechnung = new Rechnung { Kleinunternehmer = true, Umsatzsteuersatz = 19, Positionen = [new Position { Menge = 10, Einzelpreis = 50 }] };

        var betrag = Rechnungsbetrag.Berechnen(rechnung);

        Assert.Equal(new Rechnungsbetrag(500m, 0m, 500m), betrag);
    }

    [Fact]
    public void Betrag_ReducedRate()
    {
        var rechnung = new Rechnung { Umsatzsteuersatz = 7, Positionen = [new Position { Menge = 1, Einzelpreis = 100 }] };

        Assert.Equal(107m, Rechnungsbetrag.Berechnen(rechnung).Brutto);
    }

    [Fact]
    public void Betrag_WithoutPositions_IsZero()
    {
        Assert.Equal(new Rechnungsbetrag(0, 0, 0), Rechnungsbetrag.Berechnen(new Rechnung()));
    }

    // ----- Tiles -----

    [Fact]
    public void Uebersicht_TotalCountsOnlyOpenAndPaid()
    {
        var rechnungen = new[]
        {
            Mit(RechnungsStatus.Offen, 100),
            Mit(RechnungsStatus.Offen, 200),
            Mit(RechnungsStatus.Bezahlt, 1000),
            Mit(RechnungsStatus.Entwurf, 5000),
            Mit(RechnungsStatus.Entwurf, 5000),
            Mit(RechnungsStatus.Storniert, 9000),
        };

        var uebersicht = Uebersicht.Berechnen(rechnungen);

        Assert.Equal(new Summe(357m, 2), uebersicht.Offen);       // 300 + 19 %
        Assert.Equal(new Summe(1190m, 1), uebersicht.Bezahlt);
        Assert.Equal(new Summe(1547m, 3), uebersicht.Gesamt);
        Assert.Equal(2, uebersicht.Entwuerfe);
    }

    [Fact]
    public void Jahre_NewestFirst_WithCurrentYear()
    {
        var rechnungen = new[] { Mit(RechnungsStatus.Offen, 1, 2024), Mit(RechnungsStatus.Offen, 1, 2025), Mit(RechnungsStatus.Offen, 1, 2025) };

        Assert.Equal([2026, 2025, 2024], Uebersicht.Jahre(rechnungen, 2026));
        Assert.Equal([2026], Uebersicht.Jahre([], 2026));
    }

    // ----- Status -----

    [Fact]
    public void Draft_CanOnlyBeDeleted()
    {
        var entwurf = new Rechnung();

        Assert.True(entwurf.KannGeloeschtWerden);
        Assert.Empty(entwurf.ErlaubteStatuswechsel());
        Assert.Throws<InvalidOperationException>(() => entwurf.StatusAendern(RechnungsStatus.Bezahlt, DateOnly.MinValue));
    }

    [Fact]
    public void IssuedInvoice_CannotBeDeleted()
    {
        foreach (var status in new[] { RechnungsStatus.Offen, RechnungsStatus.Bezahlt, RechnungsStatus.Storniert })
            Assert.False(new Rechnung { Status = status }.KannGeloeschtWerden);
    }

    [Fact]
    public void Paid_RecordsAndClearsThePaymentDate()
    {
        var rechnung = new Rechnung { Status = RechnungsStatus.Offen };
        var heute = new DateOnly(2026, 10, 5);

        rechnung.StatusAendern(RechnungsStatus.Bezahlt, heute);
        Assert.Equal(heute, rechnung.BezahltAm);

        rechnung.StatusAendern(RechnungsStatus.Offen, heute);
        Assert.Null(rechnung.BezahltAm);

        rechnung.StatusAendern(RechnungsStatus.Storniert, heute);
        Assert.Equal(RechnungsStatus.Storniert, rechnung.Status);
        Assert.Equal([RechnungsStatus.Offen], rechnung.ErlaubteStatuswechsel());
    }

    [Fact]
    public void Load_PaymentDateWithoutPaidStatus_IsCleared()
    {
        using var temp = new TempFolder();
        var path = temp.CreateFile("rechnungen.json", """{ "rechnungen": [ { "status": "offen", "bezahltAm": "2026-01-01" } ] }""");

        var rechnung = Assert.Single(JsonDataFile.Load<RechnungenDatei>(path).Data.Rechnungen);

        Assert.Null(rechnung.BezahltAm);
    }

    [Fact]
    public void Load_OldInvoiceWithoutRate_Gets19Percent()
    {
        using var temp = new TempFolder();
        var path = temp.CreateFile("rechnungen.json", """{ "rechnungen": [ { "nummer": "1" } ] }""");

        var rechnung = Assert.Single(JsonDataFile.Load<RechnungenDatei>(path).Data.Rechnungen);

        Assert.Equal(19m, rechnung.Umsatzsteuersatz);
        Assert.False(rechnung.Kleinunternehmer);
    }

    // ----- File format -----

    [Fact]
    public void Json_ContainsOnlyStoredFields()
    {
        var rechnung = new Rechnung { KundeId = Guid.NewGuid(), PdfDatei = "a.pdf", BezahltAm = DateOnly.MinValue, Empfaenger = new Kunde(), Positionen = [new Position()] };

        using var json = JsonDocument.Parse(JsonSerializer.Serialize(rechnung, JsonDataFile.Options));
        var names = json.RootElement.EnumerateObject().Select(p => p.Name).ToList();

        Assert.Equal(
            ["id", "nummer", "datum", "zeitraum", "projekt", "kundeId", "empfaenger", "status", "bezahltAm", "pdfDatei", "umsatzsteuersatz", "kleinunternehmer", "positionen"],
            names);
    }

    // ----- Number order -----

    [Theory]
    [InlineData("111409", "111410")]
    [InlineData("RE-9", "RE-10")]
    [InlineData("RE-2026-9", "RE-2026-10")]
    [InlineData("9", "10")]
    [InlineData("009", "10")]
    [InlineData("A", "b")]
    [InlineData("RE-1", "RE-1a")]
    public void Rechnungsnummer_NaturalOrder(string kleiner, string groesser)
    {
        Assert.True(Rechnungsnummer.Vergleich.Compare(kleiner, groesser) < 0);
        Assert.True(Rechnungsnummer.Vergleich.Compare(groesser, kleiner) > 0);
    }

    [Fact]
    public void Rechnungsnummer_EqualIgnoringCase()
    {
        Assert.Equal(0, Rechnungsnummer.Vergleich.Compare("re-1", "RE-1"));
    }
}
