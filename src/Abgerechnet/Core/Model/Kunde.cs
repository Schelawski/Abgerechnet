namespace Abgerechnet.Core.Model;

/// <summary>
/// A customer in <c>kunden.json</c>: just enough for the invoice address (issue #4).
/// </summary>
public sealed class Kunde
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Firma { get; set; } = string.Empty;

    /// <summary>Short name used in the PDF file name, e.g. "Mueller".</summary>
    public string Kurzname { get; set; } = string.Empty;

    public string Strasse { get; set; } = string.Empty;

    public string Plz { get; set; } = string.Empty;

    public string Ort { get; set; } = string.Empty;

    public string Land { get; set; } = string.Empty;

    public string Ansprechpartner { get; set; } = string.Empty;

    /// <summary>VAT ID of the customer (needed later for e-invoices, issue #12).</summary>
    public string UstIdNr { get; set; } = string.Empty;

    internal void Normalize()
    {
        if (Id == Guid.Empty)
            Id = Guid.NewGuid();
        Firma ??= string.Empty;
        Kurzname ??= string.Empty;
        Strasse ??= string.Empty;
        Plz ??= string.Empty;
        Ort ??= string.Empty;
        Land ??= string.Empty;
        Ansprechpartner ??= string.Empty;
        UstIdNr ??= string.Empty;
    }
}
