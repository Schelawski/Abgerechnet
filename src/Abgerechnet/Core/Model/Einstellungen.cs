using System.Text.Json.Serialization;

namespace Abgerechnet.Core.Model;

/// <summary>
/// Content of <c>abgerechnet.json</c>: the sender's data and the invoice settings (issue #3). The sections are
/// also the prefixes of the template placeholders, e.g. <c>{{absender_firma}}</c> or <c>{{bank_iban}}</c>.
/// </summary>
public sealed class Einstellungen : IDataFile
{
    public int Version { get; set; }

    public Absender Absender { get; set; } = new();

    public Bankverbindung Bank { get; set; } = new();

    public RechnungsEinstellungen Rechnung { get; set; } = new();

    /// <summary>Default values for a new line item.</summary>
    public PositionsVorgaben NeuePosition { get; set; } = new();

    public void Normalize()
    {
        Absender ??= new();
        Bank ??= new();
        Rechnung ??= new();
        NeuePosition ??= new();
        Absender.Normalize();
        Bank.Normalize();
        Rechnung.Normalize();
        NeuePosition.Normalize();
    }
}

/// <summary>The sender of the invoices: the user.</summary>
public sealed class Absender
{
    public string Firma { get; set; } = string.Empty;

    /// <summary>Line under the company name, e.g. "Softwareentwicklung im .NET-Umfeld".</summary>
    public string Unterzeile { get; set; } = string.Empty;

    /// <summary>Name of the person.</summary>
    public string Name { get; set; } = string.Empty;

    public string Strasse { get; set; } = string.Empty;

    public string Plz { get; set; } = string.Empty;

    public string Ort { get; set; } = string.Empty;

    public string Land { get; set; } = string.Empty;

    public string Telefon { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Website { get; set; } = string.Empty;

    public string Steuernummer { get; set; } = string.Empty;

    public string UstIdNr { get; set; } = string.Empty;

    /// <summary>Company, otherwise the person's name; empty when neither is set.</summary>
    [JsonIgnore]
    public string Anzeigename => !string.IsNullOrWhiteSpace(Firma) ? Firma.Trim() : Name.Trim();

    /// <summary>True when company or name and the full address are filled in (the minimum for an invoice).</summary>
    [JsonIgnore]
    public bool IstVollstaendig =>
        Anzeigename.Length > 0
        && !string.IsNullOrWhiteSpace(Strasse) && !string.IsNullOrWhiteSpace(Plz) && !string.IsNullOrWhiteSpace(Ort);

    internal void Normalize()
    {
        Firma ??= string.Empty;
        Unterzeile ??= string.Empty;
        Name ??= string.Empty;
        Strasse ??= string.Empty;
        Plz ??= string.Empty;
        Ort ??= string.Empty;
        Land ??= string.Empty;
        Telefon ??= string.Empty;
        Email ??= string.Empty;
        Website ??= string.Empty;
        Steuernummer ??= string.Empty;
        UstIdNr ??= string.Empty;
    }
}

public sealed class Bankverbindung
{
    public string Kontoinhaber { get; set; } = string.Empty;

    public string Bank { get; set; } = string.Empty;

    /// <summary>IBAN as entered (usually in groups of four); see <see cref="Iban"/>.</summary>
    public string Iban { get; set; } = string.Empty;

    public string Bic { get; set; } = string.Empty;

    internal void Normalize()
    {
        Kontoinhaber ??= string.Empty;
        Bank ??= string.Empty;
        Iban ??= string.Empty;
        Bic ??= string.Empty;
    }
}

public sealed class RechnungsEinstellungen
{
    public const decimal DefaultUmsatzsteuersatz = 19m;
    public const int DefaultZahlungszielTage = 30;
    public const string DefaultKleinunternehmerHinweis = "Gemäß § 19 UStG wird keine Umsatzsteuer berechnet.";

    /// <summary>Small business according to § 19 UStG: no VAT, a note on the invoice instead.</summary>
    public bool Kleinunternehmer { get; set; }

    /// <summary>The note printed on invoices of a small business.</summary>
    public string KleinunternehmerHinweis { get; set; } = DefaultKleinunternehmerHinweis;

    /// <summary>VAT rate in percent, e.g. 19.</summary>
    public decimal Umsatzsteuersatz { get; set; } = DefaultUmsatzsteuersatz;

    /// <summary>Days until payment is due (for <c>{{faellig_am}}</c> and the reminder in #17); 0 = none.</summary>
    public int ZahlungszielTage { get; set; } = DefaultZahlungszielTage;

    /// <summary>
    /// Number for the next invoice, e.g. "111401" or "RE-2026-001". Empty: the highest existing number plus one.
    /// </summary>
    public string NaechsteNummer { get; set; } = string.Empty;

    /// <summary>Pattern for the PDF file name; see <see cref="PdfDateiname"/>.</summary>
    public string PdfDateiname { get; set; } = Model.PdfDateiname.DefaultMuster;

    internal void Normalize()
    {
        KleinunternehmerHinweis ??= string.Empty;
        if (Umsatzsteuersatz is < 0 or > 100)
            Umsatzsteuersatz = DefaultUmsatzsteuersatz;
        if (ZahlungszielTage < 0)
            ZahlungszielTage = 0;
        NaechsteNummer ??= string.Empty;
        if (string.IsNullOrWhiteSpace(PdfDateiname))
            PdfDateiname = Model.PdfDateiname.DefaultMuster;
    }
}

public sealed class PositionsVorgaben
{
    public const string DefaultEinheit = "Std.";

    public string Beschreibung { get; set; } = string.Empty;

    public string Einheit { get; set; } = DefaultEinheit;

    /// <summary>Net price per unit.</summary>
    public decimal Einzelpreis { get; set; }

    internal void Normalize()
    {
        Beschreibung ??= string.Empty;
        Einheit ??= string.Empty;
        if (Einzelpreis < 0)
            Einzelpreis = 0;
    }
}
