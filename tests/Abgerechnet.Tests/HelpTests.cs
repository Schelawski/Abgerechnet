using Abgerechnet.Core.Help;
using Abgerechnet.Core.Vorlagen;
using Abgerechnet.UI;

namespace Abgerechnet.Tests;

public class HelpDocumentTests
{
    [Fact]
    public void TopicsParagraphsListsAndQuotesAreParsed()
    {
        var document = HelpDocument.Parse("""
            Note for editors, ignored.

            # about | Was ist Abgerechnet?

            Erste Zeile
            zweite Zeile.

            ## Unterpunkt
            - eins
              weiter
            - zwei
            12. zwölf

            > Zeile 1
            >
            >   eingerückt
            Danach.

            # start | Erste Schritte
            Text.
            """);

        Assert.Equal(["about", "start"], document.Topics.Select(t => t.Id));
        var about = document.Topics[0];
        Assert.Equal("Was ist Abgerechnet?", about.Title);
        Assert.Collection(
            about.Blocks,
            block => Assert.Equal("Erste Zeile zweite Zeile.", Assert.IsType<HelpParagraph>(block).Spans.Single().Text),
            block => Assert.Equal("Unterpunkt", Assert.IsType<HelpHeading>(block).Spans.Single().Text),
            block => Assert.Equal(("•", "eins weiter"), Item(block)),
            block => Assert.Equal(("•", "zwei"), Item(block)),
            block => Assert.Equal(("12.", "zwölf"), Item(block)),
            block => Assert.Equal("Zeile 1\n\n  eingerückt", Assert.IsType<HelpQuote>(block).Spans.Single().Text),
            block => Assert.Equal("Danach.", Assert.IsType<HelpParagraph>(block).Spans.Single().Text));
        Assert.Equal("Text.", Assert.Single(document.Topics[1].Blocks).Spans.Single().Text);
    }

    private static (string, string) Item(HelpBlock block)
    {
        var item = Assert.IsType<HelpListItem>(block);
        return (item.Marker, string.Concat(item.Spans.Select(s => s.Text)));
    }

    [Fact]
    public void BoldAndCodeAreRecognized()
    {
        var spans = HelpDocument.ParseInline("Ordner `Vorlagen` **öffnen**, fertig.");

        Assert.Equal(
            [
                new HelpSpan("Ordner ", HelpSpanStyle.Normal),
                new HelpSpan("Vorlagen", HelpSpanStyle.Code),
                new HelpSpan(" ", HelpSpanStyle.Normal),
                new HelpSpan("öffnen", HelpSpanStyle.Bold),
                new HelpSpan(", fertig.", HelpSpanStyle.Normal),
            ],
            spans);
    }

    [Theory]
    [InlineData("2 ** 3")]
    [InlineData("a `b")]
    public void UnclosedMarkersStayText(string text)
    {
        var span = Assert.Single(HelpDocument.ParseInline(text));
        Assert.Equal(new HelpSpan(text, HelpSpanStyle.Normal), span);
    }

    [Fact]
    public void FindIgnoresCaseAndUnknownIds()
    {
        var document = HelpDocument.Parse("# about | A\nx\n# start | B\ny");

        Assert.Equal("B", document.Find("START")?.Title);
        Assert.Null(document.Find("nope"));
        Assert.Null(document.Find(null));
    }
}

/// <summary>The real help text in help.de.md.</summary>
public class EmbeddedHelpTests
{
    private static readonly HelpDocument Hilfe = HelpDocument.Load();

    private static string Text(HelpTopic topic) =>
        topic.Title + "\n" + string.Join("\n", topic.Blocks.Select(b => string.Concat(b.Spans.Select(s => s.Text))));

    private static string AllText => string.Join("\n", Hilfe.Topics.Select(Text));

    [Fact]
    public void HasAllTopicsInOrder()
    {
        Assert.Equal(HelpTopics.All, Hilfe.Topics.Select(t => t.Id));
        Assert.All(Hilfe.Topics, topic =>
        {
            Assert.False(string.IsNullOrWhiteSpace(topic.Title));
            Assert.True(topic.Blocks.Count >= 3, $"Topic {topic.Id} is (almost) empty.");
        });
    }

    [Fact]
    public void HasNoBrokenMarkup()
    {
        var texts = Hilfe.Topics
            .SelectMany(topic => topic.Blocks.Where(b => b is not HelpQuote))
            .SelectMany(block => block.Spans)
            .ToList();

        Assert.All(texts, span =>
        {
            Assert.DoesNotContain("**", span.Text);
            Assert.DoesNotContain("`", span.Text);
            Assert.DoesNotContain("[[", span.Text); // a marker that was not replaced
            // Code such as a colour "#2563eb" may start with "#".
            Assert.False(span.Style != HelpSpanStyle.Code && span.Text.TrimStart().StartsWith('#'), $"Stray heading: {span.Text}");
        });
    }

    [Fact]
    public void FirstTopicExplainsThatDataStaysOnTheComputer()
    {
        var about = Text(Hilfe.Topics[0]);

        Assert.Contains("Ihre Daten bleiben bei Ihnen", about);
        Assert.Contains("Internet", about);
        Assert.Contains("Bewusst einfach", about);
    }

    [Fact]
    public void ListsEveryPlaceholderOfTheTemplates()
    {
        var anpassen = Text(Hilfe.Find(HelpTopics.VorlageAnpassen)!);

        Assert.All(Platzhalter.Alle, p => Assert.Contains(p.Schreibweise + " – " + p.Beschreibung, anpassen));
    }

    [Fact]
    public void KiTopicShowsThePrompt()
    {
        var ki = Hilfe.Find(HelpTopics.Ki)!;
        var quote = Assert.Single(ki.Blocks.OfType<HelpQuote>());
        var prompt = quote.Spans.Single().Text;

        Assert.Equal(KiPrompt.Text(KiPrompt.VorlagePlatzhalter).ReplaceLineEndings("\n"), prompt);
        Assert.Contains(KiPrompt.WunschPlatzhalter, Text(ki)); // step 3 refers to it
    }

    [Fact]
    public void NamesTheButtonsOfTheUi()
    {
        static string Bare(string text) => text.Replace("&", string.Empty, StringComparison.Ordinal);

        string[] names =
        [
            UiText.MeineDatenButton,
            "Datei → " + Bare(UiText.MenuMeineDaten),
            "Datei → " + Bare(UiText.MenuKunden),
            "Datei → " + Bare(UiText.MenuChangeFolder),
            "Datei → " + Bare(UiText.MenuOpenFolder),
            UiText.KundenVerwalten,
            UiText.NeueRechnungButton,
            UiText.PositionHinzufuegen,
            Bare(UiText.PdfErzeugenButton),
            Bare(UiText.PdfSpeichern),
            UiText.PdfOeffnenButton,
            UiText.ImOrdnerZeigenButton,
            Bare(UiText.MenuKopieren),
            Bare(UiText.MenuAlsBezahlt),
            Bare(UiText.ZahlungseingangButton),
            Bare(UiText.AlsBezahltSpeichern),
            Bare(UiText.AlleMarkieren),
            UiText.StatusUeberfaellig,
            UiText.SpalteBezahltAm,
            Bare(UiText.MenuStatusAendern) + " → " + UiText.StatusName(Core.Model.RechnungsStatus.Storniert),
            UiText.Bearbeiten,
            "Vorlagen → " + Bare(UiText.MenuVorlagenOrdner),
            "Vorlagen → " + Bare(UiText.MenuOriginalWiederherstellen),
            UiText.PromptKopieren,
            UiText.TabRechnungen,
            UiText.TabSteuerBank,
            "Datei → " + Bare(UiText.MenuEinrichtung),
            Bare(UiText.MeineDatenSpaeter),
            Bare(UiText.ErsteRechnung),
        ];
        var help = AllText;
        Assert.All(names, name => Assert.Contains(name, help));
    }

    [Fact]
    public void TemplateNamesMatch()
    {
        var vorlagen = Text(Hilfe.Find(HelpTopics.Vorlagen)!);

        Assert.All(MitgelieferteVorlagen.Namen, name => Assert.Contains(UiText.VorlagenName(name), vorlagen));
    }
}

public class KiPromptTests
{
    [Fact]
    public void PromptKeepsTheRulesAndEndsWithTheTemplate()
    {
        var prompt = KiPrompt.Text("<html>meine Vorlage</html>");

        Assert.Contains("{{rechnung_nummer}}", prompt);
        Assert.Contains("seitenrahmen", prompt);
        Assert.Contains("wenn-gefuellt", prompt);
        Assert.Contains("@page", prompt);
        Assert.Contains("Internet", prompt);
        Assert.Contains(KiPrompt.WunschPlatzhalter, prompt);
        Assert.EndsWith("<html>meine Vorlage</html>", prompt);
    }
}
