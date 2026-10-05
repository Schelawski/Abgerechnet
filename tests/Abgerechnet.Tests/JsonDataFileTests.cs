using System.Text.Json;
using Abgerechnet.Core.Model;
using Abgerechnet.Core.Storage;

namespace Abgerechnet.Tests;

public class JsonDataFileTests
{
    private static Rechnung SampleRechnung() => new()
    {
        Nummer = "111412",
        Datum = new DateOnly(2026, 10, 1),
        Zeitraum = "September 2026",
        Projekt = "Weiterentwicklung Kundenportal",
        KundeId = Guid.NewGuid(),
        Empfaenger = new Kunde { Firma = "Grünwald & Söhne GmbH", Kurzname = "Nordlicht", Strasse = "Hauptstraße 1", Plz = "12345", Ort = "Köln" },
        Status = RechnungsStatus.Bezahlt,
        BezahltAm = new DateOnly(2026, 10, 20),
        PdfDatei = "111412_Nordlicht_2026-10-01.pdf",
        Positionen =
        [
            new Position { Beschreibung = "Entwicklung", Detail = "Sprint 17", Menge = 152.5m, Einheit = "Std.", Einzelpreis = 88.10m },
            new Position { Zeitraum = "01.–15.09.2026", Beschreibung = "Reisekosten €", Menge = 1, Einheit = "pauschal", Einzelpreis = 0.1m },
        ],
    };

    [Fact]
    public void SaveAndLoad_KeepsAllValues()
    {
        using var temp = new TempFolder();
        var path = temp.File("rechnungen.json");
        var original = SampleRechnung();

        JsonDataFile.Save(path, new RechnungenDatei { Rechnungen = [original] });
        var (loaded, exists) = JsonDataFile.Load<RechnungenDatei>(path);

        Assert.True(exists);
        Assert.Equal(JsonDataFile.CurrentVersion, loaded.Version);
        var rechnung = Assert.Single(loaded.Rechnungen);
        Assert.Equal(JsonSerializer.Serialize(original), JsonSerializer.Serialize(rechnung));
    }

    [Fact]
    public void Save_WritesReadableJson()
    {
        using var temp = new TempFolder();
        var path = temp.File("rechnungen.json");

        JsonDataFile.Save(path, new RechnungenDatei { Rechnungen = [SampleRechnung()] });
        var json = File.ReadAllText(path);

        Assert.Contains("\"version\": 1", json);
        Assert.Contains("\"datum\": \"2026-10-01\"", json);       // ISO date
        Assert.Contains("\"bezahltAm\": \"2026-10-20\"", json);
        Assert.Contains("\"status\": \"bezahlt\"", json);          // readable status
        Assert.Contains("\"menge\": 152.5", json);                 // decimal as number
        Assert.Contains("\"einzelpreis\": 88.10", json);           // decimal keeps its scale
        Assert.Contains("Grünwald & Söhne GmbH", json);              // umlauts not escaped
        Assert.Contains("Reisekosten €", json);
        Assert.Contains("\n  ", json);                             // indented
    }

    [Fact]
    public void Load_MissingFile_ReturnsEmptyDataWithoutWriting()
    {
        using var temp = new TempFolder();
        var path = temp.File("kunden.json");

        var (data, exists) = JsonDataFile.Load<KundenDatei>(path);

        Assert.False(exists);
        Assert.Empty(data.Kunden);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void Load_FileWithMissingFields_FillsDefaults()
    {
        using var temp = new TempFolder();
        // Hand-written: no version, no lists, comment, trailing comma, capitalized property, no position fields.
        var path = temp.CreateFile("rechnungen.json", """
            {
              // written by hand
              "Rechnungen": [
                { "nummer": "1", "positionen": [ { "menge": 2 } ] },
                { "nummer": "2", "positionen": null, "projekt": null },
              ]
            }
            """);

        var (data, _) = JsonDataFile.Load<RechnungenDatei>(path);

        Assert.Equal(JsonDataFile.CurrentVersion, data.Version);
        Assert.Equal(2, data.Rechnungen.Count);
        var first = data.Rechnungen[0];
        Assert.NotEqual(Guid.Empty, first.Id);
        Assert.Equal(RechnungsStatus.Entwurf, first.Status);
        Assert.Equal(string.Empty, first.Zeitraum);
        var position = Assert.Single(first.Positionen);
        Assert.Equal(2m, position.Menge);
        Assert.NotEqual(Guid.Empty, position.Id);
        Assert.Equal(string.Empty, position.Beschreibung);
        Assert.Empty(data.Rechnungen[1].Positionen);
        Assert.Equal(string.Empty, data.Rechnungen[1].Projekt);
    }

    [Fact]
    public void Load_CorruptJson_ThrowsWithLineAndLeavesFileUntouched()
    {
        using var temp = new TempFolder();
        const string broken = "{\n  \"version\": 1,\n  \"rechnungen\": [ { \"nummer\": \"1\" \n}";
        var path = temp.CreateFile("rechnungen.json", broken);

        var ex = Assert.Throws<DataFileException>(() => JsonDataFile.Load<RechnungenDatei>(path));

        Assert.Equal(DataFileProblem.Corrupt, ex.Problem);
        Assert.Equal("rechnungen.json", ex.FileName);
        Assert.NotNull(ex.Line);
        Assert.Equal(broken, File.ReadAllText(path));
    }

    [Theory]
    [InlineData("""{ "rechnungen": [ { "status": "verschickt" } ] }""")] // unknown status
    [InlineData("""{ "rechnungen": [ { "positionen": [ { "menge": "viel" } ] } ] }""")] // wrong type
    [InlineData("""{ "rechnungen": [ { "datum": "01.10.2026" } ] }""")]  // German date instead of ISO
    [InlineData("null")]
    public void Load_UnexpectedContent_IsReportedAsCorrupt(string json)
    {
        using var temp = new TempFolder();
        var path = temp.CreateFile("rechnungen.json", json);

        var ex = Assert.ThrowsAny<DataFileException>(() => JsonDataFile.Load<RechnungenDatei>(path));

        Assert.Equal(DataFileProblem.Corrupt, ex.Problem);
    }

    [Fact]
    public void Load_UnknownFields_AreIgnored()
    {
        using var temp = new TempFolder();
        var path = temp.CreateFile("rechnungen.json", """{ "version": 1, "notiz": "x", "rechnungen": [ { "nummer": "7", "farbe": "blau" } ] }""");

        var (data, _) = JsonDataFile.Load<RechnungenDatei>(path);

        Assert.Equal("7", Assert.Single(data.Rechnungen).Nummer);
    }

    [Fact]
    public void Load_FileFromNewerVersion_IsRefused()
    {
        using var temp = new TempFolder();
        var path = temp.CreateFile("kunden.json", """{ "version": 99, "kunden": [] }""");

        var ex = Assert.Throws<DataFileException>(() => JsonDataFile.Load<KundenDatei>(path));

        Assert.Equal(DataFileProblem.TooNew, ex.Problem);
    }

    [Fact]
    public void Save_KeepsOnlyThePreviousVersionAsBackup()
    {
        using var temp = new TempFolder();
        var path = temp.File("kunden.json");
        var backup = temp.File("kunden.bak.json");
        var data = new KundenDatei();

        data.Kunden.Add(new Kunde { Firma = "Erster" });
        JsonDataFile.Save(path, data);
        Assert.False(File.Exists(backup)); // nothing to back up yet

        data.Kunden.Add(new Kunde { Firma = "Zweiter" });
        JsonDataFile.Save(path, data);
        data.Kunden.Add(new Kunde { Firma = "Dritter" });
        JsonDataFile.Save(path, data);

        var backupData = JsonDataFile.Load<KundenDatei>(backup).Data;
        Assert.Equal(["Erster", "Zweiter"], backupData.Kunden.Select(k => k.Firma));
        Assert.Equal(3, JsonDataFile.Load<KundenDatei>(path).Data.Kunden.Count);
        Assert.Equal(["kunden.bak.json", "kunden.json"], Directory.GetFiles(temp.Path).Select(Path.GetFileName).Order());
    }

    [Fact]
    public void BackupPath_InsertsBakBeforeTheExtension()
    {
        Assert.Equal(Path.Combine("C:\\Daten", "rechnungen.bak.json"), JsonDataFile.BackupPath(Path.Combine("C:\\Daten", "rechnungen.json")));
    }
}
