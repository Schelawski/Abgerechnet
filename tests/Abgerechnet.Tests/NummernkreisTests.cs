using Abgerechnet.Core.Model;

namespace Abgerechnet.Tests;

public class NummernkreisTests
{
    private static readonly DateOnly Heute = new(2026, 10, 5);

    private static Rechnung[] Mit(params string[] nummern) => nummern.Select(n => new Rechnung { Nummer = n }).ToArray();

    [Theory]
    [InlineData("111412", "111413")]
    [InlineData("RE-2026-009", "RE-2026-010")]
    [InlineData("RE-2026-999", "RE-2026-1000")]
    [InlineData("2026-001", "2026-002")]
    [InlineData("A99", "A100")]
    [InlineData("12/2026", "12/2027")]   // last digit group counts
    [InlineData("RE-7a", "RE-8a")]
    [InlineData("RE", "RE1")]
    [InlineData(" 41 ", "42")]
    public void Erhoehen(string nummer, string erwartet)
    {
        Assert.Equal(erwartet, Nummernkreis.Erhoehen(nummer));
    }

    [Fact]
    public void Naechste_NoInvoicesNoSetting_StartsWithTheYear()
    {
        Assert.Equal("2026-001", Nummernkreis.Naechste(new Einstellungen(), [], 2026));
    }

    [Fact]
    public void Naechste_HighestPlusOne_NaturalOrder()
    {
        Assert.Equal("111413", Nummernkreis.Naechste(new Einstellungen(), Mit("111409", "111412", "111410"), 2026));
        Assert.Equal("RE-10", Nummernkreis.Naechste(new Einstellungen(), Mit("RE-8", "RE-9"), 2026));
    }

    [Fact]
    public void Naechste_SettingStartsTheSequence()
    {
        var einstellungen = new Einstellungen { Rechnung = { NaechsteNummer = "111401" } };

        Assert.Equal("111401", Nummernkreis.Naechste(einstellungen, [], 2026));
        Assert.Equal("111402", Nummernkreis.Naechste(einstellungen, Mit("111401"), 2026));
        Assert.Equal("111413", Nummernkreis.Naechste(einstellungen, Mit("111412"), 2026)); // setting lower: ignored
    }

    [Fact]
    public void Naechste_SettingHigher_Jumps()
    {
        var einstellungen = new Einstellungen { Rechnung = { NaechsteNummer = "111500" } };

        Assert.Equal("111500", Nummernkreis.Naechste(einstellungen, Mit("111412"), 2026));
    }

    [Fact]
    public void Naechste_NewScheme_FromSetting()
    {
        var einstellungen = new Einstellungen { Rechnung = { NaechsteNummer = "RE-2027-001" } };

        Assert.Equal("RE-2027-001", Nummernkreis.Naechste(einstellungen, Mit("111412"), 2027));
        Assert.Equal("RE-2027-002", Nummernkreis.Naechste(einstellungen, Mit("111412", "RE-2027-001"), 2027));
    }

    [Fact]
    public void Naechste_SkipsUsedNumbers()
    {
        var einstellungen = new Einstellungen { Rechnung = { NaechsteNummer = "5" } };

        // "5" is higher than "4"+1? equal → 5 is used already → 6
        Assert.Equal("6", Nummernkreis.Naechste(einstellungen, Mit("4", "5"), 2026));
    }

    [Fact]
    public void IstVergeben_IgnoresCaseSpacesAndTheEditedInvoice()
    {
        var rechnungen = Mit("RE-1", "RE-2");

        Assert.True(Nummernkreis.IstVergeben(" re-1 ", rechnungen, ausser: null));
        Assert.False(Nummernkreis.IstVergeben("RE-1", rechnungen, ausser: rechnungen[0].Id));
        Assert.False(Nummernkreis.IstVergeben("RE-3", rechnungen, ausser: null));
    }

    [Theory]
    [InlineData(2026, 10, 5, "September 2026")]
    [InlineData(2026, 1, 15, "Dezember 2025")]
    [InlineData(2026, 3, 31, "Februar 2026")]
    public void Vormonat(int jahr, int monat, int tag, string erwartet)
    {
        Assert.Equal(erwartet, Leistungszeitraum.Vormonat(new DateOnly(jahr, monat, tag)));
    }

    [Fact]
    public void Neu_UsesTheSettings()
    {
        var einstellungen = new Einstellungen
        {
            Rechnung = { Kleinunternehmer = true, Umsatzsteuersatz = 7 },
            NeuePosition = { Beschreibung = "Softwareentwicklung", Einheit = "Std.", Einzelpreis = 88 },
        };
        var kunde = Guid.NewGuid();

        var rechnung = Rechnung.Neu(einstellungen, Mit("111412"), Heute, kunde);

        Assert.Equal("111413", rechnung.Nummer);
        Assert.Equal(Heute, rechnung.Datum);
        Assert.Equal("September 2026", rechnung.Zeitraum);
        Assert.Equal(kunde, rechnung.KundeId);
        Assert.Equal(RechnungsStatus.Entwurf, rechnung.Status);
        Assert.True(rechnung.Kleinunternehmer);
        Assert.Equal(7m, rechnung.Umsatzsteuersatz);
        var position = Assert.Single(rechnung.Positionen);
        Assert.Equal("Softwareentwicklung", position.Beschreibung);
        Assert.Equal("Std.", position.Einheit);
        Assert.Equal(88m, position.Einzelpreis);
        Assert.Equal(0m, position.Menge);
    }

    [Fact]
    public void AlsNeueRechnung_CopiesCustomerProjectAndPositions()
    {
        var einstellungen = new Einstellungen { Rechnung = { Umsatzsteuersatz = 19 } };
        var alt = new Rechnung
        {
            Nummer = "111412",
            Datum = new DateOnly(2026, 9, 1),
            Zeitraum = "August 2026",
            Projekt = "Kundenportal",
            KundeId = Guid.NewGuid(),
            Empfaenger = new Kunde { Firma = "Alt GmbH" },
            Status = RechnungsStatus.Bezahlt,
            BezahltAm = new DateOnly(2026, 9, 20),
            PdfDatei = "111412.pdf",
            Umsatzsteuersatz = 16,
            Positionen =
            [
                new Position { Zeitraum = "01.–15.08.", Beschreibung = "Entwicklung", Detail = "Sprint 1", Menge = 80, Einheit = "Std.", Einzelpreis = 88 },
                new Position { Beschreibung = "Reise", Menge = 1, Einheit = "pauschal", Einzelpreis = 120 },
            ],
        };

        var neu = alt.AlsNeueRechnung(einstellungen, [alt], Heute);

        Assert.NotEqual(alt.Id, neu.Id);
        Assert.Equal("111413", neu.Nummer);
        Assert.Equal(Heute, neu.Datum);
        Assert.Equal("September 2026", neu.Zeitraum);
        Assert.Equal("Kundenportal", neu.Projekt);
        Assert.Equal(alt.KundeId, neu.KundeId);
        Assert.Null(neu.Empfaenger);       // address is frozen again with the new PDF
        Assert.Equal(RechnungsStatus.Entwurf, neu.Status);
        Assert.Null(neu.BezahltAm);
        Assert.Null(neu.PdfDatei);
        Assert.Equal(19m, neu.Umsatzsteuersatz); // current settings, not the old 16 %
        Assert.Equal(2, neu.Positionen.Count);
        Assert.Equal("", neu.Positionen[0].Zeitraum);
        Assert.Equal("Sprint 1", neu.Positionen[0].Detail);
        Assert.Equal(80m, neu.Positionen[0].Menge);
        Assert.Equal(120m, neu.Positionen[1].Einzelpreis);
        Assert.DoesNotContain(neu.Positionen, p => alt.Positionen.Any(a => a.Id == p.Id));

        // The original is unchanged.
        Assert.Equal("01.–15.08.", alt.Positionen[0].Zeitraum);
        Assert.Equal(RechnungsStatus.Bezahlt, alt.Status);
    }
}
