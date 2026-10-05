using Abgerechnet.Core.Model;
using Abgerechnet.Core.Storage;
using Abgerechnet.Core.Vorlagen;

namespace Abgerechnet.Tests;

public class VorlagenTests
{
    public static TheoryData<string> Mitgeliefert() => new(MitgelieferteVorlagen.Namen);

    [Theory]
    [MemberData(nameof(Mitgeliefert))]
    public void BuiltInTemplates_UseOnlyKnownPlaceholders_AndAreA4(string name)
    {
        var html = MitgelieferteVorlagen.Lesen(name);
        foreach (var klein in new[] { false, true })
        {
            var daten = new RechnungsDaten(new Einstellungen(), new Rechnung { Kleinunternehmer = klein, Positionen = [new Position()] }, new Kunde());

            var ergebnis = Vorlage.Ausfuellen(html, daten, new Uri("https://vorlage.abgerechnet.example/"));

            Assert.Empty(ergebnis.UnbekanntePlatzhalter);
            Assert.Contains("<table class=\"positionen\">", ergebnis.Html);
            Assert.Contains("<table class=\"summen\">", ergebnis.Html);
            Assert.DoesNotContain("{{", ergebnis.Html.Split("-->").Last()); // nothing left outside the comments
        }
        Assert.Contains("size: A4", html);
        Assert.Contains("class=\"seitenrahmen\"", html);   // footer space on every page
        Assert.Contains("src=\"logo.png\"", html);
        Assert.Contains("{{steuerhinweis}}", html);
    }

    [Fact]
    public void BuiltInTemplates_ContainNoRealPersonalData()
    {
        foreach (var name in MitgelieferteVorlagen.Namen)
        {
            var html = MitgelieferteVorlagen.Lesen(name);
            Assert.DoesNotMatch(@"DE\d{2}[ \d]{16,}", html); // no IBAN
            Assert.DoesNotContain("@", html.Replace("@page", string.Empty, StringComparison.Ordinal));
        }
    }

    [Fact]
    public void Lesen_UnknownTemplate_Throws()
    {
        Assert.Throws<ArgumentException>(() => MitgelieferteVorlagen.Lesen("gibt-es-nicht"));
    }

    [Fact]
    public void NewFolder_GetsTheTemplates_ExistingFilesStay()
    {
        using var temp = new TempFolder();

        DataFolder.Open(temp.Path);

        foreach (var name in MitgelieferteVorlagen.Namen)
            Assert.Equal(MitgelieferteVorlagen.Lesen(name), File.ReadAllText(temp.File(Path.Combine("Vorlagen", name + ".html"))));
        Assert.False(File.Exists(temp.File(Path.Combine("Vorlagen", "logo.png"))));

        // Changed and deleted templates are not touched when the folder is opened again.
        File.WriteAllText(temp.File(Path.Combine("Vorlagen", "klassisch.html")), "<p>eigene</p>");
        File.Delete(temp.File(Path.Combine("Vorlagen", "modern.html")));
        DataFolder.Open(temp.Path);
        Assert.Equal("<p>eigene</p>", File.ReadAllText(temp.File(Path.Combine("Vorlagen", "klassisch.html"))));
        Assert.False(File.Exists(temp.File(Path.Combine("Vorlagen", "modern.html"))));
    }

    [Fact]
    public void FolderWithOwnTemplate_GetsNoCopies()
    {
        using var temp = new TempFolder();
        temp.CreateFile(Path.Combine("Vorlagen", "meine.html"), "<p>x</p>");

        DataFolder.Open(temp.Path);

        Assert.Equal(["meine.html"], Directory.GetFiles(temp.File("Vorlagen")).Select(Path.GetFileName));
    }

    [Fact]
    public void Laden_PrefersTheFile_FallsBackToBuiltIn_ThenToStandard()
    {
        using var temp = new TempFolder();
        var folder = DataFolder.Open(temp.Path);
        File.WriteAllText(temp.File(Path.Combine("Vorlagen", "modern.html")), "<p>angepasst</p>");
        File.Delete(temp.File(Path.Combine("Vorlagen", "schlicht.html")));

        var datei = MitgelieferteVorlagen.Laden(folder, "modern");
        Assert.Equal("<p>angepasst</p>", datei.Html);
        Assert.NotNull(datei.Datei);

        var eingebaut = MitgelieferteVorlagen.Laden(folder, "schlicht");
        Assert.Null(eingebaut.Datei);
        Assert.False(eingebaut.Ersatz);
        Assert.Equal(MitgelieferteVorlagen.Lesen("schlicht"), eingebaut.Html);

        var ersatz = MitgelieferteVorlagen.Laden(folder, "geloeschte-eigene");
        Assert.True(ersatz.Ersatz);
        Assert.Equal(MitgelieferteVorlagen.Standard, ersatz.Name);
    }

    [Fact]
    public void Verfuegbare_BuiltInFirst_ThenOwn_WithoutBackups()
    {
        using var temp = new TempFolder();
        var folder = DataFolder.Open(temp.Path);
        temp.CreateFile(Path.Combine("Vorlagen", "zebra.html"), "x");
        temp.CreateFile(Path.Combine("Vorlagen", "Angebot.html"), "x");
        temp.CreateFile(Path.Combine("Vorlagen", "klassisch.bak.html"), "x");
        temp.CreateFile(Path.Combine("Vorlagen", "notizen.txt"), "x");

        Assert.Equal(["klassisch", "modern", "schlicht", "Angebot", "zebra"], MitgelieferteVorlagen.Verfuegbare(folder));
    }

    [Fact]
    public void Wiederherstellen_KeepsTheChangedCopy()
    {
        using var temp = new TempFolder();
        var folder = DataFolder.Open(temp.Path);
        var datei = temp.File(Path.Combine("Vorlagen", "klassisch.html"));
        File.WriteAllText(datei, "<p>meine Änderung</p>");

        var sicherung = MitgelieferteVorlagen.Wiederherstellen(folder, "klassisch");

        Assert.Equal(MitgelieferteVorlagen.Lesen("klassisch"), File.ReadAllText(datei));
        Assert.Equal(temp.File(Path.Combine("Vorlagen", "klassisch.bak.html")), sicherung);
        Assert.Equal("<p>meine Änderung</p>", File.ReadAllText(sicherung!));

        // Unchanged or missing: nothing to keep.
        Assert.Null(MitgelieferteVorlagen.Wiederherstellen(folder, "klassisch"));
        File.Delete(temp.File(Path.Combine("Vorlagen", "modern.html")));
        Assert.Null(MitgelieferteVorlagen.Wiederherstellen(folder, "modern"));
        Assert.True(File.Exists(temp.File(Path.Combine("Vorlagen", "modern.html"))));
    }

    [Fact]
    public void NameFuer_InvoiceChoiceOverStandard()
    {
        var einstellungen = new Einstellungen { Rechnung = { Vorlage = "modern" } };

        Assert.Equal("modern", MitgelieferteVorlagen.NameFuer(new Rechnung(), einstellungen));
        Assert.Equal("schlicht", MitgelieferteVorlagen.NameFuer(new Rechnung { Vorlage = "schlicht" }, einstellungen));
        Assert.Equal("klassisch", new Einstellungen().Rechnung.Vorlage);
    }

    [Fact]
    public void Kopie_KeepsTheTemplateChoice()
    {
        var alt = new Rechnung { Nummer = "1", Vorlage = "modern" };

        Assert.Equal("modern", alt.AlsNeueRechnung(new Einstellungen(), [alt], new DateOnly(2026, 10, 5)).Vorlage);
    }
}
