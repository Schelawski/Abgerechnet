using Abgerechnet.Core.Model;
using Abgerechnet.Core.Storage;

namespace Abgerechnet.Tests;

public class KundeTests
{
    [Theory]
    [InlineData("Müller & Söhne GmbH", "Mueller-Soehne")]
    [InlineData("Beispiel AG", "Beispiel")]
    [InlineData("Deutsche Bahn AG", "Deutsche-Bahn")]
    [InlineData("Weiß Consulting GmbH & Co. KG", "Weiss-Consulting")]
    [InlineData("Schulz e. K.", "Schulz")]
    [InlineData("Ärzte-Verbund Nord UG (haftungsbeschränkt)", "AerzteVerbund-Nord")]
    [InlineData("  ", "")]
    [InlineData(null, "")]
    public void KurznameVorschlag(string? firma, string expected)
    {
        Assert.Equal(expected, Kunde.KurznameVorschlag(firma));
    }

    [Fact]
    public void KurznameVorschlag_IsShort()
    {
        Assert.True(Kunde.KurznameVorschlag("Donaudampfschifffahrtsgesellschaftskapitän Verwaltungsgesellschaft").Length <= 24);
    }

    [Fact]
    public void Anschrift_LeavesOutEmptyParts()
    {
        var kunde = new Kunde { Firma = "Beispiel AG", Strasse = "Hauptstr. 1", Plz = "10115", Ort = "Berlin" };

        Assert.Equal(["Beispiel AG", "Hauptstr. 1", "10115 Berlin"], kunde.Anschrift());

        kunde.Ansprechpartner = "Frau Meier";
        kunde.Land = "Deutschland";
        kunde.Plz = "";
        Assert.Equal(["Beispiel AG", "Frau Meier", "Hauptstr. 1", "Berlin", "Deutschland"], kunde.Anschrift());
    }

    [Fact]
    public void EmpfaengerFestschreiben_KeepsTheAddressWhenTheCustomerChanges()
    {
        var kunden = new KundenDatei();
        var kunde = new Kunde { Firma = "Alt GmbH", Strasse = "Altweg 1", Plz = "11111", Ort = "Alt" };
        kunden.Kunden.Add(kunde);
        var gestellt = new Rechnung { KundeId = kunde.Id };
        var entwurf = new Rechnung { KundeId = kunde.Id };

        gestellt.EmpfaengerFestschreiben(kunde);
        kunde.Firma = "Neu GmbH";
        kunde.Strasse = "Neuweg 2";

        Assert.Equal("Alt GmbH", gestellt.EmpfaengerAus(kunden)!.Firma);
        Assert.Equal("Altweg 1", gestellt.EmpfaengerAus(kunden)!.Strasse);
        Assert.Equal("Neu GmbH", entwurf.EmpfaengerAus(kunden)!.Firma); // drafts follow the customer
    }

    [Fact]
    public void EmpfaengerAus_DeletedCustomer_KeepsTheCopy()
    {
        var kunde = new Kunde { Firma = "Weg GmbH" };
        var rechnung = new Rechnung();
        rechnung.EmpfaengerFestschreiben(kunde);

        Assert.Equal("Weg GmbH", rechnung.EmpfaengerAus(new KundenDatei())!.Firma);
        Assert.Null(new Rechnung { KundeId = kunde.Id }.EmpfaengerAus(new KundenDatei()));
    }

    [Fact]
    public void Snapshot_IsStoredInTheInvoiceFile()
    {
        using var temp = new TempFolder();
        var folder = DataFolder.Open(temp.Path);
        var kunde = new Kunde { Firma = "Beispiel AG", Ort = "Berlin" };
        var rechnung = new Rechnung { Nummer = "1" };
        rechnung.EmpfaengerFestschreiben(kunde);
        folder.Rechnungen.Rechnungen.Add(rechnung);
        folder.SaveRechnungen();

        var loaded = Assert.Single(DataFolder.Open(temp.Path).Rechnungen.Rechnungen);

        Assert.Equal(kunde.Id, loaded.KundeId);
        Assert.Equal("Berlin", loaded.Empfaenger!.Ort);
    }

    [Fact]
    public void VerwendetKunde()
    {
        var kunde = new Kunde();
        var rechnungen = new RechnungenDatei { Rechnungen = [new Rechnung { KundeId = Guid.NewGuid() }] };

        Assert.False(rechnungen.VerwendetKunde(kunde.Id));

        rechnungen.Rechnungen.Add(new Rechnung { KundeId = kunde.Id, Status = RechnungsStatus.Storniert });
        Assert.True(rechnungen.VerwendetKunde(kunde.Id));
    }

    [Fact]
    public void Sortiert_ByCompanyGerman()
    {
        var datei = new KundenDatei
        {
            Kunden = [new Kunde { Firma = "Zeta" }, new Kunde { Firma = "ärzte nord" }, new Kunde { Firma = "Alpha" }, new Kunde { Firma = "Bäcker" }],
        };

        Assert.Equal(["Alpha", "ärzte nord", "Bäcker", "Zeta"], datei.Sortiert().Select(k => k.Firma));
    }

    [Fact]
    public void DataFolder_SaveKunden_ReplacesTheCustomers()
    {
        using var temp = new TempFolder();
        var folder = DataFolder.Open(temp.Path);
        var changed = JsonDataFile.Clone(folder.Kunden);
        changed.Kunden.Add(new Kunde { Firma = "Neu AG" });

        folder.SaveKunden(changed);

        Assert.Same(changed, folder.Kunden);
        Assert.Equal("Neu AG", Assert.Single(DataFolder.Open(temp.Path).Kunden.Kunden).Firma);
    }
}
