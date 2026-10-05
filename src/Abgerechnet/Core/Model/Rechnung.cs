namespace Abgerechnet.Core.Model;

/// <summary>
/// Lifecycle of an invoice. Stored in lower case ("entwurf", "offen", …).
/// </summary>
public enum RechnungsStatus
{
    /// <summary>Being written; may still be deleted.</summary>
    Entwurf,

    /// <summary>PDF created and sent; waiting for payment.</summary>
    Offen,

    Bezahlt,

    /// <summary>Cancelled. Issued invoices are cancelled instead of deleted, so the number sequence has no gaps.</summary>
    Storniert,
}

/// <summary>
/// One invoice in <c>rechnungen.json</c>. The domain terms are German because they appear as they are in the
/// JSON files and in the template placeholders.
/// </summary>
public sealed class Rechnung
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Invoice number, e.g. "111412". Text, so leading zeros and prefixes are possible.</summary>
    public string Nummer { get; set; } = string.Empty;

    /// <summary>Invoice date.</summary>
    public DateOnly Datum { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    /// <summary>Service period as text, e.g. "September 2026".</summary>
    public string Zeitraum { get; set; } = string.Empty;

    /// <summary>Project or subject line.</summary>
    public string Projekt { get; set; } = string.Empty;

    public Guid? KundeId { get; set; }

    /// <summary>
    /// Copy of the customer's address taken when the PDF is created, so later changes to the customer do not
    /// change issued invoices. <c>null</c> for drafts.
    /// </summary>
    public Kunde? Empfaenger { get; set; }

    public RechnungsStatus Status { get; set; } = RechnungsStatus.Entwurf;

    /// <summary>Date the payment arrived; set together with <see cref="RechnungsStatus.Bezahlt"/>.</summary>
    public DateOnly? BezahltAm { get; set; }

    /// <summary>File name of the created PDF, relative to the <c>PDF</c> folder.</summary>
    public string? PdfDatei { get; set; }

    public List<Position> Positionen { get; set; } = [];

    /// <summary>Replaces missing values (e.g. from an older or hand-edited file) with defaults.</summary>
    internal void Normalize()
    {
        if (Id == Guid.Empty)
            Id = Guid.NewGuid();
        Nummer ??= string.Empty;
        Zeitraum ??= string.Empty;
        Projekt ??= string.Empty;
        if (!Enum.IsDefined(Status))
            Status = RechnungsStatus.Entwurf;
        Empfaenger?.Normalize();
        Positionen = (Positionen ?? []).Where(p => p is not null).ToList();
        foreach (var position in Positionen)
            position.Normalize();
    }
}

/// <summary>One line item of an invoice.</summary>
public sealed class Position
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Own service period; empty means the period of the invoice.</summary>
    public string Zeitraum { get; set; } = string.Empty;

    public string Beschreibung { get; set; } = string.Empty;

    /// <summary>Optional second line with details.</summary>
    public string Detail { get; set; } = string.Empty;

    public decimal Menge { get; set; }

    /// <summary>Unit such as "Std.", "Tage" or "pauschal".</summary>
    public string Einheit { get; set; } = string.Empty;

    /// <summary>Net price per unit.</summary>
    public decimal Einzelpreis { get; set; }

    internal void Normalize()
    {
        if (Id == Guid.Empty)
            Id = Guid.NewGuid();
        Zeitraum ??= string.Empty;
        Beschreibung ??= string.Empty;
        Detail ??= string.Empty;
        Einheit ??= string.Empty;
    }
}
