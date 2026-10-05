using Abgerechnet.Core.Model;
using Abgerechnet.Core.Storage;
using Abgerechnet.Core.Vorlagen;

namespace Abgerechnet.Tests;

public class PdfTests
{
    [Fact]
    public void PdfErzeugt_DraftBecomesOpen_AndTheAddressIsFrozen()
    {
        var kunde = new Kunde { Firma = "Nordlicht GmbH", Ort = "Hamburg" };
        var rechnung = new Rechnung { KundeId = kunde.Id };

        rechnung.PdfErzeugt("2026-001_Nordlicht_2026-10-05.pdf", kunde);
        kunde.Ort = "Bremen";

        Assert.Equal(RechnungsStatus.Offen, rechnung.Status);
        Assert.Equal("2026-001_Nordlicht_2026-10-05.pdf", rechnung.PdfDatei);
        Assert.Equal("Hamburg", rechnung.Empfaenger!.Ort);
    }

    [Theory]
    [InlineData(RechnungsStatus.Offen)]
    [InlineData(RechnungsStatus.Bezahlt)]
    [InlineData(RechnungsStatus.Storniert)]
    public void PdfErzeugt_AgainKeepsTheStatus(RechnungsStatus status)
    {
        var rechnung = new Rechnung { Status = status };

        rechnung.PdfErzeugt("neu.pdf", empfaenger: null);

        Assert.Equal(status, rechnung.Status);
        Assert.Equal("neu.pdf", rechnung.PdfDatei);
        Assert.Null(rechnung.Empfaenger);
    }
}
