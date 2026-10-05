using Abgerechnet.Core.Model;
using Abgerechnet.Core.Storage;

namespace Abgerechnet.Tests;

public class EinstellungenTests
{
    [Fact]
    public void NewSettings_HaveSensibleDefaults()
    {
        var einstellungen = new Einstellungen();

        Assert.False(einstellungen.Rechnung.Kleinunternehmer);
        Assert.Equal(19m, einstellungen.Rechnung.Umsatzsteuersatz);
        Assert.Equal(30, einstellungen.Rechnung.ZahlungszielTage);
        Assert.Equal("{nummer}_{kunde_kurzname}_{datum}.pdf", einstellungen.Rechnung.PdfDateiname);
        Assert.Contains("§ 19 UStG", einstellungen.Rechnung.KleinunternehmerHinweis);
        Assert.Equal("Std.", einstellungen.NeuePosition.Einheit);
        Assert.False(einstellungen.Absender.IstVollstaendig);
    }

    [Fact]
    public void SaveAndLoad_KeepsAllValues_InReadableSections()
    {
        using var temp = new TempFolder();
        var path = temp.File("abgerechnet.json");
        var einstellungen = new Einstellungen
        {
            Absender = { Firma = "Schmidt IT", Name = "Jörg Schmidt", Strasse = "Am Markt 3", Plz = "50667", Ort = "Köln", Steuernummer = "214/5678/1234" },
            Bank = { Iban = "DE89 3704 0044 0532 0130 00", Bic = "COBADEFFXXX" },
            Rechnung = { Kleinunternehmer = true, ZahlungszielTage = 0, NaechsteNummer = "RE-2026-001" },
            NeuePosition = { Beschreibung = "Softwareentwicklung", Einzelpreis = 88.5m },
        };

        JsonDataFile.Save(path, einstellungen);
        var json = File.ReadAllText(path);
        var loaded = JsonDataFile.Load<Einstellungen>(path).Data;

        Assert.Contains("\"absender\": {", json);
        Assert.Contains("\"bank\": {", json);
        Assert.Contains("\"neuePosition\": {", json);
        Assert.DoesNotContain("anzeigename", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("istVollstaendig", json, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Jörg Schmidt", loaded.Absender.Name);
        Assert.Equal("DE89 3704 0044 0532 0130 00", loaded.Bank.Iban);
        Assert.True(loaded.Rechnung.Kleinunternehmer);
        Assert.Equal(0, loaded.Rechnung.ZahlungszielTage);
        Assert.Equal("RE-2026-001", loaded.Rechnung.NaechsteNummer);
        Assert.Equal(88.5m, loaded.NeuePosition.Einzelpreis);
        Assert.True(loaded.Absender.IstVollstaendig);
    }

    [Fact]
    public void Load_FileFromIssue2_GetsDefaults()
    {
        using var temp = new TempFolder();
        // abgerechnet.json as created before the settings existed (only the version).
        var path = temp.CreateFile("abgerechnet.json", """{ "version": 1 }""");

        var loaded = JsonDataFile.Load<Einstellungen>(path).Data;

        Assert.NotNull(loaded.Absender);
        Assert.Equal(19m, loaded.Rechnung.Umsatzsteuersatz);
        Assert.Equal(30, loaded.Rechnung.ZahlungszielTage);
        Assert.Equal(PdfDateiname.DefaultMuster, loaded.Rechnung.PdfDateiname);
    }

    [Fact]
    public void Load_InvalidValues_AreRepaired()
    {
        using var temp = new TempFolder();
        var path = temp.CreateFile("abgerechnet.json", """
            {
              "version": 1,
              "absender": null,
              "rechnung": { "umsatzsteuersatz": 190, "zahlungszielTage": -5, "pdfDateiname": " " },
              "neuePosition": { "einzelpreis": -1, "einheit": null }
            }
            """);

        var loaded = JsonDataFile.Load<Einstellungen>(path).Data;

        Assert.Equal(string.Empty, loaded.Absender.Firma);
        Assert.Equal(19m, loaded.Rechnung.Umsatzsteuersatz);
        Assert.Equal(0, loaded.Rechnung.ZahlungszielTage);
        Assert.Equal(PdfDateiname.DefaultMuster, loaded.Rechnung.PdfDateiname);
        Assert.Equal(0m, loaded.NeuePosition.Einzelpreis);
        Assert.Equal(string.Empty, loaded.NeuePosition.Einheit);
    }

    [Fact]
    public void DataFolder_SaveEinstellungen_ReplacesTheSettings()
    {
        using var temp = new TempFolder();
        var folder = DataFolder.Open(temp.Path);
        var changed = JsonDataFile.Clone(folder.Einstellungen);
        changed.Absender.Firma = "Neu GmbH";

        folder.SaveEinstellungen(changed);

        Assert.Same(changed, folder.Einstellungen);
        Assert.Equal("Neu GmbH", DataFolder.Open(temp.Path).Einstellungen.Absender.Firma);
    }

    [Fact]
    public void Clone_IsIndependent()
    {
        var original = new Einstellungen { Absender = { Firma = "Alt" } };

        var copy = JsonDataFile.Clone(original);
        copy.Absender.Firma = "Neu";

        Assert.Equal("Alt", original.Absender.Firma);
    }

    [Theory]
    [InlineData("Schmidt IT", "Jörg Schmidt", "Schmidt IT")]
    [InlineData("", "Jörg Schmidt", "Jörg Schmidt")]
    [InlineData("  ", "  ", "")]
    public void Anzeigename_PrefersTheCompany(string firma, string name, string expected)
    {
        Assert.Equal(expected, new Absender { Firma = firma, Name = name }.Anzeigename);
    }
}
