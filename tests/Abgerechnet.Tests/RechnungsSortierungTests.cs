using Abgerechnet.Core.Model;

namespace Abgerechnet.Tests;

public class RechnungsSortierungTests
{
    private static readonly Kunde Zeta = new() { Firma = "Zeta GmbH" };
    private static readonly Kunde Alpha = new() { Firma = "Alpha AG" };

    private static readonly Rechnung[] Rechnungen =
    [
        new() { Nummer = "9", Datum = new(2026, 3, 1), KundeId = Zeta.Id, Status = RechnungsStatus.Bezahlt, Positionen = [new() { Menge = 1, Einzelpreis = 300 }] },
        new() { Nummer = "10", Datum = new(2026, 5, 1), KundeId = Alpha.Id, Status = RechnungsStatus.Offen, Positionen = [new() { Menge = 1, Einzelpreis = 100 }] },
        new() { Nummer = "11", Datum = new(2026, 5, 1), KundeId = Zeta.Id, Status = RechnungsStatus.Entwurf, Positionen = [new() { Menge = 1, Einzelpreis = 200 }] },
    ];

    private static RechnungsSortierung Sortierung() => new() { Kunden = new KundenDatei { Kunden = [Zeta, Alpha] } };

    private static string[] Nummern(RechnungsSortierung sortierung) => sortierung.Sortieren(Rechnungen).Select(r => r.Nummer).ToArray();

    [Fact]
    public void Default_NewestFirst_SameDayByNumber()
    {
        Assert.Equal(["11", "10", "9"], Nummern(Sortierung()));
    }

    [Fact]
    public void Nummer_StartsWithTheHighest_ThenReverses()
    {
        var sortierung = Sortierung();

        sortierung.Umschalten(RechnungsSpalte.Nummer);
        Assert.Equal(["11", "10", "9"], Nummern(sortierung));

        sortierung.Umschalten(RechnungsSpalte.Nummer);
        Assert.Equal(["9", "10", "11"], Nummern(sortierung)); // natural order, not "10" < "11" < "9"
    }

    [Fact]
    public void Kunde_AlphabeticalByCompany()
    {
        var sortierung = Sortierung();

        sortierung.Umschalten(RechnungsSpalte.Kunde);

        Assert.False(sortierung.Absteigend);
        Assert.Equal(["10", "9", "11"], Nummern(sortierung)); // Alpha, then Zeta by number
    }

    [Fact]
    public void Betrag_LargestFirst()
    {
        var sortierung = Sortierung();

        sortierung.Umschalten(RechnungsSpalte.Betrag);

        Assert.Equal(["9", "11", "10"], Nummern(sortierung));
    }

    [Fact]
    public void Status_InLifecycleOrder()
    {
        var sortierung = Sortierung();

        sortierung.Umschalten(RechnungsSpalte.Status);

        Assert.Equal(["11", "10", "9"], Nummern(sortierung)); // Entwurf, Offen, Bezahlt
    }

    [Fact]
    public void Kunde_UsesTheFrozenRecipient()
    {
        var rechnung = new Rechnung { Nummer = "1", KundeId = Zeta.Id };
        rechnung.EmpfaengerFestschreiben(new Kunde { Id = Zeta.Id, Firma = "Aaa (alter Name)" });
        var sortierung = Sortierung();
        sortierung.Umschalten(RechnungsSpalte.Kunde);

        var first = sortierung.Sortieren(Rechnungen.Append(rechnung)).First();

        Assert.Same(rechnung, first);
    }
}
