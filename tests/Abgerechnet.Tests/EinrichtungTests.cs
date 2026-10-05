using System.Drawing;
using System.Drawing.Imaging;
using Abgerechnet.Core;
using Abgerechnet.Core.Einrichtung;
using Abgerechnet.Core.Model;
using Abgerechnet.Core.Storage;
using Abgerechnet.Core.Vorlagen;

namespace Abgerechnet.Tests;

public class EinrichtungsAblaufTests
{
    [Fact]
    public void NewFolder_GoesThroughAllPages()
    {
        var schritt = EinrichtungsSchritt.Willkommen;
        var besucht = new List<EinrichtungsSchritt> { schritt };
        while (schritt != EinrichtungsSchritt.Fertig)
            besucht.Add(schritt = EinrichtungsAblauf.Weiter(schritt, bestehenderOrdner: false));

        Assert.Equal(Enum.GetValues<EinrichtungsSchritt>(), besucht);
        Assert.Equal(5, EinrichtungsAblauf.Anzahl(false));
        Assert.Equal(4, EinrichtungsAblauf.Nummer(EinrichtungsSchritt.Vorlage, false));
    }

    [Fact]
    public void ExistingFolder_SkipsMeineDatenAndVorlage_BothWays()
    {
        Assert.Equal(EinrichtungsSchritt.Fertig, EinrichtungsAblauf.Weiter(EinrichtungsSchritt.Ordner, bestehenderOrdner: true));
        Assert.Equal(EinrichtungsSchritt.Ordner, EinrichtungsAblauf.Zurueck(EinrichtungsSchritt.Fertig, bestehenderOrdner: true));
        Assert.Equal(EinrichtungsSchritt.Vorlage, EinrichtungsAblauf.Zurueck(EinrichtungsSchritt.Fertig, bestehenderOrdner: false));
        Assert.Equal(EinrichtungsSchritt.Willkommen, EinrichtungsAblauf.Zurueck(EinrichtungsSchritt.Ordner, bestehenderOrdner: true));

        Assert.Equal(3, EinrichtungsAblauf.Anzahl(true));
        Assert.Equal(3, EinrichtungsAblauf.Nummer(EinrichtungsSchritt.Fertig, true));
        Assert.Equal(2, EinrichtungsAblauf.Nummer(EinrichtungsSchritt.Ordner, true));
    }

    [Fact]
    public void ArtDesOrdners_TellsNewEmptyOtherFilesAndInvoiceFolder()
    {
        using var temp = new TempFolder();

        Assert.Equal(OrdnerArt.Neu, EinrichtungsAblauf.ArtDesOrdners(temp.File("gibt-es-nicht")));
        Assert.Equal(OrdnerArt.Leer, EinrichtungsAblauf.ArtDesOrdners(temp.Path));

        temp.CreateFile("Brief.docx", "x");
        Assert.Equal(OrdnerArt.AndereDateien, EinrichtungsAblauf.ArtDesOrdners(temp.Path));

        DataFolder.Open(temp.Path);
        Assert.Equal(OrdnerArt.Rechnungsordner, EinrichtungsAblauf.ArtDesOrdners(temp.Path));
    }

    [Fact]
    public void OrdnerVorschlag_IsRechnungenInDocuments_UnlessThatHoldsOtherFiles()
    {
        using var dokumente = new TempFolder();
        var rechnungen = dokumente.File("Rechnungen");

        Assert.Equal(rechnungen, EinrichtungsAblauf.OrdnerVorschlag(dokumente.Path));

        // An invoice folder of Abgerechnet is suggested again: it is opened.
        Directory.CreateDirectory(rechnungen);
        DataFolder.Open(rechnungen);
        Assert.Equal(rechnungen, EinrichtungsAblauf.OrdnerVorschlag(dokumente.Path));

        // Invoices written by hand: Abgerechnet gets its own folder.
        using var andere = new TempFolder();
        andere.CreateFile(Path.Combine("Rechnungen", "Rechnung 2025-01.pdf"), "x");
        Assert.Equal(andere.File("Rechnungen (Abgerechnet)"), EinrichtungsAblauf.OrdnerVorschlag(andere.Path));
    }
}

public class LocalInstallTests
{
    [Fact]
    public void CanOffer_OnlyForThePublishedExe_NotAlreadyInPlace()
    {
        var root = @"C:\Users\x\AppData\Local\Abgerechnet";

        Assert.True(LocalInstall.CanOffer(@"C:\Users\x\Downloads\Abgerechnet.exe", root));
        Assert.False(LocalInstall.CanOffer(@"C:\Users\x\AppData\Local\Abgerechnet\Abgerechnet.exe", root));
        Assert.False(LocalInstall.CanOffer(@"C:\Projekte\Abgerechnet\bin\Debug\testhost.exe", root));
        Assert.False(LocalInstall.CanOffer(null, root));
    }

    [Fact]
    public void IsTemporaryLocation_DownloadsAndSubfolders_ButNotSimilarNames()
    {
        var temporary = new[] { @"C:\Users\x\Downloads", @"C:\Users\x\Desktop\" };

        Assert.True(LocalInstall.IsTemporaryLocation(@"C:\Users\x\Downloads\Abgerechnet.exe", temporary));
        Assert.True(LocalInstall.IsTemporaryLocation(@"C:\Users\x\Downloads\neu\Abgerechnet.exe", temporary));
        Assert.True(LocalInstall.IsTemporaryLocation(@"C:\Users\x\Desktop\Abgerechnet.exe", temporary));
        Assert.False(LocalInstall.IsTemporaryLocation(@"C:\Users\x\Downloads2\Abgerechnet.exe", temporary));
        Assert.False(LocalInstall.IsTemporaryLocation(@"C:\Tools\Abgerechnet.exe", temporary));
    }

    [Fact]
    public void Copy_CopiesExeAndSettings()
    {
        using var temp = new TempFolder();
        var exe = temp.CreateFile(Path.Combine("Downloads", LocalInstall.ExeName), "exe");
        var settings = temp.CreateFile(Path.Combine("Downloads", SettingsStore.FileName), "{\"dataFolder\":\"D:\\\\Rechnungen\"}");
        var root = temp.File("Local");

        var copy = LocalInstall.Copy(exe, root, settings);

        Assert.Equal(LocalInstall.TargetPath(root), copy);
        Assert.Equal("exe", File.ReadAllText(copy));
        Assert.Equal(File.ReadAllText(settings), File.ReadAllText(Path.Combine(root, SettingsStore.FileName)));
    }
}

public class LogoTests
{
    [Fact]
    public void Uebernehmen_CopiesPng_AndConvertsOtherImages()
    {
        using var temp = new TempFolder();
        var folder = DataFolder.Open(temp.Path);
        Assert.False(Logo.Vorhanden(folder));

        var jpg = temp.File("logo.jpg");
        using (var bitmap = new Bitmap(40, 20))
            bitmap.Save(jpg, ImageFormat.Jpeg);
        Logo.Uebernehmen(folder, jpg);

        Assert.True(Logo.Vorhanden(folder));
        using (var logo = Image.FromFile(Logo.Pfad(folder)))
        {
            Assert.Equal(ImageFormat.Png, logo.RawFormat);
            Assert.Equal(new Size(40, 20), logo.Size);
        }

        var png = temp.CreateFile("anderes.png", "png-bytes");
        Logo.Uebernehmen(folder, png);
        Assert.Equal("png-bytes", File.ReadAllText(Logo.Pfad(folder)));

        Logo.Entfernen(folder);
        Assert.False(Logo.Vorhanden(folder));
        Assert.Empty(Directory.EnumerateFiles(folder.VorlagenPath, "*.tmp"));
    }

    [Fact]
    public void Uebernehmen_NoImage_ThrowsIOException_AndKeepsOldLogo()
    {
        using var temp = new TempFolder();
        var folder = DataFolder.Open(temp.Path);
        Logo.Uebernehmen(folder, temp.CreateFile("alt.png", "alt"));

        Assert.Throws<IOException>(() => Logo.Uebernehmen(folder, temp.CreateFile("kaputt.jpg", "kein Bild")));
        Assert.Equal("alt", File.ReadAllText(Logo.Pfad(folder)));
    }
}

public class BeispielrechnungTests
{
    private static readonly DateOnly Heute = new(2026, 10, 5);

    [Fact]
    public void UsesTheSendersData_AndAnInventedCustomer()
    {
        var einstellungen = new Einstellungen();
        einstellungen.Absender.Firma = "Schmidt IT";
        einstellungen.Absender.Strasse = "Lindenweg 3";
        einstellungen.Absender.Plz = "10115";
        einstellungen.Absender.Ort = "Berlin";

        var daten = Beispielrechnung.Erzeugen(einstellungen, Heute);

        Assert.Equal("Schmidt IT", daten.Einstellungen.Absender.Firma);
        Assert.Equal("Nordlicht GmbH", daten.Empfaenger!.Firma);
        Assert.Equal(2, daten.Rechnung.Positionen.Count);
        Assert.Equal("September 2026", daten.Rechnung.Zeitraum);
        Assert.Equal(string.Empty, einstellungen.Bank.Iban); // the settings themselves stay unchanged
    }

    [Fact]
    public void WithoutSenderData_ShowsAnObviouslyInventedSender()
    {
        var daten = Beispielrechnung.Erzeugen(new Einstellungen(), Heute);

        Assert.Equal("Ihre Firma", daten.Einstellungen.Absender.Firma);
        Assert.True(daten.Einstellungen.Absender.IstVollstaendig);
        Assert.NotEmpty(daten.Einstellungen.Bank.Iban);
    }

    public static TheoryData<string> Mitgeliefert() => new(MitgelieferteVorlagen.Namen);

    [Theory]
    [MemberData(nameof(Mitgeliefert))]
    public void FillsEveryBuiltInTemplate(string name)
    {
        var ergebnis = Vorlage.Ausfuellen(MitgelieferteVorlagen.Lesen(name), Beispielrechnung.Erzeugen(new Einstellungen(), Heute), (string?)null);

        Assert.Empty(ergebnis.UnbekanntePlatzhalter);
        Assert.Contains("Nordlicht GmbH", ergebnis.Html);
        Assert.Contains("Ihre Firma", ergebnis.Html);
    }
}
