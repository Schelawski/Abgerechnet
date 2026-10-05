using Abgerechnet.Core.Model;

namespace Abgerechnet.Tests;

public class ZahlungserinnerungTests
{
    private static readonly DateOnly Heute = new(2026, 10, 5);

    private static Einstellungen MitZahlungsziel(int tage)
    {
        var einstellungen = new Einstellungen();
        einstellungen.Rechnung.ZahlungszielTage = tage;
        return einstellungen;
    }

    private static Rechnung Rechnung(string nummer, RechnungsStatus status, DateOnly datum) =>
        new() { Nummer = nummer, Status = status, Datum = datum };

    [Fact]
    public void Open_IsOverdue_OnlyAfterTheDueDay()
    {
        var einstellungen = MitZahlungsziel(30);

        Assert.False(Zahlungserinnerung.IstUeberfaellig(Rechnung("1", RechnungsStatus.Offen, Heute.AddDays(-30)), einstellungen, Heute)); // due today
        Assert.True(Zahlungserinnerung.IstUeberfaellig(Rechnung("2", RechnungsStatus.Offen, Heute.AddDays(-31)), einstellungen, Heute));
        Assert.False(Zahlungserinnerung.IstUeberfaellig(Rechnung("3", RechnungsStatus.Offen, Heute), einstellungen, Heute));
    }

    [Fact]
    public void FollowsThePaymentTerm()
    {
        var rechnung = Rechnung("1", RechnungsStatus.Offen, Heute.AddDays(-15));

        Assert.True(Zahlungserinnerung.IstUeberfaellig(rechnung, MitZahlungsziel(14), Heute));
        Assert.False(Zahlungserinnerung.IstUeberfaellig(rechnung, MitZahlungsziel(30), Heute));
    }

    [Fact]
    public void WithoutPaymentTerm_RemindsAfter30Days()
    {
        var ohne = MitZahlungsziel(0);

        Assert.Equal(30, Zahlungserinnerung.Tage(ohne));
        Assert.False(Zahlungserinnerung.IstUeberfaellig(Rechnung("1", RechnungsStatus.Offen, Heute.AddDays(-30)), ohne, Heute));
        Assert.True(Zahlungserinnerung.IstUeberfaellig(Rechnung("2", RechnungsStatus.Offen, Heute.AddDays(-31)), ohne, Heute));
    }

    [Theory]
    [InlineData(RechnungsStatus.Entwurf)]
    [InlineData(RechnungsStatus.Bezahlt)]
    [InlineData(RechnungsStatus.Storniert)]
    public void OnlyOpenInvoices_AreOverdue(RechnungsStatus status)
    {
        Assert.False(Zahlungserinnerung.IstUeberfaellig(Rechnung("1", status, Heute.AddDays(-100)), new Einstellungen(), Heute));
    }

    [Fact]
    public void Draft_IsForgotten_WhenOlderThan7Days()
    {
        Assert.False(Zahlungserinnerung.IstVergessenerEntwurf(Rechnung("1", RechnungsStatus.Entwurf, Heute.AddDays(-7)), Heute));
        Assert.True(Zahlungserinnerung.IstVergessenerEntwurf(Rechnung("2", RechnungsStatus.Entwurf, Heute.AddDays(-8)), Heute));
        Assert.False(Zahlungserinnerung.IstVergessenerEntwurf(Rechnung("3", RechnungsStatus.Offen, Heute.AddDays(-8)), Heute));
        Assert.False(Zahlungserinnerung.IstVergessenerEntwurf(Rechnung("4", RechnungsStatus.Entwurf, Heute.AddDays(10)), Heute)); // dated ahead
    }

    [Fact]
    public void Pruefen_ListsBothKinds_OldestFirst()
    {
        Rechnung[] rechnungen =
        [
            Rechnung("2026-003", RechnungsStatus.Offen, new(2026, 8, 20)),
            Rechnung("2026-001", RechnungsStatus.Offen, new(2026, 7, 1)),
            Rechnung("2026-002", RechnungsStatus.Bezahlt, new(2026, 7, 2)),
            Rechnung("2026-004", RechnungsStatus.Offen, new(2026, 9, 30)),
            Rechnung("2026-005", RechnungsStatus.Entwurf, new(2026, 9, 1)),
        ];

        var erinnerung = Zahlungserinnerung.Pruefen(rechnungen, new Einstellungen(), Heute);

        Assert.Equal(["2026-001", "2026-003"], erinnerung.Ueberfaellig.Select(r => r.Nummer));
        Assert.Equal(["2026-005"], erinnerung.VergesseneEntwuerfe.Select(r => r.Nummer));
        Assert.False(erinnerung.IstLeer);
        Assert.True(Zahlungserinnerung.Pruefen([], new Einstellungen(), Heute).IstLeer);
    }

    [Fact]
    public void TageSeit_CountsCalendarDays()
    {
        Assert.Equal(42, Zahlungserinnerung.TageSeit(Rechnung("1", RechnungsStatus.Offen, Heute.AddDays(-42)), Heute));
    }

    [Fact]
    public void Sorting_ByPaymentDate_PutsUnpaidLast_WhenDescending()
    {
        var sortierung = new RechnungsSortierung();
        sortierung.Umschalten(RechnungsSpalte.BezahltAm);
        Rechnung[] rechnungen =
        [
            new() { Nummer = "1", Status = RechnungsStatus.Offen },
            new() { Nummer = "2", Status = RechnungsStatus.Bezahlt, BezahltAm = new(2026, 9, 1) },
            new() { Nummer = "3", Status = RechnungsStatus.Bezahlt, BezahltAm = new(2026, 9, 20) },
        ];

        Assert.True(sortierung.Absteigend);
        Assert.Equal(["3", "2", "1"], sortierung.Sortieren(rechnungen).Select(r => r.Nummer));
    }
}
