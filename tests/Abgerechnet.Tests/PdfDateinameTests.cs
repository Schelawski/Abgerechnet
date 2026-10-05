using Abgerechnet.Core.Model;

namespace Abgerechnet.Tests;

public class PdfDateinameTests
{
    private static readonly DateOnly Datum = new(2026, 10, 1);
    private static readonly Kunde Nordlicht = new() { Firma = "Grünwald & Söhne GmbH", Kurzname = "Nordlicht" };

    [Fact]
    public void DefaultPattern()
    {
        Assert.Equal("111412_Nordlicht_2026-10-01.pdf", PdfDateiname.Erzeugen(PdfDateiname.DefaultMuster, "111412", Datum, Nordlicht));
    }

    [Fact]
    public void AllPlaceholders()
    {
        var name = PdfDateiname.Erzeugen("{jahr}-{monat} Rechnung {nummer} {kunde}.pdf", "7", Datum, Nordlicht);

        Assert.Equal("2026-10 Rechnung 7 Grünwald & Söhne GmbH.pdf", name);
    }

    [Fact]
    public void PlaceholdersIgnoreCase_AndPdfIsAppended()
    {
        Assert.Equal("111412_Nordlicht.pdf", PdfDateiname.Erzeugen("{Nummer}_{KUNDE_KURZNAME}", "111412", Datum, Nordlicht));
    }

    [Fact]
    public void InvalidCharacters_AreReplaced()
    {
        var kunde = new Kunde { Kurzname = "A/B: \"C\"" };

        Assert.Equal("RE_2026_1_A_B_ _C.pdf", PdfDateiname.Erzeugen("{nummer}_{kunde_kurzname}", "RE/2026/1", Datum, kunde));
    }

    [Fact]
    public void WithoutShortName_TheCompanyIsUsed()
    {
        var kunde = new Kunde { Firma = "Beispiel AG" };

        Assert.Equal("5_Beispiel AG_2026-10-01.pdf", PdfDateiname.Erzeugen(PdfDateiname.DefaultMuster, "5", Datum, kunde));
    }

    [Fact]
    public void WithoutCustomer_NoDoubleUnderscores()
    {
        Assert.Equal("5_2026-10-01.pdf", PdfDateiname.Erzeugen(PdfDateiname.DefaultMuster, "5", Datum, kunde: null));
    }

    [Fact]
    public void EmptyResult_GetsAName()
    {
        Assert.Equal("Rechnung.pdf", PdfDateiname.Erzeugen("{kunde}", "", Datum, kunde: null));
    }

    [Fact]
    public void UnknownPlaceholders_AreFound()
    {
        Assert.Equal(["{kundenname}", "{tag}"], PdfDateiname.UnbekanntePlatzhalter("{nummer}_{kundenname}_{tag}_{tag}.pdf"));
        Assert.Empty(PdfDateiname.UnbekanntePlatzhalter(PdfDateiname.DefaultMuster));
        Assert.Empty(PdfDateiname.UnbekanntePlatzhalter("{NUMMER}.pdf"));
    }
}
